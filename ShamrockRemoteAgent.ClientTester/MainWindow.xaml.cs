using ShamrockRemoteAgent.ClientTester.Services;
using ShamrockRemoteAgent.ClientTester.Views;
using System.Windows;

namespace ShamrockRemoteAgent.ClientTester
{
    public partial class MainWindow : Window
    {
        private readonly BrokerWebSocketService _brokerService;
        public MainWindow()
        {
            InitializeComponent();

            _brokerService = App.BrokerSocket;
            _brokerService.LoginHandshakeCompleted += OnLoginHandshakeCompleted;
            ShowLogin();
        }

        private void ShowLogin()
        {
            var loginView = new LoginRequestView();

            MainContent.Content = loginView;
        }

        private void OnLoginHandshakeCompleted()
        {
            // This event comes from background thread (ReceiveLoop),
            // so we must switch to UI thread
            Dispatcher.Invoke(() =>
            {
                ShowLogView();
            });
        }

        private void ShowLogView()
        {
            MainContent.Content = new HexViewerView();
        }
    }
}