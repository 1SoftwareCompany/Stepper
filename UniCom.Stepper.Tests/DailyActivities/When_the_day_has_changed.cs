using Machine.Specifications;
using One.Inception;
using One.Inception.Testing;
using One.StepTracking.DailyActivities;
using System;

namespace UniCom.Stepper.Tests.DailyActivities;

[Subject(typeof(DailyActivity))]
public class When_the_day_has_changed
{
    static DailyActivity dailyActivity;
    static DailyActivityId id;
    static Urn person;

    Establish context = () =>
    {
        person = new Urn(Tests.UserId);
        id = new DailyActivityId(Tests.Tenant, Tests.DailyActivityId);

        // Create daily activity with a timestamp from yesterday
        var yesterday = DateTimeOffset.UtcNow.AddDays(-1);

        dailyActivity = Aggregate<DailyActivity>.FromHistory(stream => stream
            .AddEvent(new DailyActivityStarted(id, person, yesterday))
            .AddEvent(new PersonDailyStepsTracked(id, person, 10, yesterday)));
    };

    Because of = () => dailyActivity.TrackStep();

    It should_not_publish_any_new_events = () => dailyActivity.IsEventPublished<PersonDailyStepsTracked>().ShouldBeFalse();

    It should_keep_the_original_step_count = () => dailyActivity.RootState().StepCount.ShouldEqual(10);

    It should_have_no_uncommitted_events = () => dailyActivity.HasNewPublicEvents().ShouldBeFalse();
}