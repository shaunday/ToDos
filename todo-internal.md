# Internal TODOs (Action Items)

## Server-Side
- [ ] Create a new ASP.NET Web API or SignalR server project targeting .NET Framework 4.8
- [ ] Set up Entity Framework in the server project
- [ ] Implement SignalR (or WebSocket) hub for real-time task updates
- [ ] Broadcast task changes and lock/unlock events to all connected clients
- [ ] Add exception handling and logging middleware
- [ ] Add connection string for SQL Server in config

## Database
- [ ] Design and create the Tasks table (Id, Title, Description, IsCompleted, IsLocked, etc.)
- [ ] (Optional) Add columns for LockedBy, LockedAt, Priority, DueDate, Tags
- [ ] Run initial EF migration and update database

## Client-Side (WPF)
- [ ] Set up MVVM structure in the WPF project
- [ ] Implement MainViewModel and TaskViewModel
- [ ] Create UI for task list, add/edit/delete, mark complete/incomplete
- [ ] Add UI indication for locked tasks (disable edit, show lock icon, etc.)
- [ ] Implement service for communicating with server (API + SignalR/WebSocket)
- [ ] Handle real-time updates from server (add/edit/delete/lock/unlock)
- [ ] Implement logic to send lock/unlock requests when editing starts/ends
- [ ] Add exception handling and user-friendly error messages

## Testing/Tools
- [ ] Create a script or orchestrator app to launch multiple WPF clients for testing
- [ ] Simulate multiple users editing tasks concurrently

## Bonus Features
- [ ] Add mock or real user authentication to both client and server
- [ ] Add task prioritization, tagging, and due dates to the model and UI
- [ ] Implement UI state persistence (e.g., window size, last selected task)

## Documentation
- [ ] Write README.md with setup instructions
- [ ] Document communication protocol and design patterns used
- [ ] Update todo.md and todo-internal.md as progress is made 


- check if object is locked/exists before adding/changing?