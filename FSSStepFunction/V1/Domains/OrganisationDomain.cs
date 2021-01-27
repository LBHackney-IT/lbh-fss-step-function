using System;
using System.Collections.Generic;

namespace LbhFssStepFunction.V1.Domains
{
    public class OrganisationDomain
    {
        public OrganisationDomain()
        {
            UserOrganisations = new HashSet<UserOrganisationDomain>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string Status { get; set; }
        public DateTime? LastRevalidation { get; set; }
        public bool InRevalidationProcess { get; set; }
        public virtual ICollection<UserOrganisationDomain> UserOrganisations { get; set; }
    }
}