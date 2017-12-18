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
        private ProjectsWindow projectsWindow;
        private IEnumerable<ProjectListItem> projects;

        public ProjectsPage(ProjectsWindow projectsWindow)
        {
            this.projectsWindow = projectsWindow;
            InitializeComponent();
            LoadProjects();
        }

        internal async void LoadProjects()
        {
            var response = await projectsWindow.CommandQueryDispatcher.SendAsync<IEnumerable<ProjectListItem>>($"api/project-management/projects?isAdmin={CurrentUser.Type == UserType.Admin}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                MessageBox.Show("Failed to load projects");
            else
            {
                projects = response.ResponseContent;
                ProjectsDataGrid.DataContext = projects.Select(x => x.Name);
            }            
        }

        private void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            projectsWindow.Content = projectsWindow.AddProjectPage;
        }

        private void ReloadProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProjects();
        }
    }
}
