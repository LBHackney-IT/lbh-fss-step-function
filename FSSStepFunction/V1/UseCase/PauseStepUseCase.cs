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
            try {
                LoggingHandler.LogInfo("Pause step executing request to gateway to get organisation");
            
                var organisation = _organisationsGateway.GetOrganisationById(id);
                
                if (organisation == null)
                {
                    LoggingHandler.LogInfo($"Organisation with id={id} was not found!");
                    return null;
                }
                else if (!organisation.InRevalidationProcess)
                {
                    LoggingHandler.LogInfo($"Organisation with id={id} has been reverified since Step 3 execution.");
                    return null;
                }
                else
                {
                    LoggingHandler.LogInfo($"Organisation with id={id} was found, attempting to pause it.");
                    _organisationsGateway.PauseOrganisation(organisation.Id);

                    var response = organisation.ToResponse();

                    await _notifyGateway
                        .SendNotificationEmail(
                            response.OrganisationName,
                            response.EmailAddresses.ToArray(),
                            4);

                    LoggingHandler.LogInfo($"Organisation with id={id} was paused, exiting Pause Step use case");

                    response.StateResult = true; // symbollic return

                    return response;
                }
            }
            // blocks could be used to execute custom logic, however, not much can be done while exception handling
            // approach hasn't yet been discussed and agreed on.  
            catch (ResourceNotFoundException ex) {
                LoggingHandler.LogInfo($"Organisation with id={id} was found, but could not be accessed from 'PauseOrganisation Gateway method.");
                LoggingHandler.LogInfo($"Logged message:\n{ex.Message}");
                if (ex.InnerException != null)
                    LoggingHandler.LogInfo($"Logged inner message:\n{ex?.InnerException?.Message}");
                throw ex; // terminate execution, force retry.
            }
            catch (Exception ex) {
                LoggingHandler.LogInfo($"Unexpected exception occured while calling Pause Step use case with id={id}.");
                LoggingHandler.LogInfo($"Logged message:\n{ex.Message}");
                if (ex.InnerException != null)
                    LoggingHandler.LogInfo($"Logged inner message:\n{ex?.InnerException?.Message}");
                throw ex;
            }
        }
    }
}