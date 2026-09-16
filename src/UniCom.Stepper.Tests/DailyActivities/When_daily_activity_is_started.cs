using Machine.Specifications;
using One.Inception;
using One.Inception.Testing;
using One.StepTracking.DailyActivities;
using System;

namespace UniCom.Stepper.Tests.DailyActivities;

[Subject(typeof(DailyActivity))]
public class When_daily_activity_is_started
{
    static DailyActivity dailyActivity;
    static DailyActivityId id;
    static Urn person;

    Establish context = () =>
    {
        person = new Urn(Tests.UserId);
        id = new DailyActivityId(Tests.Tenant, Tests.DailyActivityId);
    };

    Because of = () => dailyActivity = new DailyActivity(id, person);

    It should_create_a_new_daily_activity = () => dailyActivity.ShouldNotBeNull();

    It should_have_the_correct_aggregate_id = () => dailyActivity.RootState<DailyActivityState>().Id.ShouldEqual(id);

    It should_have_the_correct_person_id = () => dailyActivity.RootState<DailyActivityState>().PersonId.ShouldEqual(person);

    It should_have_step_count_of_zero = () => dailyActivity.RootState<DailyActivityState>().StepCount.ShouldEqual(0);

    It should_publish_the_correct_event = () => dailyActivity.IsEventPublished<DailyActivityStarted>().ShouldBeTrue();

    It should_have_no_uncommitted_events = () => dailyActivity.HasNewPublicEvents().ShouldBeFalse();
}