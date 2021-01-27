using LbhFssStepFunction.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LbhFssStepFunction.Tests.TestHelpers
{
    public static class DbCleardown
    {
        public static void ClearAll(DatabaseContext context)
        {
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE organizations CASCADE");
        }
    }
}