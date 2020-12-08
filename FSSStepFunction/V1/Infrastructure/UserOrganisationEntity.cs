using System;

namespace LbhFssStepFunction.V1.Infrastructure
{
    public class UserOrganisationEntity
    {
        public int Id { get; set; }
        public int OrganisationId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int UserId { get; set; }

        public virtual OrganisationEntity Organisation { get; set; }
        public virtual UserEntity User { get; set; }
    }
}