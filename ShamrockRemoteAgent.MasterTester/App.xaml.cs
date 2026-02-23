using ShamrockRemoteAgent.MasterTester.Services;
using System.Windows;

namespace ShamrockRemoteAgent.MasterTester
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string MasterId { get; } =
            $"Master-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";

        // Global WS config
        public static string BrokerHost { get; set; } = "localhost";
        public static int BrokerPort { get; set; } = 5000;

        public static BrokerWebSocketService BrokerSocket { get; }
            = new BrokerWebSocketService();

        public static async void CheckConnect()
        {
            if (!BrokerSocket.IsConnected)
            {
                await BrokerSocket.ConnectAsync(
                    BrokerHost,
                    BrokerPort,
                    MasterId);
            }
        }
    }

}
