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
    public class ReminderToReminderUseCase : IReminderToReminderUseCase
    {
        private readonly INotifyGateway _notifyGateway;
        private readonly IOrganisationsGateway _organisationsGateway;
        private readonly string _waitDuration;

        public ReminderToReminderUseCase(
            IOrganisationsGateway organisationsGateway = null,
            INotifyGateway notifyGateway = null)
        {
            _notifyGateway = notifyGateway ?? new NotifyGateway();
            _organisationsGateway = organisationsGateway ?? new OrganisationsGateway();
            _waitDuration = Environment.GetEnvironmentVariable("WAIT_DURATION");
        }
        public async Task<OrganisationResponse> GetOrganisationAndSendEmail(int organisationId, int step)
        {
            LoggingHandler.LogInfo($"Step {step} executing request to gateway to get organisation");
            var organisation = _organisationsGateway.GetOrganisationById(organisationId);
            
            if (organisation == null)
            {
                LoggingHandler.LogInfo($"Organisation with Id={organisationId} can no longer be found!");
                return null;
            }
            else if (!organisation.InRevalidationProcess)
            {
                LoggingHandler.LogInfo($"Organisation with Id={organisationId} is no longer in Reverification process!");
                return null;
            }
            else {
                LoggingHandler.LogInfo($"Organisation with Id={organisationId} was found, attempting to send out emails.");
                var organisationResponse = organisation.ToResponse(); // TODO: Need refactoring! This should be domain logic

                await _notifyGateway
                    .SendNotificationEmail(
                        organisationResponse.OrganisationName, 
                        organisationResponse.EmailAddresses.ToArray(), 
                        step);

                organisationResponse.StateResult = true;

                DateTime nextRunDate = SharedUtils.WaitTimeToDate(_waitDuration);
                organisationResponse.NextStepTime = nextRunDate;
                
                LoggingHandler.LogInfo(
                    $"{(step > 2 ? "Pause Step" : $"Step {step+1}")} is scheduled at: {nextRunDate.ToString("s")}.");
                return organisationResponse;
            }
        }
    }
}
