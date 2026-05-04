using PersonalFinanceTracker.ViewModel;
using System.Windows;

namespace PersonalFinanceTracker.View
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                await viewModel.LoginAsync(PasswordBox.Password);
            }
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.ShowRegister();
            }
        }
    }
}