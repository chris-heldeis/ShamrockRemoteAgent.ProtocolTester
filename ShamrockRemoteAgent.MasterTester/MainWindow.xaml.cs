using ShamrockRemoteAgent.MasterTester.Services;
using ShamrockRemoteAgent.MasterTester.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.MasterTester
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
        private void SidebarList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SidebarList.SelectedItem is not ListViewItem item)
                return;

            if (item.Tag is not string key)
                return;

            switch (item.Tag.ToString())
            {
                case "Ping":
                    MainContent.Content = new PingRequestView();
                    break;

                case "Client Connect":
                    MainContent.Content = new ClientConnectRequestView();
                    break;
                case "Client Disconnect":
                    MainContent.Content = new ClientDisconnectRequestView();
                    break;
                case "Send Message":
                    MainContent.Content = new SendMessageRequestView();
                    break;
                case "Read Message":
                    MainContent.Content = new ReadMessageRequestView();
                    break;
                case "Send Command":
                    MainContent.Content = new SendCommandRequestView();
                    break;
                case "ReadVersion":
                    MainContent.Content = new ReadVersionRequestView();
                    break;
                case "GetErrorMsg":
                    MainContent.Content = new GetErrorMsgRequestView();
                    break;
                case "Get Hardware Status":
                    MainContent.Content = new GetHardwareStatusRequestView();
                    break;
                case "Get Last Error Msg":
                    MainContent.Content = new GetLastErrorMsgRequestView();
                    break;
                case "Read Detailed Version":
                    MainContent.Content = new ReadDetailedVersionRequestView();
                    break;
                case "Close":
                    MainContent.Content = new CloseRequestView();
                    break;
            }
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
        private async void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!App.BrokerSocket.IsConnected)
                {
                    await App.BrokerSocket.ConnectAsync(
                        App.BrokerHost,
                        App.BrokerPort,
                        App.MasterId);
                }
            }
            catch (Exception ex)
            {
                PacketBus.PublishLog($"Auto-connect failed: {ex.Message}");
            }
        }

        private async void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                await App.BrokerSocket.CloseAsync();
            }
            catch
            {
                // ignore shutdown errors
            }
        }
    }
}