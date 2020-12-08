using LbhFssStepFunction.V1.Domains;

namespace LbhFssStepFunction.V1.Gateways.Interface
{
    public interface IOrganisationGateway
    {
        OrganisationDomain GetOrganisationById(int id);
    }
}