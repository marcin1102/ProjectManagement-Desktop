using ProjectManagement.Contracts.Project.Commands;
using ProjectManagement.Infrastructure.Message;
using ProjectManagementView.Contracts.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UserManagement.Contracts.User.Queries;

namespace ProjectManagement.Projects
{
    /// <summary>
    /// Interaction logic for AssignUserWindow.xaml
    /// </summary>
    public partial class AssignUserWindow : Window
    {
        ManageUsersPage page;
        Guid projectId;
        CommandQueryDispatcher commandQueryDispatcher;
        IEnumerable<UserListItem> users;

        public AssignUserWindow(ManageUsersPage page,  Guid projectId)
        {
            InitializeComponent();
            this.page = page;
            this.projectId = projectId;
            commandQueryDispatcher = new CommandQueryDispatcher();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            await AssignUserToProject();
        }

        private async Task AssignUserToProject()
        {
            var user = (UserListItem)Users.SelectedItem;
            if (user == null)
                return;

            var response = await commandQueryDispatcher.SendAsync(new AssignUserToProject(user.Id), $"api/project-management/projects/{projectId}/assign-member", Infrastructure.Http.HttpOperationType.PATCH);
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            Close();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            var response = await commandQueryDispatcher.SendAsync<IReadOnlyCollection<UserListItem>>("api/user-management/users");
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("Cannot load users. Try again later");
                return;
            }

            users = response.ResponseContent;
            Users.ItemsSource = users;
        }

        private async void Reload_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }
    }
}
