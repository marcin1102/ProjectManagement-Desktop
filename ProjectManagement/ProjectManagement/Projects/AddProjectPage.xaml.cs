using ProjectManagement.Contracts.Project.Commands;
using ProjectManagement.Infrastructure.Http;
using ProjectManagement.Infrastructure.Message;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ProjectManagement.Projects
{
    /// <summary>
    /// Interaction logic for AddProject.xaml
    /// </summary>
    public partial class AddProjectPage : Page
    {
        private MainWindow mainWindow;

        public AddProjectPage(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        public async Task CreateProject()
        {
            if(string.IsNullOrWhiteSpace(ProjectName.Text))
            {
                MessageBox.Show("Enter project name first");
                return;
            }

            var project = new CreateProject(ProjectName.Text);
            var response = await mainWindow.CommandQueryDispatcher.SendAsync(project, "api/project-management/projects", HttpOperationType.POST);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                mainWindow.MainFrame.Content = mainWindow.ProjectsPage;
                mainWindow.ProjectsPage.LoadProjects();
                ProjectName.Text = "";
            }
            else
                MessageBox.Show(response.ResponseContent);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Content = mainWindow.ProjectsPage;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateProject();
        }
    }
}
