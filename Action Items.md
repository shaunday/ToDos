
## Orch:
* Logs Viewer is broken
* Add a button to open server-log 
* Track and simulate connection drops, reconnect logic, slow DB writes, and queue overflow 

## Headless Sim:
* Get to work

## Client:
* Actions Col is cut when multi launch via orch
* Use a timer or debounce logic to prevent multiple rapid calls.
* Tasks: padding for color/ right side is being cut off on edit mode
* Show item editing when filter and its not on
* mark filtering/clear button if filtered
* loading/ or something while tasks are loading/adding
* On permanent disconnect, prompt UI to notify the user or retry manually + notify on ui


## Server
* Better handling of results on hub 
* Log thread blocking or starvation events to simulate server pressure


## SignalR/Tasks Client
* Cancellation tokens or timeout policies to retry/wait delays for signalR client
