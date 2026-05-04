using System.Windows;

namespace PersonalFinanceTracker.View.Dialog
{
    public partial class AddCashDialog : Window
    {
        public AddCashDialog()
        {
            InitializeComponent();
        }

        public decimal Amount { get; private set; }

        public string Source { get; private set; } = string.Empty;

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount.");
                return;
            }

            Amount = amount;
            Source = string.IsNullOrWhiteSpace(SourceBox.Text)
                ? "Cash Deposit"
                : SourceBox.Text.Trim();

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