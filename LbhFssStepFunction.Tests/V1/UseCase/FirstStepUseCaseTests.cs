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
            Given that the first step use case gets called,
            It calls the organisation gateway GetOrganisationsToReview method.")]
        public async Task FirstStepUseCaseCallsOrganisationGateway()
        {
            // act
            await _classUnderTest.GetOrganisationAndSendEmail(1);
            
            // assert
            _mockOrganisationGateway.Verify(gw => gw.GetOrganisationById(It.IsAny<int>()), Times.Once);
        }

        [TestCase(TestName = @"
            Given that the first step use case gets called,
            It calls the notify gateway SendNotificationEmail method.")]
        public async Task FirstStepUseCaseCallsNotificationGateway()
        {
            // arrange
            var organisation = EntityHelpers.CreateOrganisation();
            _mockOrganisationGateway.Setup(og => og.GetOrganisationById(It.IsAny<int>())).Returns(organisation.ToDomain());

            // act
            await _classUnderTest.GetOrganisationAndSendEmail(1);
            
            // assert
            _mockNotifyGateway.Verify(gw => gw.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<int>()), Times.Once);
        }

        // There should be a test case for UC return, but the current return doesn't make sense.
        // Will look into refactoring in the future.

        [TestCase(TestName = @"
            Given an existing organisation's Id (GW mock will not throw an error),
            When the first step usecase's GetOrganisationAndSendEmail method is called,
            Then the secondary call to FlagOrganisationToBeInRevalidation organition gateway's method is made")]
        public async Task FirstStepUCCallsFlagOrgMethod()
        {
            // arrange
            int randomOrgId = Randomm.Id(100, 200);

            // act
            // _mockOrganisationGateway.Setup(gw => gw.FlagOrganisationToBeInRevalidation(It.IsAny<int>()));
            await _classUnderTest.GetOrganisationAndSendEmail(randomOrgId);

            // assert
            _mockOrganisationGateway.Verify(gw => gw.FlagOrganisationToBeInRevalidation(It.IsAny<int>()), Times.Once);
        }

        [TestCase(TestName = @"
            Given an existing organisation's Id (GW mock will not throw an error),
            When the first step usecase's GetOrganisationAndSendEmail method is called,
            Then the secondary call to FlagOrganisationToBeInRevalidation organition gateway's method is made")]
        public async Task FirstStepUCReturnsNullWhenFlagOrgMethodThrowsResourceNotFound()
        {
            // arrange
            int randomOrgId = Randomm.Id(100, 200);
            _mockOrganisationGateway.Setup(gw => gw.FlagOrganisationToBeInRevalidation(It.IsAny<int>()))
                .Throws(new ResourceNotFoundException("Error"));

            // act
            var ucResult = await _classUnderTest.GetOrganisationAndSendEmail(randomOrgId);

            // assert
            _mockOrganisationGateway.Verify(gw => gw.FlagOrganisationToBeInRevalidation(It.IsAny<int>()), Times.Once);
            ucResult.Should().Be(null);
        }
    }
}