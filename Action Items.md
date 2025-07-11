---

# Internal TODOs — Scalable Real-Time To-Do App

---

## Logging
* Add exception handling and logging middleware
* Log Thread.ManagedThreadId before and after await calls to demonstrate async thread usage
* Log thread blocking or starvation events to simulate server pressure
* Log EF database connection usage (open/used connections) to demonstrate connection pooling awareness

---

## SignalR Client

* You can add cancellation tokens or timeout policies to retry/wait delays.
* On permanent disconnect, prompt UI to notify the user or retry manually.
* Optionally implement a circuit breaker with Polly if you want to stop hammering the server.

---

## SignalR HOST

* API Versioning
* Implement SignalR hub for real-time task updates
  services.AddSignalR().AddHubOptions(opts =>
  {
  opts.KeepAliveInterval = TimeSpan.FromSeconds(30);
  opts.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
  });
* Broadcast task changes and lock/unlock events to all connected clients

---

## Entity Framework and Database

* Use async EF methods with ConfigureAwait(false) for scalability
  await dbContext.SaveChangesAsync().ConfigureAwait(false);
* Design and create the Tasks table with columns: Id, Title, Description, IsCompleted, IsLocked, UserId, ShardId
* Ensure database schema is normalized to 3rd Normal Form (3NF) for extensibility
* Prepare schema and code for CQRS-style read/write separation 
* Implement sharding per user (single Tasks table with ShardId column)
  var shardId = \_shardResolver.GetShard(userId);
  var tasks = db.Tasks.Where(t => t.ShardId == shardId);
* partitioning proof?
* Add connection string for SQL Server in configuration
* Add ThreadPool.SetMinThreads(...) to show concurrency tuning preparation
* Implement memory caching for frequent reads
* Implement \_taskCommandQueue to throttle DB writes (Single Writer Pattern)
* Simulate master/slave DB (write to one, read from another with periodic sync)

---

## Client-Side (WPF + MVVM)

* set login stuff
* Create UI for task list, add/edit/delete, mark complete/incomplete
* Add UI indication for locked tasks (lock icon, disabled editing controls)
* Implement service layer for communication with SignalR and REST API
* Handle real-time updates from server (add/edit/delete/lock/unlock)
* Implement logic to send lock/unlock requests on edit start/end
* Add user-friendly exception handling and error messages
* Show SignalR connection state in the UI
* Extend task model and UI with priority, tags, and due date fields
* BONUS: Implement UI state persistence (e.g., window size, last selected task)
^ Save UI state on Window\.Closing and restore on Window\.Loaded using Properties.Settings.Default or JSON file in %AppData%

Make sure that both client and server validate all inputs (e.g., task titles not empty, due dates valid).

---

## Testing and Tooling

* Create simulator client, add logging display to orchestrator, add auto login with a user
* Simulate multiple users editing tasks concurrently to test locking and queues
* Simulate a large number of clients, throttled writes, and sharded reads
---

## Users
* Add mock or real user authentication on both client and server

---

## Documentation

* Write README.md with setup instructions and architecture overview
* Document design patterns used and communication protocols (SignalR + REST)
* Explain scalability patterns used:

  * Queues buffer write load under stress (throttling)
  * Single Writer Pattern prevents data conflicts
  * Use of ConfigureAwait(false) for freeing up threads
  * ThreadPool tuning via SetMinThreads(...)

---

## Notes and Reminders

* Check if task is locked or exists before allowing edits or saves
* Track and simulate connection drops, reconnect logic, slow DB writes, and queue overflow
* Prepare to defend your architecture choices (SignalR, queues, caching, etc.)
* Be aware SignalR doesn’t guarantee message delivery, handle edge cases


---

## Moar

* Client can queue local changes while offline, then sync when back online.
* Throttle or reject low-priority writes when under heavy load.: backpressure mechanism.