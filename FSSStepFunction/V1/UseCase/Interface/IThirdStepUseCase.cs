namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface IThirdStepUseCase
    {
        OrganisationResponse GetOrganisationAndSendEmail(int id);
    }
}