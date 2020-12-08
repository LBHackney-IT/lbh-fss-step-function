using Amazon.Lambda.Core;
using LbhFssStepFunction.V1;
using LbhFssStepFunction.V1.UseCase;
using LbhFssStepFunction.V1.UseCase.Interface;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace LbhFssStepFunction
{
    public class Handler
    {
        private readonly IGetOrganisationUseCase _getOrganisationUseCase;

        public Handler()
        {
            _getOrganisationUseCase = new GetOrganisationUseCase();
        }
        public OrganisationResponse SendEmail1(Request request)
        {
            return _getOrganisationUseCase.GetOrganisation(request.organisationId);
        }

        public OrganisationResponse Wait1stMonth(Request request)
        {
            return _getOrganisationUseCase.GetOrganisation(request.organisationId);
        }


    }

    public class Request
    {
        public int organisationId { get; set; }
    }
}