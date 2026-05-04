using System.Windows;

namespace PersonalFinanceTracker.View.Dialog
{
    public partial class EditBudgetDialog : Window
    {
        public EditBudgetDialog(decimal currentBudget)
        {
            InitializeComponent();
            BudgetBox.Text = currentBudget.ToString("F2");
        }

        public decimal Budget { get; private set; }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(BudgetBox.Text, out decimal budget) || budget < 0)
            {
                MessageBox.Show("Please enter a valid budget.");
                return;
            }

            Budget = budget;
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