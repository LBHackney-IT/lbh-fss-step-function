using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Handlers;
using LbhFssStepFunction.V1.UseCase.Interface;

namespace LbhFssStepFunction.V1.UseCase
{
    public class SecondStepUseCase : ISecondStepUseCase
    {
        private readonly IOrganisationGateway _organisationsGateway;

        public SecondStepUseCase()
        {
            _organisationsGateway = new OrganisationsGateway();
        }
        public OrganisationResponse GetOrganisationAndSendEmail(int id)
        {
            LoggingHandler.LogInfo("Executing request to gateway to get organisation");
            var organisation = _organisationsGateway.GetOrganisationById(id).ToResponse();
            return organisation;
        }
    }
}