using ProjectManagement.Contracts.Sprint.Enums;
using ProjectManagement.Sprints.Pages;
using ProjectManagementView.Contracts.Issues;
using ProjectManagementView.Contracts.Projects.Sprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private MainWindow mainWindow;
        private IEnumerable<IssueListItem> issues;

        public ProjectPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        public async void SetProject(Guid projectId, string projectName)
        {
            ProjectData.ProjectId = projectId;
            ProjectData.ProjectName = projectName;
            mainWindow.ManageUsersPage = new ManageUsersPage(projectId, mainWindow);
            LoadIssues();
            LoadSprint();
        }

        private async Task LoadSprint()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IEnumerable<SprintListItem>>($"api/project-management/projects/{ProjectData.ProjectId}/sprints");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("Failed to load sprint");
                return;
            }

            var sprintInProgress = response.ResponseContent.SingleOrDefault(x => x.Status == SprintStatus.InProgress);
            if (sprintInProgress == null)
                Sprint.Text = "No sprint currently in progress";
            else
                Sprint.Text = sprintInProgress.Name;
            
        }

        public async Task LoadIssues()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IEnumerable<IssueListItem>>($"api/project-management/projects/{ProjectData.ProjectId}/issues");

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
            mainWindow.MainFrame.Content = new Issue.CreateIssue(mainWindow, ProjectData.ProjectId);
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.CommandQueryDispatcher.RemoveAccessToken();
            new LoginWindow().Show();
            mainWindow.Visibility = Visibility.Hidden;
            mainWindow.Close();
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = mainWindow.ManageUsersPage;
        }

        private void ManageIssueButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIssue = GetSelectedInGridIssue();
            if(selectedIssue == null)
            {
                MessageBox.Show("Select issue first");
                return;
            }
            mainWindow.MainFrame.Content = new Issue.IssuePage(mainWindow, ProjectData.ProjectId, selectedIssue.Id);
        }

        private IssueListItem GetSelectedInGridIssue()
        {
            var selectedIssue = (IssueListItem)TodoDataGrid.SelectedItem;
            if (selectedIssue != null)
                return selectedIssue;

            selectedIssue = (IssueListItem)InProgressDataGrid.SelectedItem;
            if (selectedIssue != null)
                return selectedIssue;

            selectedIssue = (IssueListItem)DoneDataGrid.SelectedItem;
            return selectedIssue;
        }

        private void InProgressDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TodoDataGrid.UnselectAllCells();
            DoneDataGrid.UnselectAllCells();
        }

        private void TodoDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InProgressDataGrid.UnselectAllCells();
            DoneDataGrid.UnselectAllCells();
        }

        private void DoneDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TodoDataGrid.UnselectAllCells();
            InProgressDataGrid.UnselectAllCells();
        }

        private void CreateSprintButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = new CreateSprintPage(mainWindow, ProjectData.ProjectId);
        }

        private async void MoveToInProgressMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedIssue = (IssueListItem)TodoDataGrid.SelectedItem;
            var issuePage = new Issue.IssuePage(mainWindow, ProjectData.ProjectId, selectedIssue.Id);
            await issuePage.LoadIssue();
            var response = await issuePage.MarkAsInProgress();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            await LoadIssues();
        }

        private async void MoveToDoneMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedIssue = (IssueListItem)InProgressDataGrid.SelectedItem;
            var issuePage = new Issue.IssuePage(mainWindow, ProjectData.ProjectId, selectedIssue.Id);
            await issuePage.LoadIssue();
            var response = await issuePage.MarkAsDone();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            await LoadIssues();
        }

        private async void ReloadProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadIssues();
        }

        private void ManageSprintsButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = new SprintsPage(mainWindow);
        }
    }
}
