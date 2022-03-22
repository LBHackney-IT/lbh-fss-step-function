using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using LbhFssStepFunction.V1;
using LbhFssStepFunction.V1.Boundary.Requests;
using LbhFssStepFunction.V1.UseCase.Interface;
using Moq;
using AutoFixture;
using LbhFssStepFunction.Tests.TestHelpers;
using System;

namespace LbhFssStepFunction.Tests
{
    [TestFixture]
    public class HandlerTests
    {
        private static Fixture _fixture = new Fixture();
        private Handler _classUnderTest;
        private Mock<IStartFunctionUseCase> _mockStartFunctionUseCase;
        private Mock<IFirstStepUseCase> _mockFirstStepUseCase;
        private Mock<IReminderToReminderUseCase> _mockReminderToReminderUC;
        private Mock<ISecondStepUseCase> _mockSecondStepUseCase;
        private Mock<IThirdStepUseCase> _mockThirdStepUseCase;
        private Mock<IPauseStepUseCase> _mockPauseStepUseCase;

        [SetUp]
        public void Setup()
        {
            _mockStartFunctionUseCase = new Mock<IStartFunctionUseCase>();
            _mockFirstStepUseCase = new Mock<IFirstStepUseCase>();
            _mockReminderToReminderUC = new Mock<IReminderToReminderUseCase>();
            _mockSecondStepUseCase = new Mock<ISecondStepUseCase>();
            _mockThirdStepUseCase = new Mock<IThirdStepUseCase>();
            _mockPauseStepUseCase = new Mock<IPauseStepUseCase>();
            _classUnderTest = new Handler(_mockStartFunctionUseCase.Object,
                _mockFirstStepUseCase.Object,
                _mockReminderToReminderUC.Object,
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
        public async Task FirstStepHandlerCallsFirstStepUseCase()
        {
            var request = _fixture.Create<OrganisationRequest>();
            await _classUnderTest.FirstStep(request);
            _mockFirstStepUseCase.Verify(uc =>
                uc.GetOrganisationAndSendEmail(It.Is<int>(x => x == request.OrganisationId)), Times.Once);
        }

        [TestCase(TestName = @"
            Given a valid organisation id,
            When the first step function gets called,
            Then it returns an Organisation Response.")]
        public async Task FirstStepHandlerReturnsOrganisationResponse()
        {
            // arrange
            var request = _fixture.Create<OrganisationRequest>();
            var expectedResponse = _fixture.Create<OrganisationResponse>();
            
            _mockFirstStepUseCase.Setup(x => x.GetOrganisationAndSendEmail(It.IsAny<int>())).ReturnsAsync(expectedResponse);
            
            // act
            var response = await _classUnderTest.FirstStep(request);

            // assert
            response.Should().Be(expectedResponse);
        }

        #region Second Step

        [TestCase(TestName = @"
            Given a valid Organisation Id,
            When the Handler's Step 2 function gets called,
            Then it calls the Reminder To Reminder use case's GetOrganisationAndSendEmail method,
            And passes in the correct parameters,
            Then the handler function returns correct response.")]
        public async Task SecondStepHandlerCallsSecondStepUseCaseHappyPath()
        {
            // arrange
            int expectedStepNumber = 2;
            
            var request = Randomm.Create<OrganisationRequest>();
            var ucResponse = Randomm
                .Build<OrganisationResponse>()
                .With(o => o.OrganisationId, request.OrganisationId)
                .Create();

            _mockReminderToReminderUC
                .Setup(uc =>
                    uc.GetOrganisationAndSendEmail(
                        It.Is<int>(id => id == request.OrganisationId),
                        It.IsAny<int>()))
                .ReturnsAsync(ucResponse);

            // act
            var handlerResult = await _classUnderTest.SecondStep(request);

            // assert
            _mockReminderToReminderUC.Verify(
                uc => uc.GetOrganisationAndSendEmail(
                    It.Is<int>(x => x == request.OrganisationId),
                    It.Is<int>(step => step == expectedStepNumber)),
                Times.Once);
            
            handlerResult.Should().Equals(ucResponse); // should be the same object reference
        }

        [TestCase(TestName = @"
            Given an invalid Organisation Id,
            When the Handler's Step 2 function gets called,
            And the subsequent call to Reminder To Reminder use case returns NULL result,
            Then the handler function returns the same NULL result")]
        public async Task SecondStepHandlerReturnsNullWhenOrganisationDoesntExist()
        {
            // arrange
            var request = Randomm.Create<OrganisationRequest>();
            var ucResponse = null as OrganisationResponse;

            _mockReminderToReminderUC
                .Setup(uc =>
                    uc.GetOrganisationAndSendEmail(
                        It.Is<int>(id => id == request.OrganisationId),
                        It.IsAny<int>()))
                .ReturnsAsync(ucResponse);

            // act
            var handlerResult = await _classUnderTest.SecondStep(request);

            // assert
            handlerResult.Should().BeNull();
        }
        
        #endregion
        #region Third Step

        [TestCase(TestName = @"
            Given a valid Organisation Id,
            When the Handler's Step 3 function gets called,
            Then it calls the Reminder To Reminder use case's GetOrganisationAndSendEmail method,
            And passes in the correct parameters,
            Then the handler function returns correct response.")]
        public async Task ThirdStepHandlerCallsThirdStepUseCaseHappyPath()
        {
            // arrange
            int expectedStepNumber = 3;
            
            var request = Randomm.Create<OrganisationRequest>();
            var ucResponse = Randomm
                .Build<OrganisationResponse>()
                .With(o => o.OrganisationId, request.OrganisationId)
                .Create();

            _mockReminderToReminderUC
                .Setup(uc =>
                    uc.GetOrganisationAndSendEmail(
                        It.Is<int>(id => id == request.OrganisationId),
                        It.IsAny<int>()))
                .ReturnsAsync(ucResponse);

            // act
            var handlerResult = await _classUnderTest.ThirdStep(request);

            // assert
            _mockReminderToReminderUC.Verify(
                uc => uc.GetOrganisationAndSendEmail(
                    It.Is<int>(x => x == request.OrganisationId),
                    It.Is<int>(step => step == expectedStepNumber)),
                Times.Once);
            
            handlerResult.Should().Equals(ucResponse); // should be the same object reference
        }

        [TestCase(TestName = @"
            Given an invalid Organisation Id,
            When the Handler's Step 3 function gets called,
            And the subsequent call to Reminder To Reminder use case returns NULL result,
            Then the handler function returns the same NULL result")]
        public async Task ThirdStepHandlerReturnsNullWhenOrganisationDoesntExist()
        {
            // arrange
            var request = Randomm.Create<OrganisationRequest>();
            var ucResponse = null as OrganisationResponse;

            _mockReminderToReminderUC
                .Setup(uc =>
                    uc.GetOrganisationAndSendEmail(
                        It.Is<int>(id => id == request.OrganisationId),
                        It.IsAny<int>()))
                .ReturnsAsync(ucResponse);

            // act
            var handlerResult = await _classUnderTest.ThirdStep(request);

            // assert
            handlerResult.Should().BeNull();
        }

        #endregion

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