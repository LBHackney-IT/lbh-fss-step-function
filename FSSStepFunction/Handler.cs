using Amazon.Lambda.Core;
using LbhFssStepFunction.V1;
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

        public Handler()
        {
            _firstStepUseCase = new FirstStepUseCase();
            _secondStepUseCase = new SecondStepUseCase();
        }
        public OrganisationResponse FirstStep(Request request)
        {
            return _firstStepUseCase.GetOrganisationAndSendEmail(request.organisationId);
        }

        public OrganisationResponse SecondStep(Request request)
        {
            return _secondStepUseCase.GetOrganisationAndSendEmail(request.organisationId);
        }

        public OrganisationResponse ThirdStep(Request request)
        {
            return _thirdStepUseCase.GetOrganisationAndSendEmail(request.organisationId);
        }

        public OrganisationResponse PauseStep(Request request)
        {
            return _pauseStepUseCase.GetOrganisationAndSendEmail(request.organisationId);
        }



    }

    public class Request
    {
        public int organisationId { get; set; }
    }
}