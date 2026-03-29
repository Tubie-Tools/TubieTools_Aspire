using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer
{

    /// <summary>
    /// TypeLoadException: Could not load type 'Microsoft.EntityFrameworkCore.Metadata.Internal.AdHocMapper' from assembly 'Microsoft.EntityFrameworkCore, Version=9.0.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'.
    /// Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkRelationalServicesBuilder.TryAddCoreServices()
    /// 
    /// Unable to create a 'DbContext' of type 'ApplicationDbContext'. The exception 'Unable to resolve service for type 'Microsoft.EntityFrameworkCore.DbContextOptions`1[DataAccessLayer.ApplicationDbContext]' while attempting to activate 'DataAccessLayer.ApplicationDbContext'.' was thrown while attempting to create an instance. For the different patterns supported at design time, see https://go.microsoft.com/fwlink/?linkid=851728
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(new IdentityRole { Name = "Guardian", NormalizedName = "GUARDIAN", Id = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString() });
        }
    }
}
