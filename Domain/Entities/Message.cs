using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public string MessageText { get; set; }

        public string SenderName { get; set; }

        public int? TeamId { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Team Team { get; set; }
    }
}