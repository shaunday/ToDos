

1.	Real-time data synchronization between multiple desktop clients.
2.	Robust architecture and design pattern usage.
3.	Performance and scalability awareness.
4. DB •	Design should be normalized and allow future extensibility



## Logging

* Add exception handling and logging middleware
* Log Thread.ManagedThreadId before and after await calls to demonstrate async thread usage
* Log thread blocking or starvation events to simulate server pressure
* Log EF database connection usage (open/used connections) to demonstrate connection pooling awareness

---

## SignalR Client
can add cancellation tokens or timeout policies to retry/wait delays.

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
* Throttle or reject low-priority writes when under heavy load.: backpressure mechanism.
---

## Client-Side (WPF + MVVM)

* On permanent disconnect, prompt UI to notify the user or retry manually.
* when edit > filter it might set editing task to invisible.. fix

Make sure that both client and server validate all inputs (e.g., task titles not empty, due dates valid).

---

## Testing and Tooling

* Create simulator client, add logging display to orchestrator, add auto login with a user
* Simulate multiple users editing tasks concurrently to test locking and queues
* Simulate a large number of clients, throttled writes, and sharded reads

---

## Documentation

* Write README.md with setup instructions and architecture overview
* Document design patterns used and communication protocols (SignalR + REST)
* Prepare to defend your architecture choices (SignalR, queues, caching, etc.)
* Explain scalability patterns used:

  * Queues buffer write load under stress (throttling)
  * Single Writer Pattern prevents data conflicts
  * Use of ConfigureAwait(false) for freeing up threads
  * ThreadPool tuning via SetMinThreads(...)

---

## Notes and Reminders

* Track and simulate connection drops, reconnect logic, slow DB writes, and queue overflow
* Be aware SignalR doesn’t guarantee message delivery, handle edge cases


