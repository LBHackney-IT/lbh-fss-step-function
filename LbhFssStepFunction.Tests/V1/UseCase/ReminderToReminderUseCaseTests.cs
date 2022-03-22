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
    public class ReminderToReminderUseCaseTests
    {
        private ReminderToReminderUseCase _classUnderTest;
        private Mock<IOrganisationsGateway> _mockOrganisationGateway;
        private Mock<INotifyGateway> _mockNotifyGateway;

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("WAIT_DURATION", "600");
            _mockOrganisationGateway = new Mock<IOrganisationsGateway>();
            _mockNotifyGateway = new Mock<INotifyGateway>();
            _classUnderTest = new ReminderToReminderUseCase(
                _mockOrganisationGateway.Object,
                _mockNotifyGateway.Object);
        }

        [TestCase(TestName = @"
            Given any Organisation Id,
            And any Step number,
            When the Reminder To Reminder use case gets called,
            Then it always calls the organisation gateway's GetOrganisationById method with correct parameters.")]
        public async Task ReminderToReminderUseCaseCallsOrganisationGatewayWithCorrectParams()
        {
            // arrange
            int randomId = Randomm.Id(); //It doesn't matter if it's existing or non-existing id.
            int anyStep = Randomm.Int(); //It doesn't what step number it is for this test

            // act
            await _classUnderTest.GetOrganisationAndSendEmail(randomId, anyStep);
            
            // assert
            _mockOrganisationGateway.Verify(
                gw => gw.GetOrganisationById(It.Is<int>(id => id == randomId)), 
                Times.Once);
        }

        [TestCase(TestName = @"
            Given any Step number,
            And an existing Organisation Id,
            And that organisation being in Revalidation process, 
            When the Reminder To Reminder use case gets called,
            Then it calls the notify gateway SendNotificationEmail method with correct parameters,
            And it returns the correct organisation & Next Step state data.")]
        public async Task ReminderToReminderUseCaseCallsNotificationGatewayWithCorrectParamsIfOrganisationWasNotUpdated()
        {
            // arrange
            string waitDuration = Environment.GetEnvironmentVariable("WAIT_DURATION");
            var expectedScheduledDate = SharedUtils.WaitTimeToDate(waitDuration);

            int step = Randomm.Int(); // Step number has to be passed onto the notify GW

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
            var ucResult = await _classUnderTest.GetOrganisationAndSendEmail(existingId, step);
            
            // assert
            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.Is<string>(n => n == domainOrganisation.Name), 
                    It.Is<string[]>(es => es.All(e => emails.Contains(e)) && es.Count() == emails.Count()), 
                    It.Is<int>(state => state == step)), 
                Times.Once);

            ucResult.Should().NotBeNull();
            ucResult.EmailAddresses.Should().BeEquivalentTo(emails);
            // Next step time should be ±2 seconds from the time the UC was executed. Should be accurate enough for testing purposes.
            ucResult.NextStepTime.Should().BeCloseTo(expectedScheduledDate, precision: 2000);
            ucResult.StateResult.Should().BeTrue();
            ucResult.OrganisationId.Should().Be(existingId); // the rest of the organisations data is irrelevant.
        }

        [TestCase(TestName = @"
            Given any Step number, 
            And an existing Organisation Id,
            And that organisation NOT being in Revalidation process,
            When the Reminder To Reminder use case gets called,
            Then it does NOT call the notify gateway's SendNotfication method,
            And it returns a NULL result.")]
        public async Task ReminderToReminderUCDoesNotCallNotifyGWAndReturnsNullWhenOrganisationWasUpdated()
        {
            // arrange
            int existingId = Randomm.Id();
            int anyStep = Randomm.Int(); //It doesn't what step number it is for this test

            var organisation = EntityHelpers.CreateOrganisationWithUsers(activeUsers: true);
            organisation.Id = existingId;
            
            // This being false implies that organisation was updated since the previous step.
            // It is because Step 1 switches this value to to "true", and no other step changes
            // this value again so it can only be changed to "false" external influence.
            organisation.InRevalidationProcess = false;

            var domainOrganisation = organisation.ToDomain();

            _mockOrganisationGateway
                .Setup(gw => gw.GetOrganisationById(It.Is<int>(id => id == existingId)))
                .Returns(domainOrganisation);

            // act
            var ucResult = await _classUnderTest.GetOrganisationAndSendEmail(existingId, anyStep);

            // assert
            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()),
                Times.Never);

            ucResult.Should().BeNull(); // Should be a more complicted assertion, but such "return" is outside MVP scope.
        }

        [TestCase(TestName = @"
            Given any Step number, 
            And a NOT existing Organisation Id,
            When the Reminder To Reminder use case gets called,
            Then it does NOT call notify gateway's SendNotfication method,
            And it returns a NULL result.")]
        public async Task ReminderToReminderUCDoesNotCallNotifyGWAndReturnsNullWhenOrganisationIsNotFound()
        {
            // arrange
            int nonExistingId = Randomm.Id();
            int anyStep = Randomm.Int(); //It doesn't what step number it is for this test

            // organisation was removed between since the triggering of either previous step, or
            // (unlikely case) this step.
            OrganisationDomain organisation = null;

            _mockOrganisationGateway
                .Setup(gw => gw.GetOrganisationById(It.Is<int>(id => id == nonExistingId)))
                .Returns(organisation);

            // act
            var ucResult = await _classUnderTest.GetOrganisationAndSendEmail(nonExistingId, anyStep);

            // assert
            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()),
                Times.Never);
            
            ucResult.Should().BeNull(); // A more proper return requires doing a small tech spike.
        }
    }
}
