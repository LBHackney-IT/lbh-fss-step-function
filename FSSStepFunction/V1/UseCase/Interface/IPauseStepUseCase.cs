using System.Threading.Tasks;

namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface IPauseStepUseCase
    {
        Task<OrganisationResponse> GetOrganisationAndSendEmail(int id);
    }
}