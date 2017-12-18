using ProjectManagement.Infrastructure.Message;
using ProjectManagement.Infrastructure.UserSettings;
using ProjectManagementView.Contracts.Projects;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace ProjectManagement.Projects
{
    /// <summary>
    /// Interaction logic for ProjectsWindow.xaml
    /// </summary>
    public partial class ProjectsWindow : Window
    {
        internal readonly CommandQueryDispatcher CommandQueryDispatcher;
        internal readonly ProjectsPage ProjectsPage;
        internal readonly AddProjectPage AddProjectPage;

        public ProjectsWindow()
        {
            InitializeComponent();

            CommandQueryDispatcher = new CommandQueryDispatcher();
            ProjectsPage = new ProjectsPage(this);
            Content = ProjectsPage;
            AddProjectPage = new AddProjectPage(this);
        }        
    }
}
