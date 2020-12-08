using System;
using System.Collections.Generic;

namespace LbhFssStepFunction.V1.Domains
{
    public class UserDomain
    {
        public int Id { get; set; }
        public string SubId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Status { get; set; }
    }
}