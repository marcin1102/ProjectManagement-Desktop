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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectManagement.Projects
{
    /// <summary>
    /// Interaction logic for ManageUsersPage.xaml
    /// </summary>
    public partial class ManageUsersPage : Page
    {
        private Guid projectId;
        private MainWindow mainWindow;
        private IEnumerable<UserData> users;

        public ManageUsersPage(Guid projectId, MainWindow mainWindow)
        {
            InitializeComponent();
            this.projectId = projectId;
            this.mainWindow = mainWindow;
            LoadUsers();
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.CommandQueryDispatcher.RemoveAccessToken();
            new LoginWindow().Show();
            mainWindow.Visibility = Visibility.Hidden;
            mainWindow.Close();
        }

        private void ReloadUsersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        internal async void LoadUsers()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IEnumerable<UserData>>($"api/project-management/projects/{projectId}/users");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                MessageBox.Show("Failed to load projects");
            else
            {
                users = response.ResponseContent;
                UsersDataGrid.ItemsSource = users;
            }
        }

        private void AssignUserButton_Click(object sender, RoutedEventArgs e)
        {
            new AssignUserWindow(this, projectId).ShowDialog();
        }
    }
}
