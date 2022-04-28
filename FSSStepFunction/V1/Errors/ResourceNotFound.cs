using System;

namespace LbhFssStepFunction.V1.Errors
{
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(string message) : base(message) {}
    }
}
