


signlarr server?
lock unlock subs on tasksvm
a bit more ui work on the list
caching server, client
login/ orchestrator > login

## Testing and Tooling

* Create simulator client, add logging display to orchestrator, add auto login with a user
* Simulate multiple users editing tasks concurrently to test locking and queues
* Simulate a large number of clients, throttled writes, and sharded reads

---

## Documentation

* Write README.md with setup instructions and architecture overview

---

## Notes and Reminders

* Track and simulate connection drops, reconnect logic, slow DB writes, and queue overflow


nice to have:
show item editing when filter and its not on
On permanent disconnect, prompt UI to notify the user or retry manually.
Log thread blocking or starvation events to simulate server pressure
can add cancellation tokens or timeout policies to retry/wait delays for signalR client
commit pending methods?