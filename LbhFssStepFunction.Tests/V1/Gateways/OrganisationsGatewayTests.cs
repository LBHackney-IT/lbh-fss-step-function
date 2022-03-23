using System;
using System.Collections.Generic;
using System.Linq;
using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Gateways;
using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LbhFssStepFunction.V1.Errors;
using LbhFssStepFunction.V1.Domains;

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
            Given the Database contains Organisations with expired data (t > 1y old),
            When the GetOrganisationsToReview Gateway method is called,
            Then it returns ONLY organisations that have expired.")]
        public void GetOrganisationsToReviewReturnsOrganisationInCorrectDateRange()
        {
            // arrange
            var testOrganisations = EntityHelpers.CreateOrganisations().ToList();
            testOrganisations.ForEach(o => {
                // Setting other filtering related properties as constant to isolate date filter testing.
                o.InRevalidationProcess = false;
                o.Status = "Published";
                // 365th day is the last allowed day, hence should be ignored.
                // The implementation does not concern itself with precision when it comes to leap year.
                // Method should work even when Org. data has expired close to 2years ago 
                // (current state of some of the data at the time of this commit)
                o.LastRevalidation = DateTime.Today.AddDays(Randomm.Int(-720, -366));
            });

            var controlOrganisations = EntityHelpers.CreateOrganisations(10).ToList();
            controlOrganisations.ForEach(o => {
                // Ensuring control Orgs. don't clash with the test ones for the purposes of the test.
                o.LastRevalidation = DateTime.Today.AddDays(Randomm.Int(-365, 0));
                // Setting these properties like this should show that the Date range is independent
                // of these other filter parameters.
                o.InRevalidationProcess = Randomm.EqualChanceItems(false, true);
                o.Status = Randomm.EqualChanceItems("Published", "Paused", "Awaiting Review", "Rejected");
            });

            var organisationsPool = testOrganisations.Concat(controlOrganisations).ToList();
            DatabaseContext.AddRange(organisationsPool);
            DatabaseContext.SaveChanges();

            organisationsPool.ForEach(
                o => DatabaseContext.Entry(o).State = EntityState.Detached);

            // act
            var expiredOrgsList = _classUnderTest.GetOrganisationsToReview();

            // assert
            expiredOrgsList.Should().HaveCount(3);
            expiredOrgsList.Should().BeEquivalentTo(testOrganisations, options =>
            {
                options.Excluding(ex => ex.UserOrganisations);
                return options;
            });
        }

        [TestCase(TestName = @"
            Given the Database contains Organisations with expired data,
            And some of them are 'Published',
            When the GetOrganisationsToReview Gateway method is called,
            Then it returns expired organisations ONLY if they're Published.")]
        public void GetOrganisationsToReviewReturnsOnlyPublishedExpiredOrganisations()
        {
            // arrange
            var testOrganisations = EntityHelpers.CreateOrganisations(4).ToList();
            testOrganisations.ForEach(o => {
                // Setting other filtering related properties as constant to isolate organisation status testing.
                o.InRevalidationProcess = false;
                o.LastRevalidation = DateTime.Today.AddDays(Randomm.Int(-1000, -366));
                // Organisation has to be "Published" - effectively means it was validated to be real
                // by Hackney staff. If it wasn't validated to be real, there's no point to re-verify anything.
                o.Status = "Published";
            });

            var controlOrganisations = EntityHelpers.CreateOrganisations(10).ToList();
            controlOrganisations.ForEach(o => {
                // Ensuring control Orgs. don't clash with the test ones for the purposes of the test.
                // Status has to be anything but "Published", doesn't even matter if it's a real value or not. 
                o.Status = Randomm.EqualChanceItems("Paused", "Awaiting Review", "Rejected");
                // Setting these other parameters as random to test that organisation status filtering is
                // independent of other filter parameters.
                o.InRevalidationProcess = Randomm.EqualChanceItems(false, true);
                o.LastRevalidation = DateTime.Today.AddDays(Randomm.Int(-720, 0));
            });

            var organisationsPool = testOrganisations.Concat(controlOrganisations).ToList();
            DatabaseContext.AddRange(organisationsPool);
            DatabaseContext.SaveChanges();

            organisationsPool.ForEach(
                o => DatabaseContext.Entry(o).State = EntityState.Detached);

            // act
            var expiredOrgsList = _classUnderTest.GetOrganisationsToReview();

            // assert
            expiredOrgsList.Should().HaveCount(4);
            expiredOrgsList.Should().BeEquivalentTo(testOrganisations, options =>
            {
                options.Excluding(ex => ex.UserOrganisations);
                return options;
            });
        }

        [TestCase(TestName = @"
            Given the Database contains published & expired Organisations,
            And some of them are NOT In Revalidation Process,
            When the GetOrganisationsToReview Gateway method is called,
            Then it returns ONLY the published, expired organisations that are NOT InRevalidation process")]
        public void GetOrganisationsToReviewReturnsOnlyOrganisationsThatAreNotYetInRevalidation()
        {
            var testOrganisations = EntityHelpers.CreateOrganisations(2).ToList();
            testOrganisations.ForEach(o => {
                // Setting other filtering related properties as constant to isolate organisation in-revalidation filter testing.
                o.LastRevalidation = DateTime.Today.AddDays(Randomm.Int(-1000, -366));
                o.Status = "Published";
                // Organisation has to NOT be in Revalidation process already, if it is then it in revalidation process,
                // then it means that the Step function has already triggered the process for the organisation.
                // No need to do it twice.
                o.InRevalidationProcess = false;
            });

            var controlOrganisations = EntityHelpers.CreateOrganisations(10).ToList();
            controlOrganisations.ForEach(o => {
                // Ensuring control Orgs. don't clash with the test ones for the purposes of the test.
                o.InRevalidationProcess = true;
                // Setting these other parameters as random to test that organisation in revalidation
                // filtering is independent of other filter parameters.
                o.LastRevalidation = DateTime.Today.AddDays(Randomm.Int(-720, 0));
                o.Status = Randomm.EqualChanceItems("Published", "Paused", "Awaiting Review", "Rejected");
            });

            var organisationsPool = testOrganisations.Concat(controlOrganisations).ToList();
            DatabaseContext.AddRange(organisationsPool);
            DatabaseContext.SaveChanges();

            organisationsPool.ForEach(
                o => DatabaseContext.Entry(o).State = EntityState.Detached);

            // act
            var expiredOrgsList = _classUnderTest.GetOrganisationsToReview();

            // assert
            expiredOrgsList.Should().HaveCount(2);
            expiredOrgsList.Should().BeEquivalentTo(testOrganisations, options =>
            {
                options.Excluding(ex => ex.UserOrganisations);
                return options;
            });
        }

        [TestCase(TestName = @"
            Given the Database does NOT contain organisations that are at the same time all: Expired,
            And 'Published',
            And NOT in Revalidation,
            When GetOrganisationsToReview Gateway method is called,
            Then it returns an EMPTY collection.")]
        public void GetOrganisationsToReviewReturnsEmptyCollectionWhenOrganisationsWithMatchingCriteriaAreNotFound()
        {
            // arrange
            var controlOrganisations = EntityHelpers.CreateOrganisations(10).ToList();
            controlOrganisations.ForEach(o => {
                // At this point, the test is in showing that EMPTY collection gets returned when there is NO
                // match at all (unlike previous tests). The setting of all 3 of these fields as invalid 
                // is just a symbollic gesture to make the point of the test come accross better.
                // Also due to these being non-matching entities, there's no need to do entity tracker setup.
                o.Status = Randomm.EqualChanceItems("Paused", "Awaiting Review", "Rejected");
                o.InRevalidationProcess = true;
                o.LastRevalidation = DateTime.Today.AddDays(Randomm.Int(-365, 0));
            });

            DatabaseContext.AddRange(controlOrganisations);
            DatabaseContext.SaveChanges();

            // act
            var expiredOrgsList = _classUnderTest.GetOrganisationsToReview();

            // assert
            expiredOrgsList.Should().NotBeNull();
            expiredOrgsList.Should().BeEmpty();
        }

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
            _classUnderTest.PauseOrganisation(organisation.Id);

            DatabaseContext.Entry(organisation).State = EntityState.Detached;

            // assert
            var retrievedOrganisation = DatabaseContext.Organisations.First(o => o.Id == organisation.Id);
            retrievedOrganisation.Status.Should().Be("Paused");
        }

        [TestCase(TestName = @"
            Given a NOT existing organisation id,
            When the PauseOrganisation Gateway method is called,
            Then the execution flow terminates with ResourceNotFound exception")]
        public void AttemptingToPauseNonExistentOrgThrowsException()
        {
            // arrange
            var controlOrganisation = EntityHelpers.CreateOrganisation();
            // test that it won't default to some random or first in the list organisation
            DatabaseContext.Add(controlOrganisation);
            DatabaseContext.SaveChanges();

            int randomOrgId = Randomm.Id(minimum: 300);

            // act
            Action testMethodCall = () => _classUnderTest.PauseOrganisation(randomOrgId);

            // assert
            testMethodCall.Should().Throw<ResourceNotFoundException>();
        }

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
        public void AttemptingToUpdateNonExistentOrgThrowsAnException()
        {
            // arrange
            var controlOrganisation = EntityHelpers.CreateOrganisation();
            // test that it won't default to some random or first in the list organisation
            DatabaseContext.Add(controlOrganisation);
            DatabaseContext.SaveChanges();

            int randomOrgId = Randomm.Id(100, 200);

            // act
            Action testMethodCall = () => _classUnderTest.FlagOrganisationToBeInRevalidation(randomOrgId);

            // assert
            testMethodCall.Should().Throw<ResourceNotFoundException>();
        }

        # endregion
    }
}
