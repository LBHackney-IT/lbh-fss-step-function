using System;
using FluentAssertions;

namespace LbhFssStepFunction.Tests.TestHelpers
{
    public static class CustomiseAssertions
    {
        public static void ApproximationDateTime()
        {
            AssertionOptions.AssertEquivalencyUsing(options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation)).WhenTypeIs<DateTime>();
                options.Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation)).WhenTypeIs<DateTimeOffset>();
                return options;
            });
        }
    }
}