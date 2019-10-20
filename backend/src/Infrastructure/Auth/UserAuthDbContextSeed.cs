using Microsoft.EntityFrameworkCore;
using System;
using Core.Common.Auth;

namespace Infrastructure.Auth
{
    public static class UserAuthDbContextSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAuthenticationData>().HasData(new UserAuthenticationData()
            {
                UserId = Guid.Parse("93d7c87d-3186-4f9d-a7a2-8004667d4092"),
                UserName = "test", Password = "pass"
            });
            
        }
    }
}
