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
        private readonly IFirstStepUseCase _firstStepUseCase;
        private readonly ISecondStepUseCase _secondStepUseCase;
        private readonly IThirdStepUseCase _thirdStepUseCase;
        private readonly IPauseStepUseCase _pauseStepUseCase;
        private readonly IStartFunctionUseCase _startFunctionUseCase;

        public Handler(IStartFunctionUseCase startFunctionUseCase = null,
        IFirstStepUseCase firstStepUseCase = null,
        ISecondStepUseCase secondStepUseCase = null,
        IThirdStepUseCase thirdStepUseCase = null,
        IPauseStepUseCase pauseStepUseCase = null)
        {
            _startFunctionUseCase = startFunctionUseCase ?? new StartFunctionUseCase();
            _firstStepUseCase = firstStepUseCase ?? new FirstStepUseCase();
            _secondStepUseCase = secondStepUseCase ?? new SecondStepUseCase();
            _thirdStepUseCase = thirdStepUseCase ?? new ThirdStepUseCase();
            _pauseStepUseCase = pauseStepUseCase ?? new PauseStepUseCase();
        }

        public void StartFunction()
        {
            LoggingHandler.LogInfo("Organisation review scheduled job started");
            _startFunctionUseCase.Execute();
        }
        public async Task<OrganisationResponse> FirstStep(OrganisationRequest request)
        {
            return await _firstStepUseCase.GetOrganisationAndSendEmail(request.OrganisationId).ConfigureAwait(true);
        }

        public async Task<OrganisationResponse> SecondStep(OrganisationRequest request)
        {
            return await _secondStepUseCase.GetOrganisationAndSendEmail(request.OrganisationId).ConfigureAwait(true);
        }

        public async Task<OrganisationResponse> ThirdStep(OrganisationRequest request)
        {
            return await _thirdStepUseCase.GetOrganisationAndSendEmail(request.OrganisationId).ConfigureAwait(true);
        }

        public async Task<OrganisationResponse> PauseStep(OrganisationRequest request)
        {
            return await _pauseStepUseCase.GetOrganisationAndSendEmail(request.OrganisationId).ConfigureAwait(true);
        }

    }
}