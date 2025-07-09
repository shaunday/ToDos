# ToDos Real-Time Collaborative App

## Overview
This is a WPF + ASP.NET (SignalR) To-Do List application with real-time data synchronization. Multiple desktop clients can connect and see updates instantly as tasks are added, edited, deleted, or locked for editing.

## Features
- Add, edit, delete tasks
- Mark tasks as complete/incomplete
- Real-time updates across all clients (SignalR)
- Task locking to prevent simultaneous edits
- Task prioritization, tagging, and due dates
- MS SQL Server persistence (Entity Framework 6)
- Clean, MVVM-based WPF UI

## Tech Stack
- **Client:** WPF (.NET Framework 4.8, MVVM)
- **Server:** ASP.NET (SignalR, .NET Framework 4.8)
- **Database:** MS SQL Server (Entity Framework 6)

## Communication Approach
All client-server communication, including CRUD operations and real-time updates, is handled exclusively via SignalR hub methods. There are no traditional Web API (REST) endpoints in this implementation.

## Setup Instructions
1. **Clone the repository**
2. **Restore NuGet packages**
3. **Configure your SQL Server connection string** in `App.config`/`Web.config` (see below)
4. **Run Entity Framework migrations** to create the database
5. **Build and run the server**
6. **Build and run one or more WPF clients**

## Connection String Example
```
<connectionStrings>
  <add name="TaskDbContext" connectionString="Data Source=YOUR_SERVER;Initial Catalog=ToDosDb;Integrated Security=True;" providerName="System.Data.SqlClient" />
</connectionStrings>
```

## Communication Protocol
- **SignalR** is used for all CRUD operations and real-time updates.
- Clients call SignalR hub methods for adding, editing, deleting, and locking tasks.
- All clients receive updates instantly when any client changes a task.

## Design Patterns Used
- **Repository Pattern** for data access abstraction
- **MVVM** for WPF client structure
- **Pub/Sub** (SignalR) for real-time updates

## Notes
- For simplicity, entities are used directly in API responses and client models. In a production system, DTOs would be recommended.
- The app is designed for extensibility and can be enhanced with authentication, advanced filtering, etc.

## Bonus Features (if implemented)
- User authentication (mock or real)
- Task prioritization, tagging, due dates
- UI state persistence

## License
MIT (or specify your license) 