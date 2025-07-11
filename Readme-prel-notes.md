list projects, responsibilities
list packages, reason for adding


WPF.Client > Interface for Communication > Implementation

Automapping: Model <> DTO



ASP.Net.Web > Interface for Communication > Implementation

Automapping: DB Entity <> DTO


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

SignalRTaskSyncClient
Aa resilient client for connecting to a SignalR hub to synchronize tasks in real time. It manages connection lifecycle, including automatic reconnection with exponential backoff and detailed logging via Serilog.

Key features:
* Raises events for task changes like TaskAdded, TaskUpdated, TaskDeleted, etc.
* Queues method calls made while disconnected and executes them once reconnected, preventing failures.
* Provides connection status updates (Connecting, Connected, Reconnecting, Disconnected, Failed) via events.
* Wraps hub calls with retry policies for transient error handling.
* Thread-safe and suitable for desktop or client applications needing reliable real-time updates.
* Use ConnectAsync() to establish connection, call API methods safely, and subscribe to events to stay in sync with server-side task updates.