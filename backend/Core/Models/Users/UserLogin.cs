using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Users
{
    public class UserLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public virtual User User { get; set; }
    }
}