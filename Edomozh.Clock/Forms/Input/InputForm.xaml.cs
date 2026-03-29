using System.Windows;
using Edomozh.Clock.Helpers;

namespace Edomozh.Clock.Forms;

public partial class InputForm : Window
{
    public string InputText => InputTextBox.Text;

    public InputForm(string title, string prompt)
    {
        InitializeComponent();

        Title = title;
        PromptTextBlock.Text = prompt;
        Icon = IconHelper.CreateClockIconSource();

        Loaded += (s, e) => InputTextBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
