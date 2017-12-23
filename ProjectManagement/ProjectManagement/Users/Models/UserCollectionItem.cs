using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UserManagement.Contracts.User.Queries;

namespace ProjectManagement.Users.Models
{
    public class UserCollectionItem
    {
        public UserCollectionItem(Guid id, string firstName, string lastName, string email, string role, long version)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Role = role;
            Version = version;
            RoleChanged = Visibility.Hidden;
        }

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public long Version { get; private set; }
        public string NewRole { get; set; }
        public Visibility RoleChanged { get; private set; }


        public static UserCollectionItem FromContract(UserListItem item)
        {
            return new UserCollectionItem(item.Id, item.FirstName, item.LastName, item.Email, item.Role, item.Version);
        }

        internal void ChangeRole(string role)
        {
            if(string.IsNullOrEmpty(role) || role == Role)
            {
                RoleChanged = Visibility.Hidden;
                NewRole = null;
                return;
            }

            NewRole = role;
            RoleChanged = Visibility.Visible;
        }

        internal void ResetRoleChange(string newRole)
        {
            Role = newRole;
            NewRole = null;
            RoleChanged = Visibility.Hidden;
        }
    }
}
