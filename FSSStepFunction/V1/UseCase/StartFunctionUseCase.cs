using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using LbhFssStepFunction.V1.Boundary.Requests;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Handlers;
using LbhFssStepFunction.V1.UseCase.Interface;
using Newtonsoft.Json;

namespace LbhFssStepFunction.V1.UseCase
{
    public class StartFunctionUseCase : IStartFunctionUseCase
    {
        private readonly IOrganisationsGateway _organisationsGateway;
        private readonly string fssStateMachineArn = Environment.GetEnvironmentVariable("FSS_STEP_FUNCTION_ARN");
        private readonly string accessId = Environment.GetEnvironmentVariable("ACCESS_ID");
        private readonly string accessKey = Environment.GetEnvironmentVariable("ACCESS_KEY");
        public StartFunctionUseCase(IOrganisationsGateway gateway = null)
        {
            _organisationsGateway = gateway ?? new OrganisationsGateway();
        }

        public async Task Execute()
        {
            LoggingHandler.LogInfo("Executing request to gateway to get organisations up for review");
            var organisations = _organisationsGateway.GetOrganisationsToReview();
            if (organisations != null)
            {
                var amazonStepFunctionsConfig = new AmazonStepFunctionsConfig { RegionEndpoint = RegionEndpoint.EUWest2 };
                using (var amazonStepFunctionsClient =
                    new AmazonStepFunctionsClient(accessId, accessKey, amazonStepFunctionsConfig))
                {
                    foreach (var organisation in organisations)
                    {
                        var input = new OrganisationRequest
                        {
                            OrganisationId = organisation.Id,
                        };
                        var jsonData1 = JsonConvert.SerializeObject(input);
                        var startExecutionRequest = new StartExecutionRequest
                        {
                            Input = jsonData1,
                            Name = Guid.NewGuid().ToString(),
                            StateMachineArn = fssStateMachineArn
                        };
                        LoggingHandler.LogInfo(
                            $"Initiating state machine for organisation {organisation.Name}");
                        try
                        {
                            await amazonStepFunctionsClient.StartExecutionAsync(startExecutionRequest);
                        }
                        catch (Exception e)
                        {
                            LoggingHandler.LogError(e.Message + e.StackTrace);
                        }
                    }
                }
            }
            else
            {
                LoggingHandler.LogInfo("There are organisations are up for review today.");
            }
            LoggingHandler.LogInfo("Executing completed");
        }
    }
}