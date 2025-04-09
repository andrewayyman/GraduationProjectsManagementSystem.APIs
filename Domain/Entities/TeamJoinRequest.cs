using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TeamJoinRequest
    {
        public int Id { get; set; }                                                     // unique id for the request
        public string? Message { get; set; }                                            // message from the student to the team
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;                      // date of the request
        public JoinRequestStatus Status { get; set; } = JoinRequestStatus.Pending;      // status of the request (pending, accepted, rejected)

        // RelationShips

        // one request can be for one team, and one team can have many requests
        public int TeamId { get; set; }                                                 // id of the team that the student wants to join

        public Team? Team { get; set; }

        // one request can be for one student, and one student can have many requests
        public int StudentId { get; set; }                                              // id of the student that wants to join the team

        public Student? Student { get; set; }
    }
}