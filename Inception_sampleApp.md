# Overview
The goal of this project is to provide a production-ready application skeleton that demonstrates the architectural concepts behind the Inception framework. Although the sample application itself is intentionally simple, the patterns and infrastructure it uses are the same ones that can be applied to large-scale, real-world software systems.

The framework is based on over 15 years of practical experience building and operating enterprise software. It has successfully supported a platform with more than three million users and has been integrated with dozens of third-party systems for both importing and streaming data. Throughout that time, the architecture and its underlying concepts have been continuously refined, proven in production, and validated under real operational workloads.

From the beginning, the primary objective of the framework was to allow developers to focus on solving business problems rather than repeatedly implementing infrastructure concerns such as messaging, persistence, configuration, resilience, and integration plumbing. That objective has largely been achieved. Today, the framework provides these capabilities as reusable building blocks, enabling teams to develop new business functionality more quickly and consistently.

At the same time, the framework is not intended to hide how distributed systems work. Developers are expected to understand the architectural concepts and runtime behavior behind the abstractions. The framework simply provides the tools, conventions, and proven implementations needed to apply those concepts consistently, allowing teams to concentrate on delivering business value instead of rebuilding common infrastructure for every project.


# Sample application
// photo from the whiteboard
## Purpose
A minimal reference application built on the Inception framework, intended to run continuously on an integration server. Its job is to give a simple, observable way to confirm that all messages are flowing correctly through every applicable service and component of the framework, at all times.

## Domain
A person performing daily steps.

### Business Area 1 — Step Tracking

A person accumulates steps throughout the day.
Each day, the person's step count starts over from 0.
This area is responsible for collecting and recording steps as they happen.

### Business Area 2 — Rewards

A person receives a reward once they reach a daily step threshold.
If the threshold is met for 7 consecutive days, the person receives a pair of shoes.
Other rewards may exist for other milestones/conditions.

### Why this domain was chosen
It's deliberately simple and non-technical, so the running app can serve as a continuous, easy-to-understand health check — if steps aren't being tracked or rewards aren't being granted, something in the framework's message flow is broken.

# Tools
Although this reference application has a simple business domain, the supporting infrastructure mirrors what would be used in a production environment. The intention is to demonstrate how the Inception framework operates in a realistic distributed system rather than relying on simplified development-only components.

## RabbitMQ — Messaging

RabbitMQ serves as the application's messaging backbone. Commands, events, and other asynchronous messages flow through RabbitMQ, allowing different parts of the system to communicate without being tightly coupled. This makes the application resilient, scalable, and easier to evolve over time.

## Cassandra — Persistent Storage

Apache Cassandra is used as the primary data store. It provides high availability, fault tolerance, and excellent horizontal scalability, making it well suited for systems that continuously process large volumes of data across multiple nodes.

## Redis — Distributed Coordination

Redis is used for distributed synchronization and coordination between application instances. This enables multiple nodes to work together safely without relying on a single application server. Combined with RabbitMQ and Cassandra, it allows the application to scale horizontally by simply adding more nodes, provided the underlying infrastructure is sized appropriately.

## Consul — Configuration Management

Consul is responsible for centralized configuration management through the Settix project. Rather than embedding configuration within the application, services retrieve their configuration from a central source, making it easier to manage multiple environments and tenants while allowing configuration changes without modifying the application itself.

---

Together, these technologies form a foundation that has been proven in production systems. While they introduce more complexity than is strictly necessary for a sample application, they accurately represent the infrastructure required to build reliable, scalable, and maintainable distributed software.

# Steps
## Multitenancy
The Inception framework is designed for software systems and SaaS platforms where multiple tenants coexist within the same application. In the typical case, all tenants share the same codebase and infrastructure while their data and execution remain logically isolated. Although the framework allows tenant-specific behavior when required, the preferred approach is to keep business logic shared across tenants and introduce tenant-specific customizations only where they provide real value.

For the purposes of this reference application, only a single tenant will be configured, named one. This keeps the sample simple while still exercising the framework's multitenancy capabilities. In a real deployment, the tenant name would typically correspond to the customer or organization using the system.

