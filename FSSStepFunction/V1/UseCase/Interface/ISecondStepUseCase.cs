using System.Threading.Tasks;

namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface ISecondStepUseCase
    {
        Task<OrganisationResponse> GetOrganisationAndSendEmail(int id);
    }
}