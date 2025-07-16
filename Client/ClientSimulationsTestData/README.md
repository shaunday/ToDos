# ClientSimTest

This directory contains scenario-based test scripts for simulating various client behaviors in the ToDos system. Each subdirectory represents a scenario category, with scripts to execute different flows.

**Note:** Each script file in a scenario directory is intended to be run concurrently with the others in the same directory, simulating multiple clients acting at the same time.

## Running Simulations

### Orchestrated Multi-Client Mode
To run multiple simulator clients in parallel (as specified by the `numOfClients` header in the script file), use the `--orchestrate` flag:

```
ToDos.Clients.Simulator.exe --orchestrate <scriptfile>
```
- The orchestrator will read the `numOfClients` value from the script file (default is 1 if missing) and launch that many clients in parallel.
- Each client will use a unique userId (base userId from the script + client index).

### Single-Client Mode
To run a single simulator client (legacy mode), provide a userId and script file:

```
ToDos.Clients.Simulator.exe <userId> <scriptfile>
```
- Only one client will run, regardless of the `numOfClients` value in the script file.

## Script File Headers
- `userId=<number>`: Base user ID for the simulation (each client gets userId + index).
- `signToEvents=true|false`: Subscribe to task events and log them.
- `numOfClients=<number>`: Number of concurrent simulator clients to launch (used only in orchestrate mode).

## Example Script File
```
userId=123
signToEvents=true
numOfClients=5

Add,500,500,2000
Add,100,50,500
Delete,50,20,200
GetAll,10,1000,500
```

## Scenario Directories

### 1. BasicUsage
Simulates standard user flows—logging in, adding, and completing tasks.
- `login_and_add_task.txt`: Log in and add a single task.
- `add_multiple_tasks.txt`: Add several tasks in sequence.
- `complete_task.txt`: Mark a task as completed.

### 2. ConcurrentUsers
Tests multiple users interacting with the system at the same time, including conflict scenarios.
- `two_users_add_tasks_client1.txt`, `two_users_add_tasks_client2.txt`: Two users add tasks concurrently.
- `user_conflict_edit_client1.txt`, `user_conflict_edit_client2.txt`: Two users attempt to edit the same task.
- `simultaneous_login_logout_client1.txt`, `simultaneous_login_logout_client2.txt`: Users log in and out at the same time.
- `multi_user_edit_locking_client1.txt`, `multi_user_edit_locking_client2.txt`, `multi_user_edit_locking_client3.txt`: **Simulate multiple users editing tasks concurrently to test locking and queue handling.**

### 3. OfflineSync
Simulates working offline, then syncing changes and resolving conflicts.
- `add_tasks_offline_client1.txt`, `add_tasks_offline_client2.txt`: Add tasks while offline.
- `sync_after_reconnect_client1.txt`, `sync_after_reconnect_client2.txt`: Sync tasks after reconnecting.
- `conflict_resolution_client1.txt`, `conflict_resolution_client2.txt`: Handle conflicts during sync.

### 4. TaggingAndFiltering
Focuses on adding/removing tags and filtering tasks by tags.
- `add_tags_to_tasks_client1.txt`, `add_tags_to_tasks_client2.txt`: Add tags to tasks.
- `filter_by_tag_client1.txt`, `filter_by_tag_client2.txt`: Filter tasks by tag.
- `remove_tag_client1.txt`, `remove_tag_client2.txt`: Remove a tag from a task.

### 5. ErrorHandling
Covers invalid operations, error responses, and recovery from failures.
- `invalid_login_client1.txt`, `invalid_login_client2.txt`: Attempt login with invalid credentials.
- `add_task_missing_fields_client1.txt`, `add_task_missing_fields_client2.txt`: Add a task with missing required fields.
- `network_failure_recovery_client1.txt`, `network_failure_recovery_client2.txt`: Simulate network failure and recovery.

### 6. BulkOperations
Tests the system’s handling of bulk actions on tasks.
- `bulk_add_tasks_client1.txt`, `bulk_add_tasks_client2.txt`: Add many tasks at once.
- `bulk_delete_tasks_client1.txt`, `bulk_delete_tasks_client2.txt`: Delete multiple tasks in bulk.
- `bulk_update_status_client1.txt`, `bulk_update_status_client2.txt`: Update the status of many tasks at once. 