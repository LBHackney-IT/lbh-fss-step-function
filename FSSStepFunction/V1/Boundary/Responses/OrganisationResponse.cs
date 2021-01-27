using System;
using System.Collections.Generic;

namespace LbhFssStepFunction.V1
{
    public class OrganisationResponse
    {
        public int OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public List<string> EmailAddresses { get; set; }
        public bool StateResult { get; set; }
        public DateTime NextStepTime { get; set; }
    }
}