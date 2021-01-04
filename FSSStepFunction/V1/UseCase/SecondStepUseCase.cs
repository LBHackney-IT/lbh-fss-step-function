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
        private readonly INotifyGateway _notifyGateway;

        public SecondStepUseCase()
        {
            _organisationsGateway = new OrganisationsGateway();
            _notifyGateway = new NotifyGateway();
        }
        public OrganisationResponse GetOrganisationAndSendEmail(int id)
        {
            LoggingHandler.LogInfo("Second step executing request to gateway to get organisation");
            var organisation = _organisationsGateway.GetOrganisationById(id).ToResponse();
            if (organisation == null)
                return null;
            _notifyGateway.SendNotificationEmail(organisation.EmailAddresses.ToArray(), 2);
            organisation.StateResult = 0;
            return organisation;
        }
    }
}