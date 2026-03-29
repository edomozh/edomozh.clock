using System.Windows;
using Edomozh.Clock.Helpers;

namespace Edomozh.Clock.Forms;

public partial class ConfirmForm : Window
{
    public ConfirmForm(string title, string message)
    {
        InitializeComponent();
        
        Title = title;
        MessageTextBlock.Text = message;
        Icon = IconHelper.CreateClockIconSource();
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
