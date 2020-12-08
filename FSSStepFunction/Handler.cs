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

        public Handler()
        {
            _firstStepUseCase = new FirstStepUseCase();
        }
        public OrganisationResponse SendEmail1(Request request)
        {
            return _firstStepUseCase.GetOrganisationAndSendEmail(request.organisationId);
        }

        public OrganisationResponse Wait1stMonth(Request request)
        {
            return _secondStepUseCase.GetOrganisationAndSendEmail(request.organisationId);
        }


    }

    public class Request
    {
        public int organisationId { get; set; }
    }
}