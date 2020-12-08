using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways;
using LbhFssStepFunction.V1.Handlers;
using LbhFssStepFunction.V1.UseCase.Interface;

namespace LbhFssStepFunction.V1.UseCase
{
    public class GetOrganisationUseCase : IGetOrganisationUseCase
    {
        private readonly OrganisationsGateway _gateway;

        public GetOrganisationUseCase()
        {
            _gateway = new OrganisationsGateway();
        }
        public OrganisationResponse GetOrganisation(int id)
        {
            LoggingHandler.LogInfo("Executing request to gateway to get organisation");
            return _gateway.GetOrganisationById(id).ToResponse();
        }
    }
}