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

        public string SenderName { get; set; }   // اللي بعت الرسالة (اسم الطالب أو المشرف)

        public int? TeamId { get; set; }          // فريق اللي الرسالة تخصه

        public DateTime SentAt { get; set; } = DateTime.UtcNow;  // توقيت إرسال الرسالة

        public Team Team { get; set; }
    }
}
