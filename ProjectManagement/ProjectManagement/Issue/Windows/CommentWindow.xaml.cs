using ProjectManagement.Contracts.Task.Commands;
using ProjectManagementView.Contracts.Issues;
using ProjectManagementView.Contracts.Issues.Enums;
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
using System.Windows.Shapes;

namespace ProjectManagement.Issue
{
    /// <summary>
    /// Interaction logic for CommentWindow.xaml
    /// </summary>
    public partial class CommentWindow : Window
    {
        MainWindow mainWindow;
        IssuePage issuePage;
        Guid projectId;
        Guid issueId;
        IssueType issueType;
        LinkedTo linkedTo;

        public CommentWindow(MainWindow mainWindow, IssuePage issuePage, Guid projectId, Guid issueId, IssueType issueType, LinkedTo linkedTo)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.issuePage = issuePage;
            this.projectId = projectId;
            this.issueId = issueId;
            this.issueType = issueType;
            this.linkedTo = linkedTo;
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(CommentContent.Text))
            {
                MessageBox.Show("Fill comment content first!");
                return;
            }

            (HttpStatusCode statusCode, string ResponseContent) response;
            switch (issueType)
            {
                case IssueType.Task:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new CommentTask(CommentContent.Text), $"/api/project-management/projects/{projectId}/tasks/{issueId}/comment", Infrastructure.Http.HttpOperationType.PATCH);
                    break;
                case IssueType.Nfr:
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new CommentTask(CommentContent.Text), $"/api/project-management/projects/{projectId}/nfrs/{issueId}/comment", Infrastructure.Http.HttpOperationType.PATCH);
                    break;
                case IssueType.Subtask:
                    if(linkedTo == null)
                    {
                        MessageBox.Show("Cannot comment subtask without information about parent task");
                        return;
                    }
                    response = await mainWindow.CommandQueryDispatcher.SendAsync(new CommentTask(CommentContent.Text), $"/api/project-management/projects/{projectId}/tasks/{linkedTo.IssueId}/subtasks/{issueId}/comment", Infrastructure.Http.HttpOperationType.PATCH);
                    break;
                case IssueType.Bug:
                    if (linkedTo == null)
                    {
                        MessageBox.Show("Cannot comment subtask without information about parent task");
                        return;
                    }
                    if (linkedTo.IssueType == IssueType.Task)
                        response = await mainWindow.CommandQueryDispatcher.SendAsync(new CommentTask(CommentContent.Text), $"/api/project-management/projects/{projectId}/tasks/{linkedTo.IssueId}/bugs/{issueId}/comment", Infrastructure.Http.HttpOperationType.PATCH);
                    else if (linkedTo.IssueType == IssueType.Nfr)
                        response = await mainWindow.CommandQueryDispatcher.SendAsync(new CommentTask(CommentContent.Text), $"/api/project-management/projects/{projectId}/nfrs/{linkedTo.IssueId}/bugs/{issueId}/comment", Infrastructure.Http.HttpOperationType.PATCH);
                    else
                    {
                        MessageBox.Show("Incorrect parent issue type");
                        return;
                    }
                    break;
                default:
                    response = (HttpStatusCode.NotFound, "Something went wrong!");
                    break;
            }
            if(response.statusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }
            issuePage.LoadIssue();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
