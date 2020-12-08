using System;
using System.Collections.Generic;

namespace LbhFssStepFunction.V1.Infrastructure
{
    public class UserEntity
    {
        public UserEntity()
        {
            UserOrganisations = new HashSet<UserOrganisationEntity>();
        }

        public int Id { get; set; }
        public string SubId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Status { get; set; }
        public virtual ICollection<UserOrganisationEntity> UserOrganisations { get; set; }
    }
}