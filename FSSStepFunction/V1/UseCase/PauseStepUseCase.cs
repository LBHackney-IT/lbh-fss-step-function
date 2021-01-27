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

        public PauseStepUseCase(IOrganisationsGateway organisationsGateway = null, INotifyGateway notifyGateway = null)
        {
            _organisationsGateway = organisationsGateway ?? new OrganisationsGateway();
            _notifyGateway = notifyGateway ?? new NotifyGateway();
        }
        public async Task<OrganisationResponse> GetOrganisationAndSendEmail(int id)
        {
            LoggingHandler.LogInfo("Second step executing request to gateway to get organisation");
            var org = _organisationsGateway.GetOrganisationById(id);
            if (org == null)
                return null;
            var updatedOrg = _organisationsGateway.PauseOrganisation(org.Id);
            if (updatedOrg == null)
                return null;
            var organisation = updatedOrg.ToResponse();
            await _notifyGateway.SendNotificationEmail(organisation.OrganisationName, organisation.EmailAddresses.ToArray(), 4).ConfigureAwait(true);
            return organisation;
        }
    }
}