using System;
using System.IO;
using Newtonsoft.Json;
using Todos.Ui.Models;
using Serilog;

namespace Todos.Ui.Services
{
    public static class UiStatePersistenceService
    {
        private const string AppDataFolder = "ToDos";
        private const string StateFileName = "ui_state.json";

        public static string GetUserStateFilePath(int userId)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, AppDataFolder, userId.ToString());
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, StateFileName);
        }

        public static UiStateModel Load(int userId)
        {
            var path = GetUserStateFilePath(userId);
            if (!File.Exists(path))
                return null;
            try
            {
                string json = null;
                try
                {
                    json = File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[UIState] Read failed: {Path}", path);
                    return null;
                }
                return JsonConvert.DeserializeObject<UiStateModel>(json);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[UIState] Deserialize failed: {Path}", path);
                return null;
            }
        }

        public static void Save(int userId, UiStateModel state)
        {
            if (state == null) return;
            var path = GetUserStateFilePath(userId);
            try
            {
                var json = JsonConvert.SerializeObject(state, Formatting.Indented);
                try
                {
                    File.WriteAllText(path, json);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[UIState] Write failed: {Path}", path);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[UIState] Serialize failed: {Path}", path);
            }
        }
    }
} 