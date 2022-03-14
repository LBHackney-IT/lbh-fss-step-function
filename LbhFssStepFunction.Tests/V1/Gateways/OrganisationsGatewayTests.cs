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

        #region Get Single Organisation

        [TestCase(TestName = @"
            Given an existing organisation id,
            When the GetOrganisationById Gateway method is called with the id,
            Then the Gateway will return an organisation that matches")]
        public void GivenAnIdAMatchingOrganisationGetsReturned()
        {
            // arrange
            var controlOrganisation = EntityHelpers.CreateOrganisation();
            var testOrganisation = EntityHelpers.CreateOrganisation();

            DatabaseContext.AddRange(controlOrganisation, testOrganisation);
            DatabaseContext.SaveChanges();

            DatabaseContext.Entry(testOrganisation).State = EntityState.Detached; // Ignore change tracker

            // act
            var retrievedOrganisation = _classUnderTest.GetOrganisationById(testOrganisation.Id);

            // assert
            retrievedOrganisation.Should().NotBeNull();
            retrievedOrganisation.Should().BeEquivalentTo(testOrganisation, options =>
            {
                options.Excluding(ex => ex.UserOrganisations);
                return options;
            });
        }

        // Not found... return null
        [TestCase(TestName = @"
            Given a NOT existing organisation id,
            When the GetOrganisationById Gateway method is called,
            Then it returns NULL result.")]
        public void GetSingleOrganisationReturnsNullWhenCalledWithNotExistingId()
        {
            // arrange
            var controlEntity = EntityHelpers.CreateOrganisation();
            DatabaseContext.Add(controlEntity);
            DatabaseContext.SaveChanges(); // We don't return null just because DB is empty

            var randomId = Randomm.Id(minimum: 200);

            // act
            var retrievedOrganisation = _classUnderTest.GetOrganisationById(randomId);

            // assert
            retrievedOrganisation.Should().BeNull();
        }

        #endregion
        #region Get Multiple Organisations

        [TestCase(TestName = @"
            Given organisations in the db up for review,
            When the gateway is called,
            Then these organisations are returned")]
        public void GivenOrganisationAreUpForReviewGetOrganisationsToReviewReturnsTheseOrganisations()
        {
            // arrange
            var organisations = EntityHelpers.CreateOrganisations(10).ToList();

            // TODO: rewrite this to be more concise
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

            // act
            var gatewayResult = _classUnderTest.GetOrganisationsToReview();

            // assert
            gatewayResult.Count.Should().Be(3);
            var expectedResult = organisations.Take(3);
            gatewayResult.Should().BeEquivalentTo(expectedResult, options =>
            {
                options.Excluding(ex => ex.UserOrganisations);
                return options;
            });
        }

        // The above is... only the correct ones are returned.
        // Returns nothing when nothing is in DB, or when nothing is available
        // Published/Non-Published tested separatelly
        // Date range tested separately

        #endregion
        #region Pause Organisation

        [TestCase(TestName = @"
            Given an existing organisation id,
            When the PauseOrganisation Gateway method is called,
            Then the organisation's status is set to Paused")]
        public void OrganisationIsPausedWhenAMethodIsCalledWithAnExistingId()
        {
            // arrange
            var organisation = EntityHelpers.CreateOrganisation();
            DatabaseContext.Add(organisation);
            DatabaseContext.SaveChanges();

            // act
            var result = _classUnderTest.PauseOrganisation(organisation.Id);

            // assert
            result.Status.Should().Be("Paused");
        }

        // Given non-existing organisation Id, method throws RecordNotFoundException
        // + See into the returns of the method

        #endregion
        #region Flag: In Revalidation

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

        # endregion
    }
}
