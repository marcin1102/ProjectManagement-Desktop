using ProjectManagement.Contracts.Sprint.Commands;
using ProjectManagement.Contracts.Sprint.Enums;
using ProjectManagement.Infrastructure.Http;
using ProjectManagement.Issue;
using ProjectManagement.Projects;
using ProjectManagementView.Contracts.Issues;
using ProjectManagementView.Contracts.Projects.Sprints;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ProjectManagement.Sprints.Pages
{
    /// <summary>
    /// Interaction logic for ManageSprint.xaml
    /// </summary>
    public partial class ManageSprint : Page
    {
        private MainWindow mainWindow;
        private Guid sprintId;
        private SprintResponse sprint;

        public ManageSprint(MainWindow mainWindow, Guid sprintId)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.sprintId = sprintId;
            LoadSprint();
        }

        public async Task LoadSprint()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<SprintResponse>($"/api/project-management/projects/{ProjectData.ProjectId}/sprints/{sprintId}");
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show($"Cannot get sprint from server. Http status code {response.StatusCode}");
                return;
            }

            sprint = response.ResponseContent;
            SprintName.Text = sprint.Name;
            Status.Text = sprint.Status.ToString();
            From.Text = sprint.Start.Date.ToShortDateString();
            To.Text = sprint.End.Date.ToShortDateString();

            switch (sprint.Status)
            {
                case Contracts.Sprint.Enums.SprintStatus.Planned:
                    StartSprint.IsEnabled = true;
                    FinishSprint.IsEnabled = false;
                    break;
                case Contracts.Sprint.Enums.SprintStatus.InProgress:
                    StartSprint.IsEnabled = false;
                    FinishSprint.IsEnabled = true;
                    break;
                case Contracts.Sprint.Enums.SprintStatus.Finished:
                    StartSprint.IsEnabled = false;
                    FinishSprint.IsEnabled = false;
                    UnfinishedIssues.ItemsSource = sprint.UnfinishedIssues;
                    break;
                default:
                    break;
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = new SprintsPage(mainWindow);
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSprint();
        }

        private async void StartSprint_Click(object sender, RoutedEventArgs e)
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync(new StartSprint(sprintId, ProjectData.ProjectId), $"/api/project-management/projects/{ProjectData.ProjectId}/sprints/{sprintId}/start", HttpOperationType.PATCH);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show($"{response.ResponseContent}");
                return;
            }

            await LoadSprint();
        }

        private async void FinishSprint_Click(object sender, RoutedEventArgs e)
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync(new StartSprint(sprintId, ProjectData.ProjectId), $"/api/project-management/projects/{ProjectData.ProjectId}/sprints/{sprintId}/finish", HttpOperationType.PATCH);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show($"{response.ResponseContent}");
                return;
            }

            await LoadSprint();
        }

        private void ManageIssue_Click(object sender, RoutedEventArgs e)
        {
            var issue = (UnfinishedIssue)UnfinishedIssues.SelectedItem;
            if(issue == null)
            {
                MessageBox.Show("Select issue first");
                return;
            }
            mainWindow.MainFrame.Content = new IssuePage(mainWindow, ProjectData.ProjectId, issue.IssueId, this);
        }
    }
}
