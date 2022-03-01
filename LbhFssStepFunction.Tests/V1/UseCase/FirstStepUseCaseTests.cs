using System.Threading.Tasks;
using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.UseCase;
using Moq;
using NUnit.Framework;
using System;

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

    }
}