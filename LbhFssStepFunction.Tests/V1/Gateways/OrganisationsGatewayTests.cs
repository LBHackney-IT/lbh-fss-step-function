using System;
using System.Linq;
using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Gateways;
using LbhFssStepFunction.V1.Infrastructure;
using NUnit.Framework;
using FluentAssertions;

namespace LbhFssStepFunction.Tests.V1.Gateways
{
    public class OrganisationsGatewayTests : DatabaseTests
    {
        private OrganisationsGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new OrganisationsGateway(ConnectionString.TestDatabase());
        }
        
        [TestCase(TestName = "Given an organisation id when the gateway is called with the id the gateway will return an organisation that matches")]
        public void GivenAnIdAMatchingOrganisationGetsReturned()
        {
            var organisation = EntityHelpers.CreateOrganisation();
            DatabaseContext.Add(organisation);
            DatabaseContext.SaveChanges();
            var gatewayResult = _classUnderTest.GetOrganisationById(organisation.Id);
            var expectedResult = DatabaseContext.Organisations.Find(organisation.Id);
            gatewayResult.Should().NotBeNull();
            gatewayResult.Should().BeEquivalentTo(expectedResult, options =>
            {
                options.Excluding(ex => ex.UserOrganisations);
                return options;
            });
        }
        
        [TestCase(TestName = "Given organisations in the db up for review when the gateway is called these organisations are returned")]
        public void GivenOrganisationAreUpForReviewGetOrganisationsToReviewReturnsTheseOrganisations()
        {
            var organisation = EntityHelpers.CreateOrganisation();
            DatabaseContext.Add(organisation);
            DatabaseContext.SaveChanges();
            var gatewayResult = _classUnderTest.GetOrganisationById(organisation.Id);
            var expectedResult = DatabaseContext.Organisations.Find(organisation.Id);
            gatewayResult.Should().NotBeNull();
            gatewayResult.Should().BeEquivalentTo(expectedResult, options =>
            {
                options.Excluding(ex => ex.UserOrganisations);
                return options;
            });
        }
    }
}