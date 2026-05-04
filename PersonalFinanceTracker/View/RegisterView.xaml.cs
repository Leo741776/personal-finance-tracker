using PersonalFinanceTracker.ViewModel;
using System.Windows;

namespace PersonalFinanceTracker.View
{
    public partial class RegisterView : Window
    {
        public RegisterView()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                await viewModel.RegisterAsync(
                    PasswordBox.Password,
                    ConfirmPasswordBox.Password);
            }
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.ShowLogin();
            }
        }
    }
}