using System.Threading.Tasks;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Handlers;
using LbhFssStepFunction.V1.UseCase.Interface;

namespace LbhFssStepFunction.V1.UseCase
{
    public class PauseStepUseCase : IPauseStepUseCase
    {
        private readonly IOrganisationsGateway _organisationsGateway;
        private readonly INotifyGateway _notifyGateway;

        public PauseStepUseCase()
        {
            _organisationsGateway = new OrganisationsGateway();
            _notifyGateway = new NotifyGateway();
        }
        public async Task<OrganisationResponse> GetOrganisationAndSendEmail(int id)
        {
            LoggingHandler.LogInfo("Second step executing request to gateway to get organisation");
            var organisation = _organisationsGateway.GetOrganisationById(id).ToResponse();
            if (organisation == null)
                return null;
            await _notifyGateway.SendNotificationEmail(organisation.OrganisationName, organisation.EmailAddresses.ToArray(), 4).ConfigureAwait(true);
            return organisation;
        }
    }
}