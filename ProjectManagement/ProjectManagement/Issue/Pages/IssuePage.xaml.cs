using ProjectManagement.Contracts.Bug.Commands;
using ProjectManagement.Contracts.Nfr.Commands;
using ProjectManagement.Contracts.Task.Commands;
using ProjectManagement.Infrastructure.Http;
using ProjectManagement.Projects;
using ProjectManagementView.Contracts.Issues;
using ProjectManagementView.Contracts.Issues.Enums;
using ProjectManagementView.Contracts.Projects;
using ProjectManagementView.Contracts.Projects.Sprints;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ProjectManagement.Issue
{
    /// <summary>
    /// Interaction logic for IssuePage.xaml
    /// </summary>
    public partial class IssuePage : Page, INotifyPropertyChanged
    {
        private Visibility assigneeConfirmButtonVisibility { get; set; }
        public Visibility AssigneeConfirmButtonVisibility
        {
            get { return assigneeConfirmButtonVisibility; }
            set { assigneeConfirmButtonVisibility = value; OnPropertyChanged("AssigneeConfirmButtonVisibility"); }
        }

        private Visibility sprintConfirmButtonVisibility { get; set; }
        public Visibility SprintConfirmButtonVisibility
        {
            get { return sprintConfirmButtonVisibility; }
            set { sprintConfirmButtonVisibility = value; OnPropertyChanged("SprintConfirmButtonVisibility"); }
        }

        MainWindow mainWindow;
        Guid projectId;
        Guid issueId;
        Page previousPage;
        IssueResponse issueResponse;
        UserData newAssignee;
        SprintListItem newSprint;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public IssuePage(MainWindow mainWindow, Guid projectId, Guid issueId, Page previousPage)
        {
            mainWindow.DataContext = this;
            AssigneeConfirmButtonVisibility = Visibility.Hidden;
            SprintConfirmButtonVisibility = Visibility.Hidden;

            InitializeComponent();
            this.mainWindow = mainWindow;
            this.projectId = projectId;
            this.issueId = issueId;
            this.previousPage = previousPage;
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = previousPage;
            try
            {
                var projectPage = (ProjectPage)previousPage;
                projectPage.LoadIssues();
            }
            catch (Exception)
            {
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadIssue();
            LoadDependecies();
        }

        public async Task LoadIssue()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IssueResponse>($"api/project-management/projects/{projectId}/issues/{issueId}");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show($"Cannot load issue. Http operation status code: {response.StatusCode}");
                mainWindow.MainFrame.Content = previousPage;
                return;
            }

            issueResponse = response.ResponseContent;

            TitleTextBlock.Text = issueResponse.Title;
            DescriptionTextBlock.Text = issueResponse.Description;
            Status.Text = issueResponse.Status.ToString();
            Reporter.Text = issueResponse.ReporterFullName;
            Comments.ItemsSource = issueResponse.Comments;

            switch (issueResponse.Status)
            {
                case Contracts.Issue.Enums.IssueStatus.Todo:
                    MoveToDoneButton.IsEnabled = false;
                    MoveToInProgressButton.IsEnabled = true;
                    break;
                case Contracts.Issue.Enums.IssueStatus.InProgress:
                    MoveToDoneButton.IsEnabled = true;
                    MoveToInProgressButton.IsEnabled = false;
                    break;
                case Contracts.Issue.Enums.IssueStatus.Done:
                    MoveToDoneButton.IsEnabled = false;
                    MoveToInProgressButton.IsEnabled = false;
                    break;
            }
        }

        private async Task LoadDependecies()
        {
            await LoadPossibleAssignees();
            await LoadSprints();
            await LoadRelatedIssues();
        }

        private async Task LoadRelatedIssues()
        {
            (HttpStatusCode StatusCode, IReadOnlyCollection<IssueListItem> ResponseContent) response = (HttpStatusCode.NotFound, null);
            switch (issueResponse.IssueType)
            {
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Task:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync<IReadOnlyCollection<IssueListItem>>($"api/project-management/projects/{projectId}/tasks/{issueId}/related-issues");
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Nfr:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync<IReadOnlyCollection<IssueListItem>>($"api/project-management/projects/{projectId}/nfrs/{issueId}/related-issues");
                    break;
                default:
                    return;
            }

            if(response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Cannot load related issues. Http operation status code: {response.StatusCode}");
                return;
            }

            RelatedIssues.ItemsSource = response.ResponseContent;
        }

        private async Task LoadSprints()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IReadOnlyCollection<SprintListItem>>($"api/project-management/projects/{projectId}/sprints");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show($"Cannot load sprints. Http operation status code: {response.StatusCode}");
                return;
            }

            Sprint.ItemsSource = response.ResponseContent;
            if (issueResponse.SprintId.HasValue)
                Assignee.SelectedItem = response.ResponseContent.SingleOrDefault(x => x.Id == issueResponse.SprintId.Value);
        }

        private async Task LoadPossibleAssignees()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IReadOnlyCollection<UserData>>($"api/project-management/projects/{projectId}/users");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show($"Cannot load assignee. Http operation status code: {response.StatusCode}");
                return;
            }

            Assignee.ItemsSource = response.ResponseContent;
            if (issueResponse.AssigneeId.HasValue)
                Assignee.SelectedItem = response.ResponseContent.SingleOrDefault(x => x.Id == issueResponse.AssigneeId.Value);
        }

        private void CommentButton_Click(object sender, RoutedEventArgs e)
        {
            new CommentWindow(mainWindow, this, projectId, issueId, issueResponse.IssueType, issueResponse.LinkedTo).ShowDialog();
        }

        private void Assignee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var assignee = (UserData)Assignee.SelectedItem;
            if (assignee != null && assignee.Id != issueResponse.AssigneeId)
            {
                newAssignee = assignee;
                AssigneeConfirmButtonVisibility = Visibility.Visible;
            }
            else
                AssigneeConfirmButtonVisibility = Visibility.Hidden;
        }

        private void Sprint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sprint = (SprintListItem)Sprint.SelectedItem;
            if (sprint != null && sprint.Id != issueResponse.SprintId)
            {
                newSprint = sprint;
                SprintConfirmButtonVisibility = Visibility.Visible;
            }
            else
                SprintConfirmButtonVisibility = Visibility.Hidden;
        }

        private async void ConfirmAssignee_Click(object sender, RoutedEventArgs e)
        {
            (HttpStatusCode StatusCode, string ResponseContent) response = (HttpStatusCode.NotFound , "Something went wrong");
            switch (issueResponse.IssueType)
            {
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Task:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignAssigneeToTask(newAssignee.Id), $"api/project-management/projects/{projectId}/tasks/{issueId}/assign-assignee", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Nfr:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignAssigneeToNfr(newAssignee.Id), $"api/project-management/projects/{projectId}/nfrs/{issueId}/assign-assignee", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Subtask:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignAssigneeToSubtask(newAssignee.Id), $"api/project-management/projects/{projectId}/tasks/{issueResponse.LinkedTo.IssueId}/subtasks/{issueId}/assign-assignee", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Bug:
                    if(issueResponse.IsLinkedIssue)
                    {
                        if(issueResponse.LinkedTo.IssueType == IssueType.Task)
                            response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignAssigneeToTasksBug(newAssignee.Id), $"api/project-management/projects/{projectId}/tasks/{issueResponse.LinkedTo.IssueId}/bugs/{issueId}/assign-assignee", HttpOperationType.PATCH);
                        else
                            response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignAssigneeToNfrsBug(newAssignee.Id), $"api/project-management/projects/{projectId}/nfrs/{issueResponse.LinkedTo.IssueId}/bugs/{issueId}/assign-assignee", HttpOperationType.PATCH);
                    }
                    else
                        response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignAssigneeToBug(newAssignee.Id), $"api/project-management/projects/{projectId}/bugs/{issueId}/assign-assignee", HttpOperationType.PATCH);
                    break;
            }

            if(response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            newAssignee = null;
            AssigneeConfirmButtonVisibility = Visibility.Hidden;
        }

        private async void ConfirmSprint_Click(object sender, RoutedEventArgs e)
        {
            (HttpStatusCode StatusCode, string ResponseContent) response = (HttpStatusCode.NotFound, "Something went wrong");
            switch (issueResponse.IssueType)
            {
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Task:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignTaskToSprint(newSprint.Id), $"api/project-management/projects/{projectId}/tasks/{issueId}/assign-to-sprint", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Nfr:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignNfrToSprint(newSprint.Id), $"api/project-management/projects/{projectId}/nfrs/{issueId}/assign-to-sprint", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Subtask:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignSubtaskToSprint(newSprint.Id), $"api/project-management/projects/{projectId}/tasks/{issueResponse.LinkedTo.IssueId}/subtasks/{issueId}/assign-to-sprint", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Bug:
                    if (issueResponse.IsLinkedIssue)
                    {
                        if (issueResponse.LinkedTo.IssueType == IssueType.Task)
                            response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignTasksBugToSprint(newSprint.Id), $"api/project-management/projects/{projectId}/tasks/{issueResponse.LinkedTo.IssueId}/bugs/{issueId}/assign-to-sprint", HttpOperationType.PATCH);
                        else
                            response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignNfrsBugToSprint(newSprint.Id), $"api/project-management/projects/{projectId}/nfrs/{issueResponse.LinkedTo.IssueId}/bugs/{issueId}/assign-to-sprint", HttpOperationType.PATCH);
                    }
                    else
                        response = await mainWindow.CommandQueryDispatcher.SendAsync(new AssignBugToSprint(newSprint.Id), $"api/project-management/projects/{projectId}/bugs/{issueId}/assign-to-sprint", HttpOperationType.PATCH);
                    break;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            newSprint = null;
            SprintConfirmButtonVisibility = Visibility.Hidden;
        }

        private async void MoveToInProgressButton_Click(object sender, RoutedEventArgs e)
        {
            var response = await MarkAsInProgress();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            MoveToInProgressButton.IsEnabled = false;
            MoveToDoneButton.IsEnabled = true;
        }

        public async Task<(HttpStatusCode StatusCode, string ResponseContent)> MarkAsInProgress()
        {
            (HttpStatusCode StatusCode, string ResponseContent) response = (HttpStatusCode.NotFound, "Something went wrong");
            switch (issueResponse.IssueType)
            {
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Task:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkTaskAsInProgress(), $"api/project-management/projects/{projectId}/tasks/{issueId}/mark-as-in-progress", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Nfr:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkNfrAsInProgress(), $"api/project-management/projects/{projectId}/nfrs/{issueId}/mark-as-in-progress", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Subtask:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkSubtaskAsInProgress(), $"api/project-management/projects/{projectId}/tasks/{issueResponse.LinkedTo.IssueId}/subtasks/{issueId}/mark-as-in-progress", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Bug:
                    if (issueResponse.IsLinkedIssue)
                    {
                        if (issueResponse.LinkedTo.IssueType == IssueType.Task)
                            response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkTasksBugAsInProgress(), $"api/project-management/projects/{projectId}/tasks/{issueResponse.LinkedTo.IssueId}/bugs/{issueId}/mark-as-in-progress", HttpOperationType.PATCH);
                        else
                            response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkNfrsBugAsInProgress(), $"api/project-management/projects/{projectId}/nfrs/{issueResponse.LinkedTo.IssueId}/bugs/{issueId}/mark-as-in-progress", HttpOperationType.PATCH);
                    }
                    else
                        response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkBugAsInProgress(), $"api/project-management/projects/{projectId}/bugs/{issueId}/mark-as-in-progress", HttpOperationType.PATCH);
                    break;
            }

            return response;
        }

        public async Task<(HttpStatusCode StatusCode, string ResponseContent)> MarkAsDone()
        {
            (HttpStatusCode StatusCode, string ResponseContent) response = (HttpStatusCode.NotFound, "Something went wrong");
            switch (issueResponse.IssueType)
            {
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Task:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkTaskAsDone(), $"api/project-management/projects/{projectId}/tasks/{issueId}/mark-as-done", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Nfr:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkNfrAsDone(), $"api/project-management/projects/{projectId}/nfrs/{issueId}/mark-as-done", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Subtask:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkSubtaskAsDone(), $"api/project-management/projects/{projectId}/tasks/{issueResponse.LinkedTo.IssueId}/subtasks/{issueId}/mark-as-done", HttpOperationType.PATCH);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Bug:
                    if (issueResponse.IsLinkedIssue)
                    {
                        if (issueResponse.LinkedTo.IssueType == IssueType.Task)
                            response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkTasksBugAsDone(), $"api/project-management/projects/{projectId}/tasks/{issueResponse.LinkedTo.IssueId}/bugs/{issueId}/mark-as-done", HttpOperationType.PATCH);
                        else
                            response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkNfrsBugAsDone(), $"api/project-management/projects/{projectId}/nfrs/{issueResponse.LinkedTo.IssueId}/bugs/{issueId}/mark-as-done", HttpOperationType.PATCH);
                    }
                    else
                        response = await mainWindow.CommandQueryDispatcher.SendAsync(new MarkBugAsDone(), $"api/project-management/projects/{projectId}/bugs/{issueId}/mark-as-done", HttpOperationType.PATCH);
                    break;
            }

            return response;
        }

        private async void MoveToDoneButton_Click(object sender, RoutedEventArgs e)
        {
            var response = await MarkAsDone();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            MoveToInProgressButton.IsEnabled = false;
            MoveToDoneButton.IsEnabled = false;
        }

        private void ManageIssue_Click(object sender, RoutedEventArgs e)
        {
            var issue = (IssueListItem)RelatedIssues.SelectedItem;
            if (issue == null)
            {
                MessageBox.Show("Select issue first");
                return;
            }
            mainWindow.MainFrame.Content = new IssuePage(mainWindow, ProjectData.ProjectId, issue.Id, this);
        }
    }
}
