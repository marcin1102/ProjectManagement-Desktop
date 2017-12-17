using ProjectManagement.Infrastructure.Message;
using ProjectManagement.Infrastructure.UserSettings;
using ProjectManagementView.Contracts.Projects;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectManagement.Projects
{
    /// <summary>
    /// Interaction logic for ProjectsWindow.xaml
    /// </summary>
    public partial class ProjectsWindow : Window
    {
        private readonly CommandQueryDispatcher commandQueryDispatcher;

        public ProjectsWindow()
        {
            InitializeComponent();
            commandQueryDispatcher = new CommandQueryDispatcher();
            LoadProjects();
        }

        private async void LoadProjects()
        {
            var projects = await commandQueryDispatcher.SendAsync<IEnumerable<ProjectListItem>>($"api/project-management/projects?isAdmin={CurrentUser.Type == UserType.Admin}");
            ProjectsDataGrid.DataContext = projects.ResponseContent.Select(x => x.Name);
        }
    }
}
