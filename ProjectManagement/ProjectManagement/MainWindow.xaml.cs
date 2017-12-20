using ProjectManagement.Infrastructure.Message;
using ProjectManagement.Projects;
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
using System.Windows.Shapes;

namespace ProjectManagement
{
    public partial class MainWindow : Window
    {
        internal readonly CommandQueryDispatcher CommandQueryDispatcher;
        internal readonly ProjectsPage ProjectsPage;
        internal readonly AddProjectPage AddProjectPage;

        public MainWindow()
        {
            InitializeComponent();

            CommandQueryDispatcher = new CommandQueryDispatcher();
            ProjectsPage = new ProjectsPage(this);
            MainFrame.Content = ProjectsPage;
            AddProjectPage = new AddProjectPage(this);
        }

        private void ProjectsNaviButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = ProjectsPage;
            ProjectsPage.LoadProjects();
        }
    }
}
