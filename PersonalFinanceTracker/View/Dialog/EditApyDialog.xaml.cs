using System.Windows;

namespace PersonalFinanceTracker.View.Dialog
{
    public partial class EditApyDialog : Window
    {
        public EditApyDialog(double currentApy)
        {
            InitializeComponent();
            ApyBox.Text = currentApy.ToString("F2");
        }

        public double Apy { get; private set; }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(ApyBox.Text, out double apy) || apy < 0)
            {
                MessageBox.Show("Please enter a valid APY.");
                return;
            }

            Apy = apy;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}