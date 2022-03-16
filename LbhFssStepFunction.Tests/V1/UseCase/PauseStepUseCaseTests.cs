using LbhFssStepFunction.Tests.TestHelpers;
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

        [TestCase(TestName = "Given that the pause step use case gets called, it calls the organisation gateway GetOrganisationsToReview method.")]
        public void PauseStepUseCaseCallsOrganisationGateway()
        {
            _classUnderTest.GetOrganisationAndSendEmail(1);
            _mockOrganisationGateway.Verify(gw => gw.GetOrganisationById(It.IsAny<int>()), Times.Once);
        }

        [TestCase(TestName = "Given that the pause step use case gets called, it calls the organisation gateway PauseOrganisation method.")]
        public void PauseStepUseCaseCallsOrgnisationGatewayPauseMethod()
        {
            var organisation = EntityHelpers.CreateOrganisation();
            _mockOrganisationGateway.Setup(og => og.GetOrganisationById(It.IsAny<int>())).Returns(organisation.ToDomain());
            _classUnderTest.GetOrganisationAndSendEmail(1);
            _mockOrganisationGateway.Verify(gw => gw.PauseOrganisation(It.IsAny<int>()), Times.Once);
        }

        [TestCase(TestName = "Given that the pause step use case gets called, it calls the notify gateway SendNotificationEmail method.")]
        public void PauseStepUseCaseCallsNotificationGateway()
        {
            var organisation = EntityHelpers.CreateOrganisation();
            _mockOrganisationGateway.Setup(og => og.GetOrganisationById(It.IsAny<int>())).Returns(organisation.ToDomain());
            _classUnderTest.GetOrganisationAndSendEmail(1);
            _mockNotifyGateway.Verify(gw => gw.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<int>()), Times.Once);
        }

    }
}