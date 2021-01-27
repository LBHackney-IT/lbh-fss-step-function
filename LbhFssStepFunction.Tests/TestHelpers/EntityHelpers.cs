using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LbhFssStepFunction.V1.Infrastructure;

namespace LbhFssStepFunction.Tests.TestHelpers
{
    public static class EntityHelpers
    {
        const int _count = 3;

        public static OrganisationEntity CreateOrganisation()
        {
            var organisation = Randomm.Build<OrganisationEntity>()
                .Without(o => o.Id)
                .Create();
            return organisation;
        }

        public static ICollection<OrganisationEntity> CreateOrganisations(int count = 3)
        {
            var organisations = new List<OrganisationEntity>();
            for (var a = 0; a < count; a++)
            {
                organisations.Add(CreateOrganisation());
            }
            return organisations;
        }

    }
}
