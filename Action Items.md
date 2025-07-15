

Verify

1.	Real-time data synchronization between multiple desktop clients.
2.	Robust architecture and design pattern usage.
3.	Performance and scalability awareness.

---

## UI

a bit more ui work on the list
style to click on delete
fail to save task

---

## Entity Framework and Database

* Prepare schema and code for CQRS-style read/write separation 
* Implement \_taskCommandQueue to throttle DB writes (Single Writer Pattern)
* Simulate master/slave DB (write to one, read from another with periodic sync)
* Throttle or reject low-priority writes when under heavy load.: backpressure mechanism.

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


nice to have:
show item editing when filter and its not on
On permanent disconnect, prompt UI to notify the user or retry manually.
Log thread blocking or starvation events to simulate server pressure
can add cancellation tokens or timeout policies to retry/wait delays for signalR client