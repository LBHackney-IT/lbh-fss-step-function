using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LbhFssStepFunction.V1.Domains;
using LbhFssStepFunction.V1.Infrastructure;

namespace LbhFssStepFunction.V1.Factories
{
    public static class UserOrganisationsFactory
    {
        public static UserOrganisationDomain ToDomain(this UserOrganisationEntity userOrganisationEntity)
        {
            return new UserOrganisationDomain
            {
                Id = userOrganisationEntity.Id,
                UserId = userOrganisationEntity.UserId,
                OrganisationId = userOrganisationEntity.OrganisationId,
                CreatedAt = userOrganisationEntity.CreatedAt,
                User = userOrganisationEntity.User.ToDomain(),
            };
        }

        public static List<UserOrganisationDomain> ToDomain(
            this ICollection<UserOrganisationEntity> userOrganisationEntities)
        {
            return userOrganisationEntities == null
                ? null
                : userOrganisationEntities.Select(uO => uO.ToDomain()).ToList();
        }
    }
}