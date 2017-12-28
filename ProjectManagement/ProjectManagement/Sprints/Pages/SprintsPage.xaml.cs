using ProjectManagement.Projects;
using ProjectManagementView.Contracts.Projects.Sprints;
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
    /// Interaction logic for SprintsPage.xaml
    /// </summary>
    public partial class SprintsPage : Page
    {
        private MainWindow mainWindow;

        public SprintsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            LoadSprints();
        }

        public async Task LoadSprints()
        {
            var response = await mainWindow.CommandQueryDispatcher.SendAsync<IReadOnlyCollection<SprintListItem>>($"api/project-management/projects/{ProjectData.ProjectId}/sprints");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show($"Cannot load sprints. Http operation status code: {response.StatusCode}");
                return;
            }

            SprintDataGrid.ItemsSource = response.ResponseContent;
        }

        private async void ReloadSprintsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSprints();
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = mainWindow.ProjectPage;
        }

        private void CreateSprintButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = new CreateSprintPage(mainWindow, ProjectData.ProjectId);
        }
    }
}
