using System.Threading.Tasks;

namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface IFirstStepUseCase
    {
        Task<OrganisationResponse> GetOrganisationAndSendEmail(int id);
    }
}