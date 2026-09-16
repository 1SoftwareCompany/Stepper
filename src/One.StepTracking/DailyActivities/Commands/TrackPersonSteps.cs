using One.Inception;
using System.Runtime.Serialization;

namespace One.StepTracking.DailyActivities;

[DataContract(Namespace = BC.StepTracking, Name = "c4fe037f-84f8-4685-aec5-a1055dd1c556")]
public sealed class TrackPersonSteps : ICommand
{
    public TrackPersonSteps() { }

    [DataMember(Order = 1)]
    public DailyActivityId Id { get; set; }

    [DataMember(Order = 2)]
    public Urn PersonId { get; set; }

    [DataMember(Order = 3)]
    public int Steps { get; set; }

    [DataMember(Order = 4)]
    public DateTimeOffset Timestamp { get; set; }
}
