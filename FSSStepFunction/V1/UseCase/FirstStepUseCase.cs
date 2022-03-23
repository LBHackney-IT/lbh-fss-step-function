using System;
using System.Threading.Tasks;
using LbhFssStepFunction.V1.Errors;
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

            try {
                _organisationsGateway.FlagOrganisationToBeInRevalidation(id);
            } catch (ResourceNotFoundException ex) {
                LoggingHandler.LogInfo($"{ex.Message}\n\nTerminating First Step Function.");
                return null;
            }

            // any retrieved organisation shouldn't be "null" at this point, because if it was, then the
            // code would have terminated on line 32.
            var organisationDomain = _organisationsGateway.GetOrganisationById(id);
            var organisation = organisationDomain.ToResponse();

            await _notifyGateway
                .SendNotificationEmail(
                    organisation.OrganisationName, 
                    organisation.EmailAddresses.ToArray(),
                    1)
                .ConfigureAwait(true);
                
            organisation.StateResult = true;
            
            //ToDo: Change AddSeconds to AddDays
            organisation.NextStepTime = DateTime.Now.AddSeconds(Int32.Parse(_waitDuration));
            return organisation;
        }
    }
}
