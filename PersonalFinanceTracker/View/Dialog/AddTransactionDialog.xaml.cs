using PersonalFinanceTracker.Model;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PersonalFinanceTracker.View.Dialog
{
    public partial class AddTransactionDialog : Window
    {
        public AddTransactionDialog()
        {
            InitializeComponent();

            DatePicker.SelectedDate = DateTime.Today;

            TypeBox.ItemsSource = new List<DropdownOption<CashTransactionType>>
            {
                new("Income", CashTransactionType.Income),
                new("Expense", CashTransactionType.Expense),
                new("Transfer-In", CashTransactionType.TransferIn),
                new("Transfer-Out", CashTransactionType.TransferOut),
                new("Investment-In", CashTransactionType.InvestmentIn),
                new("Investment-Out", CashTransactionType.InvestmentOut),
                new("Dividend", CashTransactionType.Dividend),
                new("Refund", CashTransactionType.Refund),
                new("Adjustment", CashTransactionType.Adjustment)
            };

            CategoryBox.ItemsSource = new List<DropdownOption<TransactionCategory>>
            {
                new("Rent", TransactionCategory.Rent),
                new("Utilities", TransactionCategory.Utilities),
                new("Groceries", TransactionCategory.Groceries),
                new("Transportation", TransactionCategory.Transportation),
                new("Insurance", TransactionCategory.Insurance),
                new("Food", TransactionCategory.FoodItem),
                new("Entertainment", TransactionCategory.Entertainment),
                new("Shopping", TransactionCategory.Shopping),
                new("Travel", TransactionCategory.Travel),
                new("Salary", TransactionCategory.Salary),
                new("Freelance", TransactionCategory.Freelance),
                new("Bonus", TransactionCategory.Bonus),
                new("Savings", TransactionCategory.Savings),
                new("Investment", TransactionCategory.Investment),
                new("Dividend", TransactionCategory.Dividend),
                new("Transfer", TransactionCategory.Transfer),
                new("Refund", TransactionCategory.Refund),
                new("Other", TransactionCategory.Other)
            };

            TypeBox.DisplayMemberPath = nameof(DropdownOption<CashTransactionType>.DisplayName);
            CategoryBox.DisplayMemberPath = nameof(DropdownOption<TransactionCategory>.DisplayName);

            TypeBox.SelectedIndex = 1;
            CategoryBox.SelectedIndex = 0;
        }

        public string TransactionName { get; private set; } = string.Empty;

        public decimal Amount { get; private set; }

        public DateTime Date { get; private set; }

        public CashTransactionType Type { get; private set; }

        public TransactionCategory Category { get; private set; }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Please enter a transaction name.");
                return;
            }

            if (!decimal.TryParse(AmountBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount.");
                return;
            }

            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.");
                return;
            }

            if (TypeBox.SelectedItem is not DropdownOption<CashTransactionType> selectedType)
            {
                MessageBox.Show("Please select a transaction type.");
                return;
            }

            if (CategoryBox.SelectedItem is not DropdownOption<TransactionCategory> selectedCategory)
            {
                MessageBox.Show("Please select a transaction category.");
                return;
            }

            TransactionName = NameBox.Text.Trim();
            Amount = amount;
            Date = DatePicker.SelectedDate.Value;
            Type = selectedType.Value;
            Category = selectedCategory.Value;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class DropdownOption<T>
    {
        public DropdownOption(string displayName, T value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public string DisplayName { get; }

        public T Value { get; }
    }
}