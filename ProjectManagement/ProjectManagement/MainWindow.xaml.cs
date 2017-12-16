using Infrastructure.Message;
using System.Windows;

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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var commandQueryDispatcher = new CommandQueryDispatcher();
            
        }
    }
}
