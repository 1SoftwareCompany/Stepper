using One.Inception;
using System.Runtime.Serialization;

namespace One.StepTracking.DailyActivities;

[DataContract(Namespace = BC.StepTracking, Name = "3a455230-4ec2-4145-bc33-9a8e75506838")]
public sealed class DailyActivityAppService : ApplicationService<DailyActivity>,
    ICommandHandle<TrackPersonSteps>
{
    public DailyActivityAppService(IAggregateRepository repository) : base(repository)
    {
    }

    public async Task HandleAsync(TrackPersonSteps command)
    {
        DailyActivity dailyActivity = null;

        // Attempt to load the DailyActivity aggregate by ID from the repository.
        ReadResult<DailyActivity> dailyActivityResult = await repository.LoadAsync<DailyActivity>(command.Id).ConfigureAwait(false);
        if (dailyActivityResult.IsSuccess)
        {
            // The aggregate was found by ID, so we can use it to track the steps.
            dailyActivity = dailyActivityResult.Data;
        }
        else if (dailyActivityResult.NotFound)
        {
            // The aggregate was not found by ID, so we create a new instance with the provided ID and PersonId.
            dailyActivity = new DailyActivity(command.Id, command.PersonId);
        }

        // Track the steps using the provided command data.
        dailyActivity.TrackSteps(command.Steps);

        // Save the updated aggregate back to the repository.
        await repository.SaveAsync(dailyActivity).ConfigureAwait(false);
    }
}
