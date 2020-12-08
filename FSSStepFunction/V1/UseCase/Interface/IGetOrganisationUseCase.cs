namespace LbhFssStepFunction.V1.UseCase.Interface
{
    public interface IGetOrganisationUseCase
    {
        OrganisationResponse GetOrganisation(int id);
    }
}