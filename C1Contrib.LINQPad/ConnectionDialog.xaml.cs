using System.Windows;

using LINQPad.Extensibility.DataContext;

namespace C1Contrib.LINQPad
{
    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        ConnectionProperties _properties;

        public ConnectionDialog(IConnectionInfo cxInfo)
        {
            DataContext = _properties = new ConnectionProperties(cxInfo);
            Background = SystemColors.ControlBrush;
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
