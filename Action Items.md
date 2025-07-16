
orch:
  first launch doesnt  give logs (again)

client:
  Use a timer or debounce logic to prevent multiple rapid calls.



Sim:
* Simulate multiple users editing tasks concurrently to test locking and queues


---

## Documentation

* Write README.md with setup instructions and architecture overview

---

## Nice To Have
orchestration/simulation
    server log
    Track and simulate connection drops, reconnect logic, slow DB writes, and queue overflow

UI Client 
    tasks: padding for color/ right side is being cut off on edit mode
    show item editing when filter and its not on
    loading/ or something while tasks are loading/adding
    mark filtering/clear button if filtered
    On permanent disconnect, prompt UI to notify the user or retry manually
    commit pending methods manually/ notify on ui

SignalR/Tasks Client
    cancellation tokens or timeout policies to retry/wait delays for signalR client

Server
    check result after operation on hub
    Log thread blocking or starvation events to simulate server pressure

