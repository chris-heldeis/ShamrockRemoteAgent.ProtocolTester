using System.Windows;
using ShamrockRemoteAgent.ClientTester.Views;

namespace ShamrockRemoteAgent.ClientTester
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ShowLogin();
        }

        private void ShowLogin()
        {
            var loginView = new LoginRequestView();
            loginView.LoginCompleted += OnLoginCompleted;

            MainContent.Content = loginView;
        }

        private void OnLoginCompleted()
        {
            ShowLogView();
        }

        private void ShowLogView()
        {
            MainContent.Content = new HexViewerView();
        }
    }
}