using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Contracts.User.Enums;

namespace ProjectManagement.Users
{
    public class UserRole
    {
        public static List<string> Roles { get => Enum.GetNames(typeof(Role)).ToList(); }
    }
}
