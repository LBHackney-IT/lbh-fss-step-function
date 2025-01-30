using System.Collections.Generic;
using LbhFssStepFunction.V1.Domains;

namespace LbhFssStepFunction.V1.Gateways.Interface
{
    public interface IOrganisationsGateway
    {
        OrganisationDomain GetOrganisationById(int id);
        List<OrganisationDomain> GetOrganisationsToReview();
        void PauseOrganisation(int id);
        void FlagOrganisationToBeInRevalidation(int id);
    }
}