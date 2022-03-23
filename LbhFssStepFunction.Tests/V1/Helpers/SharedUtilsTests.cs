using System;
using FluentAssertions;
using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Helpers;
using NUnit.Framework;

namespace LbhFssStepFunction.Tests.V1.Helpers
{
    [TestFixture]
    public class SharedUtilsTests
    {
        [TestCase(TestName = @"
            Given a valid numerical string wait duration in days,
            When the SharedUtils's WaitTimeToDate method gets called,
            Then it returns a DateTime object,
            And the time is set to be wait time duration away from when the method was called.")]
        public void ValidWaitDaysDurationGetsParsedIntoDateTimeXDaysAwayFromCurrentTime()
        {
            // arrange
            int waitDurationDays = Randomm.Int(minimum: 1, maximum: 31);
            string waitDurationInput = waitDurationDays.ToString();

            DateTime expectedResult = DateTime.Now.AddDays(waitDurationDays);

            // act
            // making no assumptions about the return type of date representation.
            var dateResult = SharedUtils.WaitTimeToDate(waitDurationInput);

            // assert
            dateResult.GetType().Should().Be<DateTime>();
            dateResult.Should().BeCloseTo(expectedResult, 2000);
        }

        [TestCase(TestName = @"
            Given an invalid string wait duration,
            When the SharedUtils's WaitTimeToDate method gets called,
            Then it throws a FormatException with custom error message.")]
        public void InvalidWaitDurationInputThrowsExceptionWithCustomMessage()
        {
            // arrange
            string waitDurationDays = Randomm.Int(maximum: 30).ToString();
            string badInput = $"{waitDurationDays}d."; // non-numerical string

            // act
            Action badInputCall = () => SharedUtils.WaitTimeToDate(badInput);

            // assert
            badInputCall.Should().Throw<FormatException>(
                because: "waitDuration needs to be a numerical string to be parsed correctly.");
            
            try { badInputCall(); }
            catch (FormatException ex)
            {
                ex.Message.Should().Contain(waitDurationDays);
            }
        }
        
        [TestCase("", TestName = @"
            Given an empty string,
            When the SharedUtils's WaitTimeToDate method gets called,
            Then it throws ArgumentException with custom error message.")]
        [TestCase(null, TestName = @"
            Given a for some reason null string,
            When the SharedUtils's WaitTimeToDate method gets called,
            Then it throws ArgumentException with custom error message.")]
        public void EmptyWaitDurationThrowsArgumentException(string caseValue)
        {
            // arrange
            string waitDurationInput = caseValue;

            // act
            Action emptyInputCall = () => SharedUtils.WaitTimeToDate(waitDurationInput);

            // assert
            emptyInputCall.Should().Throw<ArgumentException>(
                because: "It informs about unconfigured environment variable");
            
            try { emptyInputCall(); }
            catch (ArgumentException ex)
            {
                ex.Message.Should().Contain("The wait time string was not provided");
                ex.ParamName.Should().Be("waitTime");
            }
        }
    }
}
