namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface ISecondStepUseCase
    {
        OrganisationResponse GetOrganisationAndSendEmail(int id);
    }
}