using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LbhFssStepFunction.V1;
using LbhFssStepFunction.V1.Boundary.Requests;
using LbhFssStepFunction.V1.Handlers;
using LbhFssStepFunction.V1.UseCase;
using LbhFssStepFunction.V1.UseCase.Interface;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace LbhFssStepFunction
{
    public class Handler
    {
        private readonly IStartFunctionUseCase _startFunctionUseCase;
        private readonly IFirstStepUseCase _firstStepUseCase;
        private readonly IReminderToReminderUseCase _reminderToReminderUC;
        private readonly IPauseStepUseCase _pauseStepUseCase;

        public Handler()
        {
            _startFunctionUseCase = new StartFunctionUseCase();
            _firstStepUseCase = new FirstStepUseCase();
            _reminderToReminderUC = new ReminderToReminderUseCase();
            _pauseStepUseCase = new PauseStepUseCase();
        }

        // If we're using this constructor overload, then we don't need the above one.
        // MAYBE TODO: Refactor this. Unless the parameterless CTOR is explicitly needed by AWS Host.
        public Handler(
            IStartFunctionUseCase startFunctionUseCase = null,
            IFirstStepUseCase firstStepUseCase = null,
            IReminderToReminderUseCase reminderToReminderUC = null,
            ISecondStepUseCase secondStepUseCase = null,
            IThirdStepUseCase thirdStepUseCase = null,
            IPauseStepUseCase pauseStepUseCase = null)
        {
            _startFunctionUseCase = startFunctionUseCase ?? new StartFunctionUseCase();
            _firstStepUseCase = firstStepUseCase ?? new FirstStepUseCase();
            _reminderToReminderUC = reminderToReminderUC ?? new ReminderToReminderUseCase();
            _pauseStepUseCase = pauseStepUseCase ?? new PauseStepUseCase();
        }

        public void StartFunction()
        {
            LoggingHandler.LogInfo("Organisation review scheduled job started");
            _startFunctionUseCase.Execute();
        }
        public async Task<OrganisationResponse> FirstStep(OrganisationRequest request)
        {
            return await _firstStepUseCase.GetOrganisationAndSendEmail(request.OrganisationId);
        }

        public async Task<OrganisationResponse> SecondStep(OrganisationRequest request)
        {
            return await _reminderToReminderUC
                .GetOrganisationAndSendEmail(request.OrganisationId, 2);
        }

        public async Task<OrganisationResponse> ThirdStep(OrganisationRequest request)
        {
            return await _reminderToReminderUC
                .GetOrganisationAndSendEmail(request.OrganisationId, 3);
        }

        public async Task<OrganisationResponse> PauseStep(OrganisationRequest request)
        {
            return await _pauseStepUseCase.GetOrganisationAndSendEmail(request.OrganisationId);
        }
    }
}