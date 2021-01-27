using System.Threading.Tasks;

namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface IThirdStepUseCase
    {
        Task<OrganisationResponse> GetOrganisationAndSendEmail(int id);
    }
}