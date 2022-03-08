using System;
using System.Linq;
using System.Threading.Tasks;
using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Factories;
using LbhFssStepFunction.V1.Gateways.Interface;
using LbhFssStepFunction.V1.UseCase;
using Moq;
using NUnit.Framework;

namespace LbhFssStepFunction.Tests.V1.UseCase
{
    [TestFixture]
    public class SecondStepUseCaseTests
    {
        private SecondStepUseCase _classUnderTest;
        private Mock<IOrganisationsGateway> _mockOrganisationGateway;
        private Mock<INotifyGateway> _mockNotifyGateway;

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("WAIT_DURATION", "600");
            _mockOrganisationGateway = new Mock<IOrganisationsGateway>();
            _mockNotifyGateway = new Mock<INotifyGateway>();
            _classUnderTest = new SecondStepUseCase(_mockOrganisationGateway.Object, _mockNotifyGateway.Object);
        }

        [TestCase(TestName = @"
            Given any Organisation Id,
            When the second step use case gets called,
            Then it always calls the organisation gateway's GetOrganisationById method with correct parameters.")]
        public async Task SecondStepUseCaseCallsOrganisationGatewayWithCorrectParams()
        {
            // arrange
            int randomId = Randomm.Id(); //It doesn't matter if it's existing or non-existing id.

            // act
            await _classUnderTest.GetOrganisationAndSendEmail(randomId);
            
            // assert
            _mockOrganisationGateway.Verify(gw => gw.GetOrganisationById(It.Is<int>(id => id == randomId)), Times.Once);
        }

        [TestCase(TestName = @"
            Given an existing Organisation Id,
            And that organisation being in Revalidation process, 
            When the second step use case gets called,
            Then it calls the notify gateway SendNotificationEmail method with correct parameters.")]
        public async Task SecondStepUseCaseCallsNotificationGatewayWithCorrectParams()
        {
            // arrange
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
            await _classUnderTest.GetOrganisationAndSendEmail(existingId);
            
            // assert
            _mockNotifyGateway.Verify(
                gw => gw.SendNotificationEmail(
                    It.Is<string>(n => n == domainOrganisation.Name), 
                    It.Is<string[]>(es => es.All(e => emails.Contains(e)) && es.Count() == emails.Count()), 
                    It.Is<int>(state => state == 2)), 
                Times.Once);
        }
    }
}