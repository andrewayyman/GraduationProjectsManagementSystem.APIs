using Domain.Entities;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Identity
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>

    {
        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamJoinRequest> TeamJoinRequests { get; set; }
        public DbSet<Domain.Entities.Task> Tasks { get; set; }
        public DbSet<ProjectIdea> ProjectIdeas { get; set; }

        public DbSet<ProjectIdeaRequest> ProjectIdeasRequest { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Meeting> Meetings { get; set; }

    }
}