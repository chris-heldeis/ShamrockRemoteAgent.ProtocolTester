
using ShamrockRemoteAgent.ClientTester.Services;
using System.Windows;

namespace ShamrockRemoteAgent.ClientTester
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string ClientId { get; } =
            $"Client-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";

        // Global WS config
        public static string BrokerHost { get; set; } = "localhost";
        public static int BrokerPort { get; set; } = 8080;
        public static int BrokerPort { get; set; } = 3000;

        public static BrokerWebSocketService BrokerSocket { get; }
            = new BrokerWebSocketService();
    }

}
