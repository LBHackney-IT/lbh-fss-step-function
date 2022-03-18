using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Domains;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.Helpers;
using LbhFssStepFunction.V1.UseCase;
using Moq;
using NUnit.Framework;

namespace LbhFssStepFunction.Tests.V1.UseCase
{
    [TestFixture]
    public class ThirdStepUseCaseTests
    {
        private ThirdStepUseCase _classUnderTest;
        private Mock<IOrganisationsGateway> _mockOrganisationGateway;
        private Mock<INotifyGateway> _mockNotifyGateway;

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("WAIT_DURATION", "600");
            _mockOrganisationGateway = new Mock<IOrganisationsGateway>();
            _mockNotifyGateway = new Mock<INotifyGateway>();
            _classUnderTest = new ThirdStepUseCase(_mockOrganisationGateway.Object, _mockNotifyGateway.Object);
        }

        [TestCase(TestName = @"
            Given any Organisation Id,
            When the third step use case gets called,
            Then it always calls the organisation gateway's GetOrganisationById method with correct parameters.")]
        public async Task ThirdStepUseCaseCallsOrganisationGatewayWithCorrectParams()
        {
            // arrange
            int randomId = Randomm.Id(); //It doesn't matter if it's existing or non-existing id.

            // act
            await _classUnderTest.GetOrganisationAndSendEmail(randomId);
            
            // assert
            _mockOrganisationGateway.Verify(
                gw => gw.GetOrganisationById(It.Is<int>(id => id == randomId)), 
                Times.Once);
        }

        [TestCase(TestName = @"
            Given an existing Organisation Id,
            And that organisation being in Revalidation process, 
            When the third step use case gets called,
            Then it calls the notify gateway SendNotificationEmail method with correct parameters,
            And it returns the correct organisation & Next Step state data.")]
        public async Task ThirdStepUseCaseCallsNotificationGatewayWithCorrectParamsIfOrganisationWasNotUpdated()
        {
            // arrange
            string waitDuration = Environment.GetEnvironmentVariable("WAIT_DURATION");
            var expectedScheduledDate = SharedUtils.WaitTimeToDate(waitDuration);

            int existingId = Randomm.Id();
            var organisation = EntityHelpers.CreateOrganisationWithUsers(activeUsers: true);

            organisation.Id = existingId;
            organisation.InRevalidationProcess = true;

            var emails = organisation.UserOrganisations.Select(uo => uo.User.Email).ToList();

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
                    It.Is<string[]>(es => es.All(e => emails.Contains(e)) && es.Count() == emails.Count()), 
                    It.Is<int>(state => state == 3)), 
                Times.Once);

            ucResult.Should().NotBeNull();
            ucResult.EmailAddresses.Should().BeEquivalentTo(emails);
            // Next step time should be ±2 seconds from the time the UC was executed. Should be accurate enough for testing purposes.
            ucResult.NextStepTime.Should().BeCloseTo(expectedScheduledDate, precision: 2000);
            ucResult.StateResult.Should().BeTrue();
            ucResult.OrganisationId.Should().Be(existingId); // the rest of the organisations data is irrelevant.
        }

        [TestCase(TestName = @"
            Given an existing Organisation Id,
            And that organisation NOT being in Revalidation process,
            When the Step 3 Usecase gets called,
            Then it does NOT call the notify gateway's SendNotfication method,
            And it returns a NULL result.")]
        public async Task ThirdStepUCDoesNotCallNotifyGWAndReturnsNullWhenOrganisationWasUpdated()
        {
            // arrange
            int existingId = Randomm.Id();

            var organisation = EntityHelpers.CreateOrganisationWithUsers(activeUsers: true);
            organisation.Id = existingId;
            
            // This being false implies that organisation was updated between steps 2 and 3.
            // It is because Step 1 switches this value to to "true", and it can only be changed
            // back via front-end. Since Step 2 ran successfully, it means that the change happened
            // after it.
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
        }

        [TestCase(TestName = @"
            Given a NOT existing Organisation Id,
            When the Step 3 Usecase gets called,
            Then it does NOT call notify gateway's SendNotfication method,
            And it returns a NULL result.")]
        public async Task ThirdStepUCDoesNotCallNotifyGWAndReturnsNullWhenOrganisationIsNotFound()
        {
            // arrange
            int nonExistingId = Randomm.Id();
            // organisation was removed between the triggering of Step 2 and Step 3
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
        }
    }
}
