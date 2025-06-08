using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }                                           // title of the task
        public string? Description { get; set; }                                    // description of the task
        public DateTime Deadline { get; set; }                                      // deadline of the task
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;                  // date of the task
        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Backlog;        // status of the task (Backlog, in progress, completed)

        // RelaitonShips

        public int SupervisorId { get; set; }
        public Supervisor Supervisor { get; set; }                                   // Supervisor who created the task

        public int TeamId { get; set; }                                              // Team related to the task
        public Team Team { get; set; }

        public int? AssignedStudentId { get; set; }                                  // Student assigned to the task
        public Student? AssignedStudent { get; set; }

        public ICollection<TaskSubmission> Submissions { get; set; } = new List<TaskSubmission>();
    }
}