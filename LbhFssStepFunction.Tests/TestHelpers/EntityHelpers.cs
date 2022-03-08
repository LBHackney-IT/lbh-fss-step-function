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

        #region Organisations
        public static OrganisationEntity CreateOrganisation(bool withId = false)
        {
            var organisationBuilder = Randomm.Build<OrganisationEntity>().Do(_=>{});
            
            if (!withId) organisationBuilder = organisationBuilder.Without(o => o.Id);
                
            return organisationBuilder.Create();
        }

        public static ICollection<OrganisationEntity> CreateOrganisations(int count = 3)
        {
            return NCustomItems(createEntity: () => CreateOrganisation(), quantity: count);
        }

        #endregion
        #region Users

        public static UserEntity CreateUser(
            ICollection<UserOrganisationEntity> userOrganisations = null, 
            bool withId = false)
        {
            var userBuilder = Randomm.Build<UserEntity>().Do(_=>{});
            
            if (!withId) userBuilder = userBuilder.Without(u => u.Id);
            if (userOrganisations != null)
                userBuilder = userBuilder.With(u => u.UserOrganisations, userOrganisations);

            return userBuilder.Create();
        }

        #endregion
        #region User Organisations (link entity)

        public static UserOrganisationEntity CreateUserOrganisation(
            UserEntity user = null,
            OrganisationEntity organisation = null, 
            bool withId = false)
        {
            var uoBuilder = Randomm.Build<UserOrganisationEntity>().Do(_=>{});

            if (!withId) uoBuilder = uoBuilder.Without(uo => uo.Id);
            if (user != null) uoBuilder = uoBuilder
                .With(uo => uo.User, user)
                .With(uo => uo.UserId, user.Id);
            if (organisation != null) uoBuilder = uoBuilder
                .With(uo => uo.Organisation, organisation)
                .With(uo => uo.OrganisationId, organisation.Id);

            return uoBuilder.Create();
        }

        #endregion
        #region Create Graph

        /* Generates: [1 Organisation --> Many User Organisations --> 1 User]
        We don't care about 1 User --> Many User Organisations relationship
        for this particular repository due to it focusing on processing based
        organisation state data, not user state data.*/
        public static OrganisationEntity CreateOrganisationWithUsers(
            int count = 3, bool withIds = false, bool activeUsers = false)
        {
            var organisation = CreateOrganisation(withId: withIds);

            // Create user, its user organisations link will be populated later.
            Func<UserEntity> userCreateAction = () => CreateUser(withId: withIds);
            
            // Take user and connect it to userOrganisation; Also wire-up organisation with userOrganisation.
            Func<UserEntity,UserOrganisationEntity> uoCreateAction = 
                (user) => CreateUserOrganisation(user, organisation, withId: withIds);

            var users = NCustomItems(userCreateAction, count);

            // Create and Wire-Up User Organisations with Users both ways.
            var userOrganisations = users.Select(u => {
                if (activeUsers) u.Status = "active"; // there exists some conditional logic based on this
                var userOrg = uoCreateAction(u);
                userOrg.User.UserOrganisations =
                    new List<UserOrganisationEntity> {userOrg};
                return userOrg;
            }).ToList();

            // Wire up user organisation to organisation
            organisation.UserOrganisations = userOrganisations;

            return organisation;
        }

        #endregion 
        #region Helper

        public static ICollection<T> NCustomItems<T>(Func<T> createEntity, int quantity)
        {
            return Enumerable.Range(1, quantity).Select(_ => createEntity()).ToList();
        }

        #endregion
    }
}
