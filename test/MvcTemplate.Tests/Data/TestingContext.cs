﻿using MvcTemplate.Data.Core;
using MvcTemplate.Objects;

namespace MvcTemplate.Tests.Data
{
    public class TestingContext : Context
    {
        public void DropData()
        {
            Set<RolePermission>().RemoveRange(Set<RolePermission>());
            Set<Permission>().RemoveRange(Set<Permission>());
            Set<Account>().RemoveRange(Set<Account>());
            Set<Role>().RemoveRange(Set<Role>());

            SaveChanges();
        }
    }
}
