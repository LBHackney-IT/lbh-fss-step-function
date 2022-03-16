using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Domains;
using LbhFssStepFunction.V1.Errors;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.UseCase;
using Moq;
using NUnit.Framework;

namespace LbhFssStepFunction.Tests.V1.UseCase
{
    [TestFixture]
    public class pauseStepUseCaseTests
    {
        private PauseStepUseCase _classUnderTest;
        private Mock<IOrganisationsGateway> _mockOrganisationGateway;
        private Mock<INotifyGateway> _mockNotifyGateway;

        [SetUp]
        public void Setup()
        {
            _mockOrganisationGateway = new Mock<IOrganisationsGateway>();
            _mockNotifyGateway = new Mock<INotifyGateway>();
            _classUnderTest = new PauseStepUseCase(_mockOrganisationGateway.Object, _mockNotifyGateway.Object);
        }

        [TestCase(TestName = @"
            Given any input organisation id,
            When the Pause Step use case gets called,
            Then it calls the GetOrganisationsToReview organisation gateway method
            With correct parameters.")]
        public async Task PauseStepUseCaseCallsOrganisationGateway()
        {
            // arrange
            int anyId = Randomm.Id();

            // act
            await _classUnderTest.GetOrganisationAndSendEmail(anyId);
            
            // assert
            _mockOrganisationGateway.Verify(
                gw => gw.GetOrganisationById(It.Is<int>(id => id == anyId)),
                Times.Once);
        }

        [TestCase(TestName = @"
            Given an existing Organisation Id,
            And that organisation being in Revalidation process,
            When the Pause Step use case gets called,
            Then it calls the PauseOrganisation organisation gateway method with Correct parameters.")]
        public async Task PauseStepUseCaseCallsOrgnisationGatewayPauseMethodWithCorrectParameters()
        {
            // arrange
            var organisation = EntityHelpers.CreateOrganisationWithUsers(withIds: true);
            int existingId = organisation.Id;
            organisation.InRevalidationProcess = true;

            var domainOrganisation = organisation.ToDomain();
            
            _mockOrganisationGateway
                .Setup(og => og.GetOrganisationById(It.Is<int>(id => id == existingId)))
                .Returns(domainOrganisation);

            // act
            await _classUnderTest.GetOrganisationAndSendEmail(existingId);
            
            // assert
            _mockOrganisationGateway.Verify(
                gw => gw.PauseOrganisation(It.Is<int>(id => id == existingId)),
                Times.Once);
        }

        [TestCase(TestName = @"
            Given an existing Organisation Id,
            And that organisation being in Revalidation process, 
            When the Pause Step use case gets called,
            Then it calls the SendNotificationEmail notify gateway method with correct parameters,
            And it returns the correct organisation & Next Step state data.")]
        public async Task PauseStepUseCaseCallsNotificationGatewayWithCorrectParamsIfOrganisationWasNotUpdated()
        {
            // arrange
            var organisation = EntityHelpers.CreateOrganisationWithUsers(withIds: true, activeUsers: true);
            int existingId = organisation.Id;
            // Implies that organisation wasn't updated since Step 3 was called.
            organisation.InRevalidationProcess = true;

            var expectedNotifyEmailArgs = organisation.UserOrganisations.Select(uo => uo.User.Email).ToList();

            var domainOrganisation = organisation.ToDomain();
            
            _mockOrganisationGateway
                .Setup(og => og.GetOrganisationById(It.Is<int>(id => id == existingId)))
                .Returns(domainOrganisation);
            
            // act
            var ucResult = await _classUnderTest.GetOrganisationAndSendEmail(existingId);
            
            // assert
            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.Is<string>(n => n == domainOrganisation.Name), 
                    It.Is<string[]>(es => 
                        es.All(e => expectedNotifyEmailArgs.Contains(e)) && 
                        es.Count() == expectedNotifyEmailArgs.Count()), 
                    It.Is<int>(state => state == 4)), 
                Times.Once);

            ucResult.Should().NotBeNull();
            ucResult.EmailAddresses.Should().BeEquivalentTo(expectedNotifyEmailArgs);
            // Next step time should be ±2 seconds from the time the UC was executed. Should be accurate enough for testing purposes.
            ucResult.StateResult.Should().BeTrue(); // indicate success //(default(bool));
            ucResult.OrganisationId.Should().Be(existingId); // the rest of the organisations data is irrelevant.
            ucResult.NextStepTime.Should().BeCloseTo(default(DateTime), precision: 2000); // shows datetime is irrelevant
        }

        // existing not valid
        [TestCase(TestName = @"
            Given an existing Organisation Id,
            And that organisation NOT being in Revalidation process,
            When the Pause Step use case gets called,
            Then it does NOT call the SendNotfication notify gateway's method,
            And it returns a NULL result.")]
        public async Task PauseStepUCDoesNotCallNotifyGWAndReturnsNullWhenOrganisationWasUpdated()
        {
            // arrange
            var organisation = EntityHelpers.CreateOrganisationWithUsers(withIds: true, activeUsers: true);
            int existingId = organisation.Id;
            
            // This being false implies that organisation was updated between the Step 3 and Pause Step.
            organisation.InRevalidationProcess = false;

            var domainOrganisation = organisation.ToDomain();

            _mockOrganisationGateway
                .Setup(gw => gw.GetOrganisationById(It.Is<int>(id => id == existingId)))
                .Returns(domainOrganisation);

            // act
            var ucResult = await _classUnderTest.GetOrganisationAndSendEmail(existingId);

            // assert
            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()),
                Times.Never);

            ucResult.Should().BeNull();
            // I think in this particular case it would make more sense to return 'false'
            // since that's not so much an error, but rather an invalid operation.
        }

        [TestCase(TestName = @"
            Given a NOT existing Organisation Id,
            When the Pause Step Usecase gets called,
            Then it does NOT call SendNotfication notify gateway's method,
            And it returns a NULL result.")]
        public async Task PauseStepUCDoesNotCallNotifyGWAndReturnsNullWhenOrganisationIsNotFound()
        {
            // arrange
            int nonExistingId = Randomm.Id();
            // organisation was deleted between the triggering of Step 3 and Pause Step.
            OrganisationDomain organisation = null;

            _mockOrganisationGateway
                .Setup(gw => gw.GetOrganisationById(It.Is<int>(id => id == nonExistingId)))
                .Returns(organisation);

            // act
            var ucResult = await _classUnderTest.GetOrganisationAndSendEmail(nonExistingId);

            // assert
            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()),
                Times.Never);
            
            ucResult.Should().BeNull();
            // This one is well deserved NULL since we don't even know what the organisation is anymore.
        }

        [TestCase(TestName = @"
            Given an existing organisation's Id,
            In an unlikely case of organisation getting deleted or inaccessible immediatelly after retrieving it,
            When the Pause Step use case's GetOrganisationAndSendEmail method is called,
            And UC calls PauseOrganisation notify GW method, which throws an exception as a result,
            Then the exception gets caught and NULL is returned.")]
        public void PauseStepUCReturnsThrowsWhenPauseOrgMethodThrowsResourceNotFound()
        {
            // arrange
            var organisation = EntityHelpers.CreateOrganisationWithUsers(withIds: true, activeUsers: true);
            int existingId = organisation.Id;
            organisation.InRevalidationProcess = true; // we don't terminate immediatelly
            var domainOrganisation = organisation.ToDomain();

            _mockOrganisationGateway
                .Setup(gw => gw.GetOrganisationById(It.Is<int>(id => id == existingId)))
                .Returns(domainOrganisation);

            int nonExistingId = existingId; // organisation gets deleted or inaccessible.
            
            _mockOrganisationGateway.Setup(gw => gw.PauseOrganisation(It.Is<int>(id => id == nonExistingId)))
                .Throws(new ResourceNotFoundException("Error"));
            
            // act
            Func<Task> ucCall = async () => await _classUnderTest.GetOrganisationAndSendEmail(existingId);

            // assert
            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()),
                Times.Never);
            
            ucCall.Should().Throw<ResourceNotFoundException>().WithMessage("Error");
            // Throw exception back to ensure that retry is scheduled.
        }

        // There should also be logging tests, however this would be going too far in terms of the ticket work
    }
}