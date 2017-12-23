using ProjectManagementView.Contracts.Issues;
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
    /// Interaction logic for ProjectPage.xaml
    /// </summary>
    public partial class ProjectPage : Page
    {
        private Guid projectId;
        private MainWindow mainWindow;
        private IEnumerable<IssueListItem> issues;

        public ProjectPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        public async void SetProjectId(Guid projectId)
        {
            this.projectId = projectId;
            mainWindow.CreateIssuePage = new Issue.CreateIssue(mainWindow, projectId);
            await LoadIssues();
        }

        public async Task LoadIssues()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IEnumerable<IssueListItem>>($"api/project-management/projects/{projectId}/issues");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                MessageBox.Show("Failed to load projects");
            else
            {
                issues = response.ResponseContent;
                TodoDataGrid.ItemsSource = issues.Where(x => x.Status == Contracts.Issue.Enums.IssueStatus.Todo);
                InProgressDataGrid.ItemsSource = issues.Where(x => x.Status == Contracts.Issue.Enums.IssueStatus.InProgress);
                DoneDataGrid.ItemsSource = issues.Where(x => x.Status == Contracts.Issue.Enums.IssueStatus.Done);
            }
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = mainWindow.CreateIssuePage;
        }
    }
}
