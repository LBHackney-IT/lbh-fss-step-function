using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using LbhFssStepFunction.V1;
using LbhFssStepFunction.V1.Boundary.Requests;
using LbhFssStepFunction.V1.UseCase.Interface;
using Moq;
using AutoFixture;

namespace LbhFssStepFunction.Tests
{
    [TestFixture]
    public class HandlerTests
    {
        private static Fixture _fixture = new Fixture();
        private Handler _classUnderTest;
        private Mock<IStartFunctionUseCase> _mockStartFunctionUseCase;
        private Mock<IFirstStepUseCase> _mockFirstStepUseCase;
        private Mock<ISecondStepUseCase> _mockSecondStepUseCase;
        private Mock<IThirdStepUseCase> _mockThirdStepUseCase;
        private Mock<IPauseStepUseCase> _mockPauseStepUseCase;

        [SetUp]
        public void Setup()
        {
            _mockStartFunctionUseCase = new Mock<IStartFunctionUseCase>();
            _mockFirstStepUseCase = new Mock<IFirstStepUseCase>();
            _mockSecondStepUseCase = new Mock<ISecondStepUseCase>();
            _mockThirdStepUseCase = new Mock<IThirdStepUseCase>();
            _mockPauseStepUseCase = new Mock<IPauseStepUseCase>();
            _classUnderTest = new Handler(_mockStartFunctionUseCase.Object,
                _mockFirstStepUseCase.Object,
                _mockSecondStepUseCase.Object,
                _mockThirdStepUseCase.Object,
                _mockPauseStepUseCase.Object);
        }

        [TestCase(TestName = "Given that the start function gets called, it calls the start function use case Execute method.")]
        public void StartFunctionHandlerCallsStartFunctionUseCase()
        {
            _classUnderTest.StartFunction();
            _mockStartFunctionUseCase.Verify(uc => uc.Execute(), Times.Once);
        }

        [TestCase(TestName = "Given that the first step function gets called, it calls the first step use case GetOrganisationAndSendEmail method.")]
        public void FirstStepHandlerCallsFirstStepUseCase()
        {
            var request = _fixture.Create<OrganisationRequest>();
            _classUnderTest.FirstStep(request);
            _mockFirstStepUseCase.Verify(uc =>
                uc.GetOrganisationAndSendEmail(It.Is<int>(x => x == request.OrganisationId)), Times.Once);
        }

        [TestCase(TestName = "Given that the first step function gets called with a valid organisation id, it returns an Organisation Response.")]
        public async Task FirstStepHandlerReturnsOrganisationResponse()
        {
            var request = _fixture.Create<OrganisationRequest>();
            var expectedResponse = _fixture.Create<OrganisationResponse>();
            _mockFirstStepUseCase.Setup(x => x.GetOrganisationAndSendEmail(It.IsAny<int>())).ReturnsAsync(expectedResponse);
            var response = await _classUnderTest.FirstStep(request);
            response.Should().Be(expectedResponse);
        }

        [TestCase(TestName = "Given that the second step function gets called, it calls the second step use case GetOrganisationAndSendEmail method.")]
        public void SecondStepHandlerCallsSecondStepUseCase()
        {
            var request = _fixture.Create<OrganisationRequest>();
            _classUnderTest.SecondStep(request);
            _mockSecondStepUseCase.Verify(uc =>
                uc.GetOrganisationAndSendEmail(It.Is<int>(x => x == request.OrganisationId)), Times.Once);
        }

        [TestCase(TestName = "Given that the second step function gets called with a valid organisation id, it returns an Organisation Response.")]
        public async Task SecondStepHandlerReturnsOrganisationResponse()
        {
            var request = _fixture.Create<OrganisationRequest>();
            var expectedResponse = _fixture.Create<OrganisationResponse>();
            _mockSecondStepUseCase.Setup(x => x.GetOrganisationAndSendEmail(It.IsAny<int>())).ReturnsAsync(expectedResponse);
            var response = await _classUnderTest.SecondStep(request);
            response.Should().Be(expectedResponse);
        }

        [TestCase(TestName = "Given that the third step function gets called, it calls the third step use case GetOrganisationAndSendEmail method.")]
        public void ThirdStepHandlerCallsThirdStepUseCase()
        {
            var request = _fixture.Create<OrganisationRequest>();
            _classUnderTest.ThirdStep(request);
            _mockThirdStepUseCase.Verify(uc =>
                uc.GetOrganisationAndSendEmail(It.Is<int>(x => x == request.OrganisationId)), Times.Once);
        }

        [TestCase(TestName = "Given that the third step function gets called with a valid organisation id, it returns an Organisation Response.")]
        public async Task ThirdStepHandlerReturnsOrganisationResponse()
        {
            var request = _fixture.Create<OrganisationRequest>();
            var expectedResponse = _fixture.Create<OrganisationResponse>();
            _mockThirdStepUseCase.Setup(x => x.GetOrganisationAndSendEmail(It.IsAny<int>())).ReturnsAsync(expectedResponse);
            var response = await _classUnderTest.ThirdStep(request);
            response.Should().Be(expectedResponse);
        }

        [TestCase(TestName = "Given that the pause step function gets called, it calls the pause step use case GetOrganisationAndSendEmail method.")]
        public void PauseStepHandlerCallsPauseStepUseCase()
        {
            var request = _fixture.Create<OrganisationRequest>();
            _classUnderTest.PauseStep(request);
            _mockPauseStepUseCase.Verify(uc =>
                uc.GetOrganisationAndSendEmail(It.Is<int>(x => x == request.OrganisationId)), Times.Once);
        }

        [TestCase(TestName = "Given that the pause step function gets called with a valid organisation id, it returns an Organisation Response.")]
        public async Task PauseStepHandlerReturnsOrganisationResponse()
        {
            var request = _fixture.Create<OrganisationRequest>();
            var expectedResponse = _fixture.Create<OrganisationResponse>();
            _mockPauseStepUseCase.Setup(x => x.GetOrganisationAndSendEmail(It.IsAny<int>())).ReturnsAsync(expectedResponse);
            var response = await _classUnderTest.PauseStep(request);
            response.Should().Be(expectedResponse);
        }
    }
}