using System.Windows;

namespace ShamrockRemoteAgent.ClientTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        protected override async void OnClosed(EventArgs e)
        {

            try
            {
                await App.BrokerSocket.CloseAsync();
            }
            catch
            {
                // swallow — app is exiting anyway
            }
            base.OnClosed(e);
        }
    }
}