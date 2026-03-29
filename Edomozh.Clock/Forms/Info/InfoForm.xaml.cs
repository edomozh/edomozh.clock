using System.Windows;
using Edomozh.Clock.Helpers;

namespace Edomozh.Clock.Forms;

public partial class InfoForm : Window
{
    public InfoForm(string title, string message)
    {
        InitializeComponent();
        
        Title = title;
        MessageTextBlock.Text = message;
        Icon = IconHelper.CreateClockIconSource();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
