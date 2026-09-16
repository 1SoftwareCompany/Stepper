# One.Stepper

A production-ready reference application built on the **Inception framework**. The business domain is deliberately simple — tracking a person's daily steps — so the running app can serve as a continuous, easy-to-understand health check for the framework's message flow: if steps aren't being tracked, something in the pipeline is broken.

The patterns and infrastructure used here are the same ones applied to large-scale, real-world systems. Inception is based on over 15 years of experience building and operating enterprise software, including a platform with more than three million users integrated with dozens of third-party systems.

> For the full architectural walkthrough — aggregate design decisions, naming, multitenancy, and code annotations — see [Inception_sampleApp.md](Inception_sampleApp.md).

## Domain

A person performs daily steps.

- **Step Tracking** — a person accumulates steps throughout the day; the count starts over from 0 each day. This area collects and records steps as they happen.
- **Rewards** — a person receives a reward once they reach a daily step threshold (e.g., a pair of shoes for meeting the threshold 7 consecutive days). *Not yet implemented in this repository.*

## Architecture

The application is a distributed, event-driven system:

- **Commands** express an intent (`TrackPersonSteps`).
- **Aggregates** own the business logic and validate invariants (`DailyActivity`).
- **Events** record what happened (`DailyActivityStarted`, `PersonDailyStepsTracked`) and are the unit of persistence — aggregate state is rebuilt by replaying events.

The aggregate boundary is **one person + one day = one `DailyActivity` aggregate**. This gives the aggregate a meaningful business lifecycle (it ends at the end of the day) instead of an unbounded "person" aggregate that would grow indefinitely.

### Key types

| Type | Role |
| --- | --- |
| `DailyActivityId` | Aggregate identifier, a URN of the form `urn:{tenant}:dailyactivity:{id}` (RFC 8141). |
| `DailyActivity` | The aggregate. Holds business rules, e.g. steps can only be tracked for the current day. |
| `DailyActivityState` | In-memory state rebuilt from events (`PersonId`, `StepCount`, `CreatedAt`). |
| `DailyActivityAppService` | Application service: loads/creates the aggregate, dispatches the command, persists the result. No business logic here. |
| `TrackPersonSteps` | Command: track a number of steps for a person's daily activity. |
| `DailyActivityStarted` / `PersonDailyStepsTracked` | Domain events. |

All structures that are persisted or transferred over the network are annotated with `[DataContract]`, where the `Namespace` marks the owning bounded context and the `Name` is a globally unique identifier (a GUID, so names can be renamed freely).

## Technology stack

The reference application runs on the same infrastructure used in production:

- **RabbitMQ** — messaging backbone for commands, events, and other asynchronous messages.
- **Apache Cassandra** — primary data store; high availability and horizontal scalability for continuous high-volume processing.
- **Redis** — distributed coordination and synchronization between application instances.
- **Consul** — centralized configuration management (via the Settix project), so configuration changes don't require application changes.

The framework is multitenant by design; this sample is configured with a single tenant named `one`.

## Project structure

```
One.Stepper.slnx                  Solution file
One.StepTracking.Shared/          Bounded-context constants (BC.StepTracking)
One.StepTracking/                 Step tracking domain
└── DailyActivities/
    ├── DailyActivityId.cs        Aggregate identifier (URN)
    ├── DailyActivity.cs          Aggregate (business logic)
    ├── DailyActivityState.cs     Event-sourced state
    ├── DailyActivityAppService.cs Application service + command handler
    ├── Commands/
    │   └── TrackPersonSteps.cs   Command
    └── Events/
        ├── DailyActivityStarted.cs
        └── PersonDailyStepsTracked.cs
```

## Getting started

### Prerequisites

- .NET 10 SDK
- Access to the `1nception.Domain` NuGet package (referenced by `One.StepTracking`)

### Build

```bash
dotnet build One.Stepper.slnx
```

### Running

The domain project contains the aggregate, events, and command handling. A full deployment additionally requires the messaging, storage, coordination, and configuration infrastructure listed above (RabbitMQ, Cassandra, Redis, Consul) and a host that wires the Inception runtime together.

## License

Apache License 2.0 — see [LICENSE](LICENSE).
