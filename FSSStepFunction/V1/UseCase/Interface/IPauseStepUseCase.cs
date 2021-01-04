namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface IPauseStepUseCase
    {
        OrganisationResponse GetOrganisationAndSendEmail(int id);
    }
}