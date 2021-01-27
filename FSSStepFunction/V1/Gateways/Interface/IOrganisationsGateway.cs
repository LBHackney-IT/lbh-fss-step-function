using System.Collections.Generic;
using LbhFssStepFunction.V1.Domains;

namespace LbhFssStepFunction.V1.Gateways.Interface
{
    public interface IOrganisationsGateway
    {
        OrganisationDomain GetOrganisationById(int id);
        List<OrganisationDomain> GetOrganisationsToReview();
        OrganisationDomain PauseOrganisation(int id);
    }
}