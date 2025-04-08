﻿using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface ITokenService
    {
         Task<string> CreateTokenAsync(AppUser User,UserManager<AppUser> userManager);
    }
}
