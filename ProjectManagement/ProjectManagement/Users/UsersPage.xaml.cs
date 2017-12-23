using ProjectManagement.Infrastructure.Http;
using ProjectManagement.Infrastructure.UserSettings;
using ProjectManagement.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UserManagement.Contracts.User.Commands;
using UserManagement.Contracts.User.Enums;
using UserManagement.Contracts.User.Queries;

namespace ProjectManagement.Users
{
    /// <summary>
    /// Interaction logic for UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        private MainWindow mainWindow;
        private IEnumerable<UserCollectionItem> Users;

        public UsersPage(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        internal async void LoadUsers()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IEnumerable<UserListItem>>($"api/User-management/Users");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                MessageBox.Show("Failed to load Users");
            else
            {
                Users = response.ResponseContent.Select(x => UserCollectionItem.FromContract(x)).ToList();
                UserDataGrid.ItemsSource = Users; 
            }
        }

        private void CreateUserButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = mainWindow.CreateUserPage;
        }

        private void ReloadUsersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.CommandQueryDispatcher.RemoveAccessToken();
            new LoginWindow().Show();
            mainWindow.Visibility = Visibility.Hidden;
            mainWindow.Close();
        }

        private void ConfirmChangeButton_Click(object sender, RoutedEventArgs e)
        {
            GrantRole();            
        }

        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var user = (UserCollectionItem)UserDataGrid.SelectedItem;
            if (user == null)
                return;
            var column = UserDataGrid.Columns.Single(x => (string)x.Header == "ConfirmChange");

            // TODO: Ogarnąć włączanie się jednego przycisku akceptacji nadania uprawnień
            var newRole = e.AddedItems.Cast<string>().Single();

            if (newRole != user.Role)
            {
                column.Visibility = Visibility.Visible;
                user.ChangeRole(newRole);
            }
            else
            {
                column.Visibility = Visibility.Hidden;
                user.ChangeRole(null);
            }
            return;
        }

        private async void GrantRole()
        {
            var item = UserDataGrid.SelectedItem;
            var user = (UserCollectionItem)item;
            var grantRole = new GrantRole((Role)Enum.Parse(typeof(Role), user.NewRole), user.Version);
            var response = await mainWindow.CommandQueryDispatcher.SendAsync(grantRole, $"api/user-management/users/{user.Id}/grant-role", Infrastructure.Http.HttpOperationType.PATCH);
            if(response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                MessageBox.Show("Grant Role");
                return;
            }

            user.ResetRoleChange(user.NewRole);
            var column = UserDataGrid.Columns.Single(x => (string)x.Header == "ConfirmChange");
            column.Visibility = Visibility.Hidden;
        }
    }
}

