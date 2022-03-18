using System;
using System.Threading.Tasks;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Handlers;
using LbhFssStepFunction.V1.Helpers;
using LbhFssStepFunction.V1.UseCase.Interface;

namespace LbhFssStepFunction.V1.UseCase
{
    public class ThirdStepUseCase : IThirdStepUseCase
    {
        private readonly IOrganisationsGateway _organisationsGateway;
        private readonly INotifyGateway _notifyGateway;
        private readonly string _waitDuration = Environment.GetEnvironmentVariable("WAIT_DURATION");

        public ThirdStepUseCase(IOrganisationsGateway organisationsGateway = null, INotifyGateway notifyGateway = null)
        {
            _organisationsGateway = organisationsGateway ?? new OrganisationsGateway();
            _notifyGateway = notifyGateway ?? new NotifyGateway();
        }
        public async Task<OrganisationResponse> GetOrganisationAndSendEmail(int id)
        {
            LoggingHandler.LogInfo("Third step executing request to gateway to get organisation");
            var organisation = _organisationsGateway.GetOrganisationById(id);
            
            if (organisation == null)
            {
                LoggingHandler.LogInfo($"Organisation with Id={id} can no longer be found!");
                return null;
            }
            else if (!organisation.InRevalidationProcess)
            {
                LoggingHandler.LogInfo($"Organisation with Id={id} is no longer in Reverification process!");
                return null;
            }
            else {
                LoggingHandler.LogInfo($"Organisation with Id={id} was found, attempting to send out emails.");
                var organisationResponse = organisation.ToResponse();

                await _notifyGateway
                    .SendNotificationEmail(
                        organisationResponse.OrganisationName, 
                        organisationResponse.EmailAddresses.ToArray(), 
                        3)
                    .ConfigureAwait(true);

                organisationResponse.StateResult = true;

                DateTime nextRunDate = SharedUtils.WaitTimeToDate(_waitDuration);

                organisationResponse.NextStepTime = nextRunDate;
                
                LoggingHandler.LogInfo($"Pause Step is scheduled at: {string.Concat(nextRunDate.ToString("s"), "Z")}.");
                return organisationResponse;
            }
        }
    }
}
