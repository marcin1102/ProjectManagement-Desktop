using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Infrastructure.UserSettings
{
    public class CurrentUser
    {
        public static Guid Id { get; set; }
        public static string FullName { get; set; }
        public static string Email { get; set; }
        public static UserType Type { get; set; }
        public static bool IsAdmin { get => Type == UserType.Admin; }
    }


    public enum UserType
    {
        Admin, User
    }
}
