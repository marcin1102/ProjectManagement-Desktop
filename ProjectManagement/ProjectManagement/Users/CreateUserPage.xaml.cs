using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectManagement.Users
{
    /// <summary>
    /// Interaction logic for CreateUser.xaml
    /// </summary>
    public partial class CreateUserPage : Page
    {
        private MainWindow mainWindow;
        
        public CreateUserPage(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MainFrame.Content = mainWindow.UsersPage;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateUser();
        }

        private async Task CreateUser()
        {
            if (!AreFieldsValid())
                return;

            var createUser = new UserManagement.Contracts.User.Commands.CreateUser(FirstName.Text, LastName.Text, Email.Text, Password.Password,
                (UserManagement.Contracts.User.Enums.Role)Enum.Parse(typeof(UserManagement.Contracts.User.Enums.Role), Role.Text));

            var response = await mainWindow.CommandQueryDispatcher.SendAsync(createUser, "api/user-management/users", Infrastructure.Http.HttpOperationType.POST);
            if(response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                MessageBox.Show(response.ResponseContent);
                return;
            }

            mainWindow.MainFrame.Content = mainWindow.UsersPage;
            mainWindow.UsersPage.LoadUsers();
            Clear();
        }

        private void Clear()
        {
            Email.Text = "";
            Password.Password = "";
            FirstName.Text = "";
            LastName.Text = "";
            Role.Text = "";
        }

        private bool AreFieldsValid()
        {
            bool areValid = true;
            if(string.IsNullOrWhiteSpace(Email.Text))
            {
                areValid = false;
                Email.BorderBrush = Brushes.Red;
            }
            else
                Email.BorderBrush = Brushes.Black;

            if (string.IsNullOrWhiteSpace(Password.Password))
            {
                areValid = false;
                Password.BorderBrush = Brushes.Red;
            }
            else
                Password.BorderBrush = Brushes.Black;

            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                areValid = false;
                FirstName.BorderBrush = Brushes.Red;
            }
            else
                FirstName.BorderBrush = Brushes.Black;

            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                areValid = false;
                LastName.BorderBrush = Brushes.Red;
            }
            else
                LastName.BorderBrush = Brushes.Black;

            if (string.IsNullOrWhiteSpace(Role.Text))
            {
                areValid = false;
                LastName.BorderBrush = Brushes.Red;
            }
            else
                LastName.BorderBrush = Brushes.Black;

            return areValid;
        }
    }
}
