using Machine.Specifications;
using One.Inception;
using One.Inception.Testing;
using One.StepTracking.DailyActivities;
using System;

namespace UniCom.Stepper.Tests.DailyActivities;

[Subject(typeof(DailyActivity))]
public class When_tracking_a_single_step
{
    static DailyActivity dailyActivity;
    static DailyActivityId id;
    static Urn person;

    Establish context = () =>
    {
        person = new Urn(Tests.UserId);
        id = new DailyActivityId(Tests.Tenant, Tests.DailyActivityId);

        dailyActivity = new DailyActivity(id, person);
    };

    Because of = () => dailyActivity.TrackStep();

    It should_increase_step_count_by_one = () => dailyActivity.RootState<DailyActivityState>().StepCount.ShouldEqual(1);

    It should_publish_the_steps_tracked_event = () => dailyActivity.IsEventPublished<PersonDailyStepsTracked>().ShouldBeTrue();

    It should_have_the_correct_step_count_in_event = () => 
        dailyActivity.RootState<DailyActivityState>().StepCount.ShouldEqual(1);
}

[Subject(typeof(DailyActivity))]
public class When_tracking_multiple_steps
{
    static DailyActivity dailyActivity;
    static DailyActivityId id;
    static Urn person;
    static int stepsToTrack = 5;

    Establish context = () =>
    {
        person = new Urn(Tests.UserId);
        id = new DailyActivityId(Tests.Tenant, Tests.DailyActivityId);

        dailyActivity = new DailyActivity(id, person);
    };

    Because of = () => dailyActivity.TrackSteps(stepsToTrack);

    It should_increase_step_count_by_specified_amount = () => dailyActivity.RootState<DailyActivityState>().StepCount.ShouldEqual(stepsToTrack);

    It should_publish_the_steps_tracked_event = () => dailyActivity.IsEventPublished<PersonDailyStepsTracked>().ShouldBeTrue();
}

[Subject(typeof(DailyActivity))]
public class When_tracking_steps_after_aggregation
{
    static DailyActivity dailyActivity;
    static DailyActivityId id;
    static Urn person;
    static int firstTrack = 3;
    static int secondTrack = 2;

    Establish context = () =>
    {
        person = new Urn(Tests.UserId);
        id = new DailyActivityId(Tests.Tenant, Tests.DailyActivityId);

        dailyActivity = Aggregate<DailyActivity>.FromHistory(stream => stream
            .AddEvent(new DailyActivityStarted(id, person, DateTimeOffset.UtcNow))
            .AddEvent(new PersonDailyStepsTracked(id, person, firstTrack, DateTimeOffset.UtcNow)));
    };

    Because of = () => dailyActivity.TrackSteps(secondTrack);

    It should_have_accumulated_step_count = () => dailyActivity.RootState<DailyActivityState>().StepCount.ShouldEqual(firstTrack + secondTrack);

    It should_publish_the_steps_tracked_event = () => dailyActivity.IsEventPublished<PersonDailyStepsTracked>().ShouldBeTrue();
}