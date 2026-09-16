using One.Inception;

namespace One.StepTracking.DailyActivities;

public class DailyActivityState : AggregateRootState<DailyActivity, DailyActivityId>
{
    public DailyActivityState()
    {
        StepCount = 0;
    }

    public override DailyActivityId Id { get; set; }

    public Urn PersonId { get; set; }

    public int StepCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public void When(DailyActivityStarted @event)
    {
        Id = @event.Id;
        PersonId = @event.PersonId;
        StepCount = 0;
        CreatedAt = @event.Timestamp;
    }

    public void When(PersonDailyStepsTracked @event)
    {
        Id = @event.Id;
        PersonId = @event.PersonId;
        StepCount += @event.StepsTracked;
    }
}
