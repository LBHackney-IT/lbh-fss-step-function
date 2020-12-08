using System.Collections.Generic;
using LbhFssStepFunction.V1.Domains;
using LbhFssStepFunction.V1.Infrastructure;

namespace LbhFssStepFunction.V1.Factories
{
    public static class OrganisationsFactory
    {
        public static OrganisationDomain ToDomain(this OrganisationEntity organisationEntity)
        {
            return new OrganisationDomain
            {
                Id = organisationEntity.Id,
                Name = organisationEntity.Name,
                Status = organisationEntity.Status,
                CreatedAt = organisationEntity.CreatedAt,
                SubmittedAt = organisationEntity.SubmittedAt,
                ReviewedAt = organisationEntity.ReviewedAt,
                UpdatedAt = organisationEntity.UpdatedAt,
                UserOrganisations = organisationEntity.UserOrganisations.ToDomain()
            };
        }

        public static OrganisationResponse ToResponse(this OrganisationDomain organisationDomain)
        {
            if (organisationDomain == null)
                return null;
            var orgResponse = new OrganisationResponse
            {
                Id = organisationDomain.Id,
                OrganisationName = organisationDomain.Name,
                Status = organisationDomain.Status,
                EmailAddresses = new List<string>()
            };
            foreach (var userOrg in organisationDomain.UserOrganisations)
            {
                if (userOrg.User.Status.ToLower() == "active")
                {
                    orgResponse.EmailAddresses.Add(userOrg.User.Email);
                }
            }
            return orgResponse;
        }
    }
}