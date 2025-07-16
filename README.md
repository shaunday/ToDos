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


list projects, responsibilities

add this too

ids : userid and taskid created on the server = int , tagid created on the client = guid

WPF.Client > Interface for Communication > Implementation

Automapping: Model <> DTO


ASP.Net.Web > Interface for Communication > Implementation
Automapping: DB Entity <> DTO

tips and tricks - connection id, NoSelectOnClickBehavior, clear focus, others, unlock on exit
write apis/interfaces for all access points / services

Packages Used:
CommunityToolkit.Mvvm (8.4.0) — Modern MVVM helpers and source generators for .NET UI apps.
DotNetEnv (3.1.1) — Loads environment variables from .env files for configuration.
MaterialDesignColors (5.2.1) — For Material Design color resources.
MaterialDesignThemes (5.2.1) — For Material Design WPF controls.
Unity (5.11.10) — Dependency injection container for managing object lifetimes and dependencies.

AutoMapper (15.0.0) — For object mapping.
Serilog (4.3.0) — Structured logging for .NET.
Microsoft.AspNet.SignalR.Client (2.4.3) — Real-time client communication for .NET using SignalR.
Polly(8.6.2) — .NET resilience and transient-fault-handling library for policies like retry, circuit-breaker, timeout, and bulkhead isolation.


* Document design patterns used and communication protocols 
* Explain scalability patterns used:

  * Queues buffer write load under stress (throttling)
  * Sharding, CQRS-Style Read/Write Separation
  * Use of ConfigureAwait(false) for freeing up threads
  * ThreadPool tuning via SetMinThreads(...)

Performance Optimization Patterns used:
caching, etc


highlight
Why the codebase is large (features, separation, testability, scalability).
That you focused on production-grade patterns, not just “getting it working.”
That you can also deliver smaller, focused solutions if needed

highlight - zero warnings, near zero message





instruction should have:
Make sure SQL Server is installed and running locally

Edit .env.repository (or wherever the connection string is) to point to your SQL Server instance

Run the app – the database will be created automatically
(add instructions)

broadcast filtering on server + try catch for edge cases and filtering on client

