using System.Threading.Tasks;

namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface IReminderToReminderUseCase
    {
        Task<OrganisationResponse> GetOrganisationAndSendEmail(int organisationId, int step);
    }
}