## Naming
Naming is one of the hardest problems in software development. The names we choose may not always perfectly describe the underlying concepts, and they may evolve as the system grows.

For this project, we will use the following names:

Repository / Solution: One.Stepper
First Domain Project: One.StepTracking

These names are intentionally simple and will serve as the foundation for the rest of the application.

# Aggregate
The first aggregate will represent the step activity of a specific person. Its responsibility is to keep track of the number of steps recorded for that person and provide the business behavior required to update that state.

A naive approach would be to create a Person aggregate and continuously append all of the person's activity to it. This is generally a poor design because the aggregate effectively has an unlimited lifetime. There is no natural point at which its lifecycle ends, and over time it can become unnecessarily large and difficult to manage.

When designing an aggregate, two general rules are useful:

* Identify the end of the aggregate's lifecycle and design the aggregate around it. The lifecycle should have a meaningful business boundary rather than continuing indefinitely.
* Keep the number of modifications to an aggregate within a reasonable range. An aggregate may be modified many times during its lifetime, but the expected number of changes should remain bounded. For example, changing a username is an operation that would normally occur only a limited number of times throughout a person's lifetime—not millions of times.

For our application, we are interested in a person's steps for a single day. At the end of that day, the business lifecycle of that activity is complete. The next day represents a new piece of activity with its own lifecycle.

Therefore, the aggregate will be called DailyActivity.

The initial model will contain:

PersonId: the person whose activity is being tracked
Date: the day the activity belongs to
Steps: the total number of steps recorded for that day

There is an important time-related aspect to this model. A "day" depends on a timezone, and different users or locations could potentially be in different timezones. To keep the reference application focused on the framework rather than timezone management, we will make a deliberate simplification: the application will operate in a single timezone, and all step tracking will be interpreted according to that timezone.

This gives us a clear aggregate boundary:

One person + one day = one DailyActivity aggregate.

The model is intentionally small, but the aggregate boundary demonstrates an important principle that applies equally to much larger production systems: an aggregate should be designed around a meaningful business lifecycle and consistency boundary, rather than simply around an entity or database record.

### Package references
* download and install from nuget the package `1nception.domain`

### Define the aggregate Id and write the model logic
```c#
[DataContract(Namespace = BC.StepTracking, Name = "f1d8a17b-8dd8-4037-a529-87f4dfecb5b4")]
public class DailyActivityId : AggregateRootId
{
    DailyActivityId() { }

    public DailyActivityId(string tenant, string id) : base(tenant, "dailyactivity", id) { }

    public static DailyActivityId Parse(string urn)
    {
        var id = AggregateRootId.Parse(urn);
        return new DailyActivityId(id.NID, id.Id);
    }
}
```
* DataContract attribute is used to annotate any structure which will be persisted or transfered over the network. The `Namespace` marks the bounded context which owns the data structure and the `Name` should be a globally (for the system) unique string. You can use meaningful names hare however GUIDs gives us the freedom to do renames freely.
* The IDs follow urn structure and they are fully compliant to the [rfc8141](https://datatracker.ietf.org/doc/html/rfc8141). To keep it simple, think of it like: `urn:{tenant}:{aggregate}:{id}`. For example: `urn:one:dailyactivity:1234`

Next we need several more classes.

The app service coordinates how the aggregate communicates with other parts of the systems:
* entry point for aggregate changes
* aggregate data persistence and loading
* never add business logic here.

```c#
[DataContract(Namespace = BC.StepTracking, Name = "3a455230-4ec2-4145-bc33-9a8e75506838")]
public sealed class DailyActivityAppService : ApplicationService<DailyActivity>
{
    public DailyActivityAppService(IAggregateRepository repository) : base(repository)
    {
    }
}
```

The aggregate itself. Any business logic goes here
```c#
public class DailyActivity : AggregateRoot<DailyActivityState>
{

}
```

In-memory state of the aggregate so that we could make business decisions
```c#
public class DailyActivityState : AggregateRootState<DailyActivity, DailyActivityId>
{
    public DailyActivityState()
    {

    }

    public override DailyActivityId Id { get; set; }
}
```