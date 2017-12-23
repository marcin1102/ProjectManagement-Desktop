using ProjectManagement.Infrastructure.Http;
using ProjectManagement.Infrastructure.Message;
using ProjectManagement.Infrastructure.UserSettings;
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
    /// Interaction logic for ProjectsPage.xaml
    /// </summary>
    public partial class ProjectsPage : Page
    {
        private MainWindow mainWindow;
        private IEnumerable<ProjectListItem> projects;

        public ProjectsPage(MainWindow projectsWindow)
        {
            this.mainWindow = projectsWindow;
            InitializeComponent();
        }

        internal async void LoadProjects()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IEnumerable<ProjectListItem>>($"api/project-management/projects?isAdmin={CurrentUser.Type == UserType.Admin}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                MessageBox.Show("Failed to load projects");
            else
            {
                projects = response.ResponseContent;
                ProjectsDataGrid.ItemsSource = projects.ToList();
            }            
        }

        private void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = mainWindow.AddProjectPage;
        }

        private void ReloadProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProjects();
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.CommandQueryDispatcher.RemoveAccessToken();
            new LoginWindow().Show();
            mainWindow.Visibility = Visibility.Hidden;
            mainWindow.Close();
        }

        private void ManageProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var project = (ProjectListItem)ProjectsDataGrid.SelectedItem;
            if (project == null)
                return;

            mainWindow.ProjectPage.SetProjectId(project.Id);
            mainWindow.MainFrame.Content = mainWindow.ProjectPage;
        }
    }
}
