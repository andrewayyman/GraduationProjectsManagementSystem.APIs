using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Identity.Configuration
{
    public class SupervisorConfiguration : IEntityTypeConfiguration<Supervisor>
    {

        public void Configure(EntityTypeBuilder<Supervisor> builder)
        {
            builder
           .HasOne(s => s.User)
           .WithOne(u => u.Supervisor)
           .HasForeignKey<Supervisor>(s => s.UserId);
        }
    }
}

