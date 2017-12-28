using ProjectManagement.Contracts.Sprint.Commands;
using ProjectManagement.Infrastructure.Http;
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

namespace ProjectManagement.Sprints.Pages
{
    /// <summary>
    /// Interaction logic for CreateSprint.xaml
    /// </summary>
    public partial class CreateSprintPage : Page
    {
        private Guid projectId;
        private MainWindow mainWindow;

        public CreateSprintPage(MainWindow mainWindow, Guid projectId)
        {
            InitializeComponent();

            this.projectId = projectId;
            this.mainWindow = mainWindow;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if(!AreFieldsValid())
            {
                return;
            }

            var response = await mainWindow.CommandQueryDispatcher.SendAsync(new CreateSprint(Name.Text, From.SelectedDate.Value, To.SelectedDate.Value), $"api/project-management/projects/{projectId}/sprints", HttpOperationType.POST);
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            mainWindow.MainFrame.Content = new SprintsPage(mainWindow);
            ClearPage();
        }

        private void ClearPage()
        {
            Name.Text = "";
            From.SelectedDate = null;
            To.SelectedDate = null;
        }

        private bool AreFieldsValid()
        {
            bool areValid = true;
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                areValid = false;
                Name.BorderBrush = Brushes.Red;
            }
            else
                Name.BorderBrush = Brushes.Black;

            if(!From.SelectedDate.HasValue)
            {
                areValid = false;
                From.BorderBrush = Brushes.Red;
            }
            else
                From.BorderBrush = Brushes.Black;

            if (!To.SelectedDate.HasValue)
            {
                areValid = false;
                To.BorderBrush = Brushes.Red;
            }
            else
                To.BorderBrush = Brushes.Black;

            return areValid;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = new SprintsPage(mainWindow);
        }
    }
}
