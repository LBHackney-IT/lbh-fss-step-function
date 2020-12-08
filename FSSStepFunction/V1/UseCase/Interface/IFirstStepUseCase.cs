namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface IFirstStepUseCase
    {
        OrganisationResponse GetOrganisationAndSendEmail(int id);
    }
}