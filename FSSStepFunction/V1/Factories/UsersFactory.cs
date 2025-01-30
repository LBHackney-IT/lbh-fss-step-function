using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LbhFssStepFunction.V1.Domains;
using LbhFssStepFunction.V1.Infrastructure;

namespace LbhFssStepFunction.V1.Factories
{
    public static class UsersFactory
    {
        public static UserDomain ToDomain(this UserEntity userEntity)
        {
            return new UserDomain
            {
                Id = userEntity.Id,
                Name = userEntity.Name,
                Email = userEntity.Email,
                Status = userEntity.Status
            };
        }
    }
}