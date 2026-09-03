using One.Inception;
using System.Runtime.Serialization;

namespace One.StepTracking.DailyActivities;

[DataContract(Namespace = BC.StepTracking, Name = "78167aaf-9af7-4e27-a2b9-1a3e75dfa55e")]
public sealed class PersonDailyStepsTracked : IEvent
{
    public PersonDailyStepsTracked() { }

    public PersonDailyStepsTracked(DailyActivityId id, Urn personId, int stepsTracked, DateTimeOffset timestamp)
    {
        Id = id;
        PersonId = personId;
        StepsTracked = stepsTracked;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public DailyActivityId Id { get; private set; }

    [DataMember(Order = 2)]
    public Urn PersonId { get; private set; }

    [DataMember(Order = 3)]
    public int StepsTracked { get; private set; }

    [DataMember(Order = 4)]
    public DateTimeOffset Timestamp { get; private set; }
}
