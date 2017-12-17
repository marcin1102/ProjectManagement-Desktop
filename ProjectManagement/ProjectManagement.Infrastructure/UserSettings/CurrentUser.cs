using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Infrastructure.UserSettings
{
    public static class CurrentUser
    {
        public static Guid Id { get; set; }
        public static string Email { get; set; }
        public static UserType Type { get; set; }
    }


    public enum UserType
    {
        Admin, User
    }
}
