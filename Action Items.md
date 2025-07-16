
orch:
  first launch doesnt  give logs (again)
  padding on search too big
  margin below alive too big
  margin above clear filter/simulate buttons
  add server log

client:
  Use a timer or debounce logic to prevent multiple rapid calls.
  loading/ or something while tasks are loading/adding


Server:
   container types
   check result after operation on hub


* Simulate multiple users editing tasks concurrently to test locking and queues
 ... piplines to simulate operations/ console-clients with apis?

---

## Documentation

* Write README.md with setup instructions and architecture overview

---

## Nice To Have
orchestration/simulation
    simulate a large number of clients, throttled writes, and sharded reads
    Track and simulate connection drops, reconnect logic, slow DB writes, and queue overflow

UI Client 
    tasks: padding for color/ right side is being cut off on edit mode
    show item editing when filter and its not on
    On permanent disconnect, prompt UI to notify the user or retry manually
    commit pending methods manually/ notify on ui

SignalR/Tasks Client
    cancellation tokens or timeout policies to retry/wait delays for signalR client

Server
    Log thread blocking or starvation events to simulate server pressure

