using System;
using System.Collections.Generic;

namespace LbhFssStepFunction.V1.Infrastructure
{
    public class OrganisationEntity
    {
        public OrganisationEntity()
        {
            UserOrganisations = new HashSet<UserOrganisationEntity>();
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
        public virtual ICollection<UserOrganisationEntity> UserOrganisations { get; set; }
    }
}