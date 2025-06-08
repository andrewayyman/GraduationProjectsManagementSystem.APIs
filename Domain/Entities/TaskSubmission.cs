using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TaskSubmission
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public string? FilePath { get; set; }
        public string? Comments { get; set; }
        public string? RepoLink { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
