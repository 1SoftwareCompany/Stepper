using One.Inception;
using System.Runtime.Serialization;

namespace One.StepTracking.DailyActivities;

[DataContract(Namespace = BC.StepTracking, Name = "a4473c2f-c5b4-4289-bd25-31b89fe81b02")]
public sealed class DailyActivityStarted : IEvent
{
    public DailyActivityStarted() { }

    public DailyActivityStarted(DailyActivityId id, Urn personId, DateTimeOffset timestamp)
    {
        Id = id;
        PersonId = personId;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public DailyActivityId Id { get; private set; }

    [DataMember(Order = 2)]
    public Urn PersonId { get; private set; }

    [DataMember(Order = 3)]
    public DateTimeOffset Timestamp { get; private set; }
}
