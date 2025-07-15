

Ui Client : Filtering doesnt work



## Testing and Tooling

* Simulate multiple users editing tasks concurrently to test locking and queues
* Simulate a large number of clients, throttled writes, and sharded reads

---

## Documentation

* Write README.md with setup instructions and architecture overview

---

## Nice To Have
orchestration: piplines to simulate operations/ console-clients with apis


a bit more ui work on the list : 
    padding for color/ right side is being cut off on edit mode
    show item editing when filter and its not on

Track and simulate connection drops, reconnect logic, slow DB writes, and queue overflow
On permanent disconnect, prompt UI to notify the user or retry manually.
Log thread blocking or starvation events to simulate server pressure
cancellation tokens or timeout policies to retry/wait delays for signalR client
commit pending methods?