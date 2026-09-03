using One.Inception;
using System.Runtime.Serialization;

namespace One.StepTracking.DailyActivities;

[DataContract(Namespace = BC.StepTracking, Name = "f1d8a17b-8dd8-4037-a529-87f4dfecb5b4")]
public sealed class DailyActivityId : AggregateRootId
{
    DailyActivityId() { }

    public DailyActivityId(string tenant, string id) : base(tenant, "dailyactivity", id) { }

    public static DailyActivityId Parse(string urn)
    {
        var id = AggregateRootId.Parse(urn);
        return new DailyActivityId(id.NID, id.Id);
    }
}
