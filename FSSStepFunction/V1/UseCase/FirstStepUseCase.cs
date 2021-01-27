using System;
using System.Threading.Tasks;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Handlers;
using LbhFssStepFunction.V1.UseCase.Interface;

namespace LbhFssStepFunction.V1.UseCase
{
    public class FirstStepUseCase : IFirstStepUseCase
    {
        private readonly IOrganisationsGateway _organisationsGateway;
        private readonly INotifyGateway _notifyGateway;
        private readonly string _waitDuration = Environment.GetEnvironmentVariable("WAIT_DURATION");

        public FirstStepUseCase(IOrganisationsGateway organisationsGateway = null, INotifyGateway notifyGateway = null)
        {
            _organisationsGateway = organisationsGateway ?? new OrganisationsGateway();
            _notifyGateway = notifyGateway ?? new NotifyGateway();
        }

        public async Task<OrganisationResponse> GetOrganisationAndSendEmail(int id)
        {
            LoggingHandler.LogInfo("Executing request to gateway to get organisation");
            var organisationDomain = _organisationsGateway.GetOrganisationById(id);
            if (organisationDomain == null)
                return null;
            var organisation = organisationDomain.ToResponse();
            await _notifyGateway.SendNotificationEmail(organisation.OrganisationName, organisation.EmailAddresses.ToArray(), 1).ConfigureAwait(true);
            organisation.StateResult = true;
            //ToDo: Change AddSeconds to AddDays
            organisation.NextStepTime = DateTime.Now.AddSeconds(Int32.Parse(_waitDuration));
            return organisation;
        }
    }
}