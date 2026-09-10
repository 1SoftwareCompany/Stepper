using One.Inception;

namespace One.StepTracking.DailyActivities;

public class DailyActivity : AggregateRoot<DailyActivityState>
{
    internal DailyActivity() { }

    public DailyActivity(DailyActivityId id, Urn personId)
    {
        IEvent @event = new DailyActivityStarted(id, personId, DateTimeOffset.UtcNow);
        Apply(@event);
    }

    public void TrackStep()
    {
        TrackSteps(1);
    }

    public void TrackSteps(int steps)
    {
        if (state.CreatedAt.DayOfYear == DateTimeOffset.UtcNow.DayOfYear)
        {
            IEvent @event = new PersonDailyStepsTracked(state.Id, state.PersonId, steps, DateTimeOffset.Now);
            Apply(@event);
        }
        else
        {
            // do nothing, the business rule is that we can only track steps for the current day, so if the day has changed, we don't track steps
        }
    }
}
