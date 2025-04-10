using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Identity
{
    public class AppUser: IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Student? Student { get; set; }
        public Supervisor? Supervisor { get; set; }
    }
}
