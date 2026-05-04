using System.Windows;

namespace PersonalFinanceTracker.View.Dialog
{
    public partial class DepositDialog : Window
    {
        public DepositDialog()
        {
            InitializeComponent();
        }

        public decimal Amount { get; private set; }

        private void DepositButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount.");
                return;
            }

            Amount = amount;
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