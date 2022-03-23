using System.Threading.Tasks;
using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.UseCase;
using Moq;
using NUnit.Framework;
using System;
using LbhFssStepFunction.V1.Errors;
using FluentAssertions;
using LbhFssStepFunction.V1;
using System.Linq;

namespace LbhFssStepFunction.Tests.V1.UseCase
{
    [TestFixture]
    public class FirstStepUseCaseTests
    {
        private FirstStepUseCase _classUnderTest;
        private Mock<IOrganisationsGateway> _mockOrganisationGateway;
        private Mock<INotifyGateway> _mockNotifyGateway;

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("WAIT_DURATION", "600");
            _mockOrganisationGateway = new Mock<IOrganisationsGateway>();
            _mockNotifyGateway = new Mock<INotifyGateway>();
            _classUnderTest = new FirstStepUseCase(_mockOrganisationGateway.Object, _mockNotifyGateway.Object);
        }

        [TestCase(TestName = @"
            Given an existing organisation id (the flag method doesn't throw),
            When the first step use case gets called,
            It calls the organisation gateway GetOrganisationsToReview method with correct id.")]
        public async Task FirstStepUseCaseCallsOrganisationGateway()
        {
            // arrange
            var organisation = EntityHelpers.CreateOrganisation(withId: true).ToDomain();
            int existingId = organisation.Id;

            _mockOrganisationGateway
                .Setup(gw => gw.GetOrganisationById(It.Is<int>(id => id == existingId)))
                .Returns(organisation);
            
            // act
            await _classUnderTest.GetOrganisationAndSendEmail(existingId);
            
            // assert
            _mockOrganisationGateway.Verify(
                gw => gw.GetOrganisationById(It.Is<int>(id => id == existingId)),
                Times.Once);
        }

        [TestCase(TestName = @"
            Given an existing Organisation id,
            When the first step use case gets called,
            It calls the notify gateway SendNotificationEmail method,
            And returns correct organisations and next step state data.")]
        public async Task FirstStepUseCaseCallsNotificationGateway()
        {
            // arrange
            int waitDuration = Int32.Parse(Environment.GetEnvironmentVariable("WAIT_DURATION"));
            var organisation = EntityHelpers.CreateOrganisationWithUsers(withIds: true, activeUsers: true).ToDomain();
            int existingId = organisation.Id;

            _mockOrganisationGateway
                .Setup(og => og.GetOrganisationById(It.Is<int>(id => id == existingId)))
                .Returns(organisation);

            var emailArgs = organisation.UserOrganisations.Select(uo => uo.User.Email).ToList();

            // act
            var ucResult = await _classUnderTest.GetOrganisationAndSendEmail(existingId);
            
            // assert
            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.Is<string>(n => n == organisation.Name), 
                    It.Is<string[]>(es => 
                        es.All(e => emailArgs.Contains(e)) && 
                        es.Count() == emailArgs.Count()), 
                    It.Is<int>(state => state == 1)), 
                Times.Once);

            ucResult.Should().NotBeNull(); // If it's NULL crash right away
            ucResult.EmailAddresses.Should().BeEquivalentTo(emailArgs);
            // Next step time should be ±2 seconds from the time the UC was executed.
            ucResult.NextStepTime.Should().BeCloseTo(DateTime.Now.AddSeconds(waitDuration), precision: 2000);
            ucResult.StateResult.Should().BeTrue();
            ucResult.OrganisationId.Should().Be(existingId);
        }

        // There should be a test case for the correct UC return, but the current return doesn't make sense.
        // Will look into refactoring in the future.

        [TestCase(TestName = @"
            Given an existing organisation's Id (GW mock will not throw an error),
            When the first step usecase's GetOrganisationAndSendEmail method is called,
            Then the secondary call to FlagOrganisationToBeInRevalidation organition gateway's method is made,
            And the passed in parameters are correct.")]
        public async Task FirstStepUCCallsFlagOrgMethod()
        {
            // arrange
            int existingId = Randomm.Id(100, 200);
            _mockOrganisationGateway
                .Setup(gw => gw.GetOrganisationById(It.IsAny<int>()))
                .Returns(EntityHelpers.CreateOrganisation().ToDomain());

            // act
            await _classUnderTest.GetOrganisationAndSendEmail(existingId);

            // assert
            _mockOrganisationGateway.Verify(
                gw => gw.FlagOrganisationToBeInRevalidation(It.Is<int>(id => id == existingId)),
                Times.Once);
        }

        [TestCase(TestName = @"
            Given a NOT existing organisation's Id (organisations GW mock WILL throw an error),
            When the first step usecase's GetOrganisationAndSendEmail method is called,
            Then the use case captures FlagOrganisationToBeInRevalidation organition gateway's method failure,
            And the UC returns NULL result.")]
        public async Task FirstStepUCReturnsNullWhenFlagOrgMethodThrowsResourceNotFound()
        {
            // arrange
            int randomOrgId = Randomm.Id(100, 200);
            _mockOrganisationGateway.Setup(gw => gw.FlagOrganisationToBeInRevalidation(It.IsAny<int>()))
                .Throws(new ResourceNotFoundException("Error"));
            
            // act, assert
            Func<Task<OrganisationResponse>> ucCall = async () =>
                await _classUnderTest.GetOrganisationAndSendEmail(randomOrgId);
            
            ucCall.Should().NotThrow();
            
            var ucResult = await ucCall();
            ucResult.Should().BeNull();

            _mockOrganisationGateway.Verify(
                gw => gw.GetOrganisationById(It.IsAny<int>()),
                Times.Never);

            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<int>()),
                Times.Never);
        }
    }
}