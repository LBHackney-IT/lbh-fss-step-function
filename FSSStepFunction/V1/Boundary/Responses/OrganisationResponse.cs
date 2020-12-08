using System.Collections.Generic;

namespace LbhFssStepFunction.V1
{
    public class OrganisationResponse
    {
        public int Id { get; set; }
        public string OrganisationName { get; set; }
        public string Status { get; set; }
        public List<string> EmailAddresses { get; set; }
    }
}