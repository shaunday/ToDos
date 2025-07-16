using DotNetEnv;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ToDos.Entities;
using ToDos.Server.DbSharding;
using ToDos.Server.DbReplication;

namespace ToDos.Repository
{
    public class TaskRepository : ITaskRepository, IDisposable
    {
        #region Fields
        private readonly IShardResolver _shardResolver;
        private readonly IReadWriteDbRouter _dbRouter;
        private readonly ILogger _logger;
        private readonly IDbSyncService _dbSyncService;
        private System.Threading.Timer _syncTimer;
        #endregion

        #region Constructor
        public TaskRepository(IShardResolver shardResolver, IReadWriteDbRouter dbRouter, IDbSyncService dbSyncService, ILogger logger)
        {
            _shardResolver = shardResolver;
            _dbRouter = dbRouter;
            _dbSyncService = dbSyncService;
            _logger = logger;

            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Repository");
            Env.Load(envPath);
        }
        #endregion

        #region Private Helpers
        private string GetConnectionString(int userId, bool isWriteOperation)
        {
            var shardedDbName = _shardResolver.GetDatabaseName(userId);
            var readWriteSwitchDbName = _dbRouter.GetPhysicalDbName(shardedDbName, isWriteOperation);
            return ConnectionStringAccess.GetDbConnectionString(); //using empty instead of passing (readWriteSwitchDbName)
        }
        #endregion

        #region CRUD Methods
        public async Task<IEnumerable<ToDos.Entities.TaskEntity>> GetByUserIdAsync(int userId)
        {
            try
            {
                var connStr = GetConnectionString(userId, false);
                using var context = new TaskDbContext(connStr);
                return await context.Tasks
                    .Include(t => t.Tags)
                    .Where(t => t.UserId == userId)
                    .ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in GetByUserIdAsync for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<ToDos.Entities.TaskEntity> GetByIdAsync(int userId, int id)
        {
            try
            {
                var connStr = GetConnectionString(userId, false);
                using var context = new TaskDbContext(connStr);
                return await context.Tasks
                    .Include(t => t.Tags)
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in GetByIdAsync");
                throw;
            }
        }

        public async Task AddAsync(ToDos.Entities.TaskEntity task)
        {
            try
            {
                var connStr = GetConnectionString(task.UserId, true);
                using var context = new TaskDbContext(connStr);
                context.Tasks.Add(task);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in AddAsync");
                throw;
            }
        }

        public async Task UpdateAsync(ToDos.Entities.TaskEntity task)
        {
            try
            {
                var connStr = GetConnectionString(task.UserId, true);
                using var context = new TaskDbContext(connStr);
                var existing = await context.Tasks.FindAsync(task.Id).ConfigureAwait(false);
                if (existing != null)
                {
                    context.Entry(existing).CurrentValues.SetValues(task);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in UpdateAsync");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int userId, int taskId)
        {
            try
            {
                var connStr = GetConnectionString(userId, true);
                using var context = new TaskDbContext(connStr);
                var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId).ConfigureAwait(false);
                if (task == null)
                    return false;

                context.Tasks.Remove(task);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in DeleteAsync");
                throw;
            }
        }

        public async Task<bool> SetCompletionAsync(int userId, int taskId, bool isCompleted)
        {
            try
            {
                var connStr = GetConnectionString(userId, true);
                using var context = new TaskDbContext(connStr);
                var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId).ConfigureAwait(false);
                if (task == null)
                    return false;

                task.IsCompleted = isCompleted;
                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in SetCompletionAsync");
                throw;
            }
        }
        #endregion

        #region Task Lock Methods
        public async Task<bool> LockTaskAsync(int userId, int id)
        {
            try
            {
                var connStr = GetConnectionString(userId, true);
                using var context = new TaskDbContext(connStr);
                var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId).ConfigureAwait(false);
                if (task == null || task.IsLocked)
                    return false;

                task.IsLocked = true;
                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in LockTaskAsync");
                throw;
            }
        }

        public async Task<bool> UnlockTaskAsync(int userId, int id)
        {
            try
            {
                var connStr = GetConnectionString(userId, true);
                using var context = new TaskDbContext(connStr);
                var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId).ConfigureAwait(false);
                if (task == null || !task.IsLocked)
                    return false;

                task.IsLocked = false;
                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in UnlockTaskAsync");
                throw;
            }
        }

        public async Task<bool> IsTaskLockedAsync(int userId, int id)
        {
            try
            {
                var connStr = GetConnectionString(userId, false);
                using var context = new TaskDbContext(connStr);
                var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId).ConfigureAwait(false);
                return task?.IsLocked ?? false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in IsTaskLockedAsync");
                throw;
            }
        }
        #endregion

        #region Static Utilities
        /// <summary>
        /// Clears all tasks and populates the DB with mock data for the given user IDs.
        /// </summary>
        public static void ResetAndPopulateDb(string connectionString, IEnumerable<int> userIds, int tasksPerUser = 5)
        {
            using var context = new TaskDbContext(connectionString);
            context.Tasks.RemoveRange(context.Tasks);
            context.SaveChanges();

            var mockTasks = ToDos.Server.Entities.Factory.TaskFactory.GenerateTasksForUsers(userIds, tasksPerUser);

            // Collect all unique tags from mockTasks
            var allTagNames = mockTasks.SelectMany(t => t.Tags.Select(tag => tag.Name)).Distinct().ToList();
            var tagEntities = allTagNames.Select(name => new ToDos.Entities.TagEntity { Id = Guid.NewGuid(), Name = name }).ToList();
            context.Tags.AddRange(tagEntities);
            context.SaveChanges();

            // Map TagDTOs in mockTasks to TagEntities
            var tagEntityDict = tagEntities.ToDictionary(te => te.Name, te => te);

            context.Tasks.AddRange(mockTasks.Select(t => new ToDos.Entities.TaskEntity
            {
                UserId = t.UserId,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                Priority = t.Priority,
                Tags = t.Tags.Select(tagDto => tagEntityDict[tagDto.Name]).ToList()
            }));
            context.SaveChanges();
        }
        #endregion

        #region Sync Methods
        /// <summary>
        /// Triggers a sync for all logical DBs (if supported by the sync service).
        /// </summary>
        public void SyncAllDbs()
        {
            _dbSyncService.SyncAll();
        }

        /// <summary>
        /// Starts periodic sync for all logical DBs at the given interval (in milliseconds).
        /// </summary>
        public void StartPeriodicSync(int intervalMs)
        {
            _syncTimer?.Dispose();
            _syncTimer = new System.Threading.Timer(_ => SyncAllDbs(), null, intervalMs, intervalMs);
        }

        /// <summary>
        /// Stops the periodic sync timer.
        /// </summary>
        public void StopPeriodicSync()
        {
            _syncTimer?.Dispose();
            _syncTimer = null;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            StopPeriodicSync();
        }
        #endregion
    }
}