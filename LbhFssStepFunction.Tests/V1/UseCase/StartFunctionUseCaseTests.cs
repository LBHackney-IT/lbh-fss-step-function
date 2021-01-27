using AutoFixture;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.UseCase;
using Moq;
using NUnit.Framework;

namespace LbhFssStepFunction.Tests.V1.UseCase
{
    [TestFixture]
    public class StatFunctionUseCaseTests
    {
        private static Fixture _fixture = new Fixture();   
        private StartFunctionUseCase _classUnderTest;
        private Mock<IOrganisationsGateway> _mockOrganisationGateway;
        
        [SetUp]
        public void Setup()
        {
            _mockOrganisationGateway = new Mock<IOrganisationsGateway>();
            _classUnderTest = new StartFunctionUseCase(_mockOrganisationGateway.Object);
        }
        
        [TestCase(TestName = "Given that the start function use case gets called, it calls the organisation gateway GetOrganisationsToReview method.")]
        public void StartFunctionHandlerCallsStartFunctionUseCase()
        {
            _classUnderTest.Execute();
            _mockOrganisationGateway.Verify(uc => uc.GetOrganisationsToReview(), Times.Once);
        }
        
        
    }
}