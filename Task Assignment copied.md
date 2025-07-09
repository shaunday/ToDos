# TODO List

## Core Tasks

1. Design and implement the server-side application (ASP.NET Web API/SignalR) for real-time task synchronization and CRUD operations.
2. Design and implement the MS SQL Server database schema for tasks, including normalization and extensibility.
3. Set up Entity Framework or ORM in the server project for database access and repository pattern implementation.
4. Implement real-time communication (SignalR/WebSocket) in the server for broadcasting task changes to all clients.
5. Create the WPF client application with MVVM architecture, supporting Add/Edit/Delete/Complete/Incomplete tasks.
6. Implement client-server communication in the WPF app (connect to server, receive real-time updates, send CRUD requests).
7. Implement task locking mechanism to prevent simultaneous edits across clients.
8. Design a clean, intuitive WPF UI (optionally using a design library).
9. Implement exception handling and logging on both client and server sides.

## Bonus Features

10. Add user authentication (mock or real) to the system.
11. Add task prioritization, tagging, and due dates to the data model and UI.
12. Implement persistence of UI state in the WPF client.

## Development/Testing Tools

13. Create an orchestrator/launcher app to start multiple WPF clients for testing/demo.

## Documentation

14. Write a README.md with setup instructions, design patterns used, and communication protocol reasoning. 