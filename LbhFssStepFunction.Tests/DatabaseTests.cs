using LbhFssStepFunction.Tests.TestHelpers;
using LbhFssStepFunction.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;

namespace LbhFssStepFunction.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        protected DatabaseContext DatabaseContext { get; private set; }

        [SetUp]
        public void RunBeforeAnyTests()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseNpgsql(ConnectionString.TestDatabase());
            DatabaseContext = new DatabaseContext(builder.Options);
            DatabaseContext.Database.EnsureCreated();
            DbCleardown.ClearAll(DatabaseContext);
            CustomiseAssertions.ApproximationDateTime();
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            DbCleardown.ClearAll(DatabaseContext);
        }
    }
}
