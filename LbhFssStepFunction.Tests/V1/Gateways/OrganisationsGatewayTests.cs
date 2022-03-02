using System;
using System.Linq;
using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Gateways;
using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LbhFssStepFunction.V1.Errors;

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
            var organisations = EntityHelpers.CreateOrganisations(10).ToList();
            organisations[0].Status = "Published";
            organisations[0].LastRevalidation = DateTime.Today.AddDays(-370);
            organisations[0].InRevalidationProcess = false;
            organisations[1].Status = "Published";
            organisations[1].LastRevalidation = DateTime.Today.AddDays(-375);
            organisations[1].InRevalidationProcess = false;
            organisations[2].Status = "Published";
            organisations[2].LastRevalidation = DateTime.Today.AddDays(-380);
            organisations[2].InRevalidationProcess = false;
            organisations[3].Status = "Paused";
            organisations[3].LastRevalidation = DateTime.Today.AddDays(-370);
            organisations[3].InRevalidationProcess = false;
            organisations[4].Status = "Published";
            organisations[4].LastRevalidation = DateTime.Today.AddDays(-340);
            organisations[4].InRevalidationProcess = false;
            organisations[5].Status = "Published";
            organisations[5].LastRevalidation = DateTime.Today.AddDays(-370);
            organisations[5].InRevalidationProcess = true;
            DatabaseContext.AddRange(organisations);
            DatabaseContext.SaveChanges();
            var gatewayResult = _classUnderTest.GetOrganisationsToReview();
            gatewayResult.Count.Should().Be(3);
            var expectedResult = organisations.Take(3);
            gatewayResult.Should().BeEquivalentTo(expectedResult, options =>
            {
                options.Excluding(ex => ex.UserOrganisations);
                return options;
            });
        }

        [TestCase(TestName =
            "Given an organisation id when the gateway PauseOrganisation method is called the organisation's status is set to Paused")]
        public void GivenAnOrganisationIdWhenThePauseOrganisationMethodIsCalledTheOrganisationIsSetToPaused()
        {
            var organisation = EntityHelpers.CreateOrganisation();
            DatabaseContext.Add(organisation);
            DatabaseContext.SaveChanges();
            var result = _classUnderTest.PauseOrganisation(organisation.Id);
            result.Status.Should().Be("Paused");
        }

        [TestCase(TestName = @"
            Given an organisation id of existing organisation, 
            When the FlagOrganisationToBeInRevalidation Gateway method is called, 
            Then the organisation's record in the database gets flagged to be in the re-verification process")]
        public void UpdatingExistingOrgsReverificationStatusWorks()
        {
            // arrange
            var organisation = EntityHelpers.CreateOrganisation();
            organisation.InRevalidationProcess = false;
            DatabaseContext.Add(organisation);
            DatabaseContext.SaveChanges();

            int existingOrganisationId = organisation.Id;

            // act
            _classUnderTest.FlagOrganisationToBeInRevalidation(existingOrganisationId);

            DatabaseContext.Entry(organisation).State = EntityState.Detached; // Ruling out the change tracker

            // assert
            var retrievedOrganisation = DatabaseContext.Organisations.First(o => o.Id == existingOrganisationId);
            retrievedOrganisation.InRevalidationProcess.Should().BeTrue();
        }

        [TestCase(TestName = @"
            Given an organisation id of non-existent organisation,
            When the FlagOrganisationToBeInRevalidation Gateway method is called,
            Then the execution flow terminates with ResourceNotFound exception")]
        public void AttemptinToUpdateNonExistentOrgThrowsAnException()
        {
            // arrange
            int randomOrgId = Randomm.Id(100, 200);

            // act
            Action testMethodCall = () => _classUnderTest.FlagOrganisationToBeInRevalidation(randomOrgId);

            // assert
            testMethodCall.Should().Throw<ResourceNotFoundException>();
        }
    }
}
