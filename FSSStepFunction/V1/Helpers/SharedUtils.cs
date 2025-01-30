using System;

namespace LbhFssStepFunction.V1.Helpers
{
    public static class SharedUtils
    {
        public static DateTime WaitTimeToDate(string waitTime)
        {
            if (waitTime == "" || waitTime == null)
                throw new ArgumentException(
                    message: $"The wait time string was not provided, or provided empty: '{waitTime ?? "null"}'.",
                    paramName: nameof(waitTime));

            double waitDurationDays;
            bool success = Double.TryParse(waitTime, out waitDurationDays);
            
            if(!success) throw new FormatException(
                $"The wait time string: '{waitTime}' is not numeric, hence could not be parsed.");
            
            var dateAfterWait = DateTime.Now.AddDays(waitDurationDays);

            return dateAfterWait;
        }
    }
}
