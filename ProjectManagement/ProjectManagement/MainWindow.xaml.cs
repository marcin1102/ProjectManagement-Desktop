using ProjectManagement.Infrastructure.Message;
using ProjectManagement.Projects;
using ProjectManagement.Users;
using System.Windows;

namespace ProjectManagement
{
    public partial class MainWindow : Window
    {
        internal readonly CommandQueryDispatcher CommandQueryDispatcher;
        internal readonly ProjectsPage ProjectsPage;
        internal readonly AddProjectPage AddProjectPage;
        internal readonly UsersPage UsersPage;
        internal readonly CreateUserPage CreateUserPage;
        internal readonly ProjectPage ProjectPage;
        internal ManageUsersPage ManageUsersPage;

        public MainWindow()
        {
            InitializeComponent();

            CommandQueryDispatcher = new CommandQueryDispatcher();
            ProjectsPage = new ProjectsPage(this);
            MainFrame.Content = ProjectsPage;
            AddProjectPage = new AddProjectPage(this);
            UsersPage = new UsersPage(this);
            CreateUserPage = new CreateUserPage(this);
            ProjectPage = new ProjectPage(this);
        }

        private void ProjectsNaviButton_Click(object sender, RoutedEventArgs e)
        {
            var previousFrame = MainFrame.Content;
            MainFrame.Content = ProjectsPage;

            if (previousFrame != ProjectsPage)
                ProjectsPage.LoadProjects();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProjectsPage.LoadProjects();
            UsersPage.LoadUsers();
        }

        private void UsersNaviButton_Click(object sender, RoutedEventArgs e)
        {
            var previousFrame = MainFrame.Content;
            MainFrame.Content = UsersPage;

            if (previousFrame != UsersPage)
                UsersPage.LoadUsers();
        }
    }
}
