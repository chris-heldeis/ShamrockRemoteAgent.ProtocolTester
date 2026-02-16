using System.Windows;
using System.Windows.Controls;
using ShamrockRemoteAgent.MasterTester.ViewModels;

namespace ShamrockRemoteAgent.MasterTester.Views;

public partial class ClientConnectRequestView : UserControl
{
    /// <summary>
    /// Interaction logic for ClientConnectRequestView.xaml
    /// </summary>
    public ClientConnectRequestView()
    {
        InitializeComponent();
    }

    private void OnClientConnectRequestClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is ClientConnectRequestViewModel vm)
        {
            if (!vm.Validate(out string error))
            {
                MessageBox.Show(error,
                                "Validation Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            vm.SendClientConnectRequest();
        }
    }
}
