using System.Windows;
using Edomozh.Clock.Resources;

namespace Edomozh.Clock.Forms;

public partial class ConfirmForm : Window
{
    public ConfirmForm(string title, string message)
    {
        InitializeComponent();
        
        Title = title;
        MessageTextBlock.Text = message;
        Icon = AppResources.ClockIconImageSource;
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
