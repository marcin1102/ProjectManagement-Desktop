using ProjectManagement.Infrastructure.Http;
using ProjectManagement.Infrastructure.Message;
using ProjectManagement.Infrastructure.UserSettings;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using UserManagement.Contracts.User.Commands;
using UserManagement.Contracts.User.Queries;

namespace ProjectManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private async void Login()
        {
            var commandQueryDispatcher = new CommandQueryDispatcher();
            var login = new Login(LoginTextBox.Text, PasswordTextBox.Password);

            var response = await commandQueryDispatcher.SendAsync(login, "api/user-management/users/login", HttpOperationType.POST);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                await SetUpCurrentUser(commandQueryDispatcher, LoginTextBox.Text);
                var projectsWindow = new MainWindow();
                projectsWindow.Top = this.Top;
                projectsWindow.Left = this.Left;
                projectsWindow.Show();
                Close();
            }
            else
            {
                ResponseExtensions.ToMessageBox(response.ResponseContent);
            }
        }
        private async Task SetUpCurrentUser(CommandQueryDispatcher commandQueryDispatcher, string email)
        {
            var response = await commandQueryDispatcher.SendAsync<UserResponse>($"api/user-management/users/{email}");
            var user = response.ResponseContent;
            CurrentUser.Id = user.Id;
            CurrentUser.Email = user.Email;
            CurrentUser.FullName = $"{user.FirstName} {user.LastName}";
            CurrentUser.Type = (UserType)Enum.Parse(typeof(UserType), user.Role);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
                Login();
        }
    }
}
