using ProjectManagement.Contracts.Issue.Commands;
using ProjectManagement.Contracts.Nfr.Commands;
using ProjectManagement.Contracts.Task.Commands;
using ProjectManagement.Infrastructure.UserSettings;
using ProjectManagementView.Contracts.Issues;
using ProjectManagementView.Contracts.Issues.Enums;
using ProjectManagementView.Contracts.Projects;
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

namespace ProjectManagement.Issue
{
    /// <summary>
    /// Interaction logic for CreateIssue.xaml
    /// </summary>
    public partial class CreateIssue : Page, INotifyPropertyChanged
    {
        private Guid projectId;
        private MainWindow mainWindow;

        public static IssueType issueType;
        private bool isLinkable;
        public bool IsLinkable
        {
            get => isLinkable;
            set
            {
                isLinkable = value;
                OnPropertyChanged("IsLinkable");
            }
        }

        private IEnumerable<IssueListItem> issues;
        private IEnumerable<UserData> users;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CreateIssue(MainWindow mainWindow, Guid projectId)
        {
            mainWindow.DataContext = this;

            InitializeComponent();
            this.projectId = projectId;
            this.mainWindow = mainWindow;
        }

        private void IssueType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            issueType = (IssueType)Enum.Parse(typeof(IssueType), (string)IssueType.SelectedItem);
            IsLinkable = issueType == ProjectManagementView.Contracts.Issues.Enums.IssueType.Bug ||
                            issueType == ProjectManagementView.Contracts.Issues.Enums.IssueType.Subtask;

            switch (issueType)
            {
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Subtask:
                    IssuesToLink.ItemsSource = issues
                        .Where(x => x.IssueType == ProjectManagementView.Contracts.Issues.Enums.IssueType.Task)
                        .Select(x => new IssueToLink(x.Id, x.Title, x.IssueType))
                        .ToList();
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Bug:
                    IssuesToLink.ItemsSource = issues
                        .Where(x => x.IssueType == ProjectManagementView.Contracts.Issues.Enums.IssueType.Task || x.IssueType == ProjectManagementView.Contracts.Issues.Enums.IssueType.Nfr)
                        .Select(x => new IssueToLink(x.Id, x.Title, x.IssueType))
                        .ToList();
                    break;
                default:
                    break;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
            await LoadIssues();
        }

        private async Task LoadIssues()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IEnumerable<IssueListItem>>($"api/project-management/projects/{projectId}/issues");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                MessageBox.Show("Failed to load projects");
            else
            {
                issues = response.ResponseContent;
            }
        }

        private async Task LoadUsers()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IEnumerable<UserData>>($"api/project-management/projects/{projectId}/users");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                MessageBox.Show("Failed to load projects");
            else
            {
                users = response.ResponseContent;
                Assignee.ItemsSource = users;
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var areFieldsValid = AreFieldsValid();
            if (!areFieldsValid)
                return;

            var assignee = (UserData)Assignee.SelectedItem;
            switch (issueType)
            {
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Task:
                    await CreateNewIssue(new CreateTask(Title.Text, Description.Text, CurrentUser.Id, assignee?.Id, null));
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Nfr:
                    await CreateNewIssue(new CreateNfr(Title.Text, Description.Text, CurrentUser.Id, assignee?.Id, null));
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Subtask:
                    var linkIssue = (IssueToLink)IssuesToLink.SelectedItem;
                    if(linkIssue == null)
                    {
                        MessageBox.Show("You must choose Task to link first");
                        return;
                    }
                    await CreateNewIssue(new AddSubtaskToTask(Title.Text, Description.Text, CurrentUser.Id, assignee?.Id, null), ProjectManagementView.Contracts.Issues.Enums.IssueType.Task, linkIssue.Id);
                    break;
                case ProjectManagementView.Contracts.Issues.Enums.IssueType.Bug:
                    linkIssue = (IssueToLink)IssuesToLink.SelectedItem;
                    if (linkIssue == null)
                    {
                        MessageBox.Show("You must choose Task or Nfr to link first");
                        return;
                    }
                    switch (linkIssue.IssueType)
                    {
                        case ProjectManagementView.Contracts.Issues.Enums.IssueType.Task:
                            await CreateNewIssue(new AddBugToTask(Title.Text, Description.Text, CurrentUser.Id, assignee?.Id, null), linkIssue.IssueType, linkIssue.Id);
                            break;
                        case ProjectManagementView.Contracts.Issues.Enums.IssueType.Nfr:
                            await CreateNewIssue(new AddBugToNfr(Title.Text, Description.Text, CurrentUser.Id, assignee?.Id, null), linkIssue.IssueType, linkIssue.Id);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private async Task CreateNewIssue(ICreateIssue createIssue, IssueType? linkTo = null, Guid? linkId = null)
        {
            string uri;
            if (linkTo == null)
                uri = $"api/project-management/projects/{projectId}/{issueType.ToString().ToLower()}s";
            else
                uri = $"api/project-management/projects/{projectId}/{linkTo.ToString().ToLower()}s/{linkId}/add-{issueType.ToString().ToLower()}";

            var response = await mainWindow.CommandQueryDispatcher.SendAsync(createIssue, uri, Infrastructure.Http.HttpOperationType.POST);
            if(response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            mainWindow.MainFrame.Content = mainWindow.ProjectPage;
            await mainWindow.ProjectPage.LoadIssues();
        }

        private bool AreFieldsValid()
        {
            bool areValid = true;
            if (string.IsNullOrWhiteSpace(Title.Text))
            {
                areValid = false;
                Title.BorderBrush = Brushes.Red;
            }
            else
                Title.BorderBrush = Brushes.Black;
           
            return areValid;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = mainWindow.ProjectPage;
        }
    }

    class IssueToLink
    {
        public IssueToLink(Guid id, string title, IssueType issueType)
        {
            Id = id;
            Title = title;
            IssueType = issueType;
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public IssueType IssueType { get; private set; }
    }
}
