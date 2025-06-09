using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Index(nameof(TeamId), nameof(ScheduledAt), IsUnique = true)]

    public class Meeting
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime ScheduledAt { get; set; }
        public string? MeetingLink { get; set; }

        public string? Objectives { get; set; }

        public string? Comment { get; set; }

        // ربط الاجتماع بالدكتور
        public int SupervisorId { get; set; }
        public Supervisor Supervisor { get; set; }  // Assuming you're using Identity

        // ربط الاجتماع بفريق واحد فقط
        public int TeamId { get; set; }
        public Team Team { get; set; }
    }
}

