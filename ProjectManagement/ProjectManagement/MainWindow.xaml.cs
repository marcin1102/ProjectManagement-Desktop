using Newtonsoft.Json.Linq;
using ProjectManagement.Infrastructure.Http;
using ProjectManagement.Infrastructure.Message;
using ProjectManagement.Infrastructure.String;
using ProjectManagement.Infrastructure.UserSettings;
using ProjectManagement.Projects;
using System;
using System.Linq;
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var commandQueryDispatcher = new CommandQueryDispatcher();
            var login = new Login(LoginTextBox.Text, PasswordTextBox.Text);

            var response = await commandQueryDispatcher.SendAsync(login, "api/user-management/users/login", HttpOperationType.POST);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                await SetUpCurrentUser(commandQueryDispatcher, LoginTextBox.Text);
                var projectsWindow = new ProjectsWindow();
                projectsWindow.Show();
                Close();
            }
            else
            {
                //var responseContent = response.ResponseContent.Remove(0).Remove(response.ResponseContent.Count() - 2).RemoveBackslashesFromJson();
                try
                {
                    var jArray = JArray.Parse(response.ResponseContent);
                    MessageBox.Show($"Login failed! \n {string.Join("\n", jArray.Children()["errorMessage"].Values<string>())}");
                }
                catch (System.Exception ex)
                {
                    var jsonContent = JObject.Parse(response.ResponseContent);
                    var properties = jsonContent.Properties().Where(x => x.Name == "errorMessage");
                    if (properties.Any())
                        MessageBox.Show($"Login failed! \n {string.Join("\n", properties.Select(x => x.Value))}");
                    else
                        MessageBox.Show($"Login failed! \n {string.Join("\n", jsonContent.Properties().Select(x => x.Value))}");
                }                
            }
        }

        private async Task SetUpCurrentUser(CommandQueryDispatcher commandQueryDispatcher, string email)
        {
            var userResponse = await commandQueryDispatcher.SendAsync<UserResponse>($"api/user-management/users/{email}");
            CurrentUser.Id = userResponse.ResponseContent.Id;
            CurrentUser.Email = userResponse.ResponseContent.Email;
            CurrentUser.Type = (UserType)Enum.Parse(typeof(UserType), userResponse.ResponseContent.Role);
        }
    }
}
