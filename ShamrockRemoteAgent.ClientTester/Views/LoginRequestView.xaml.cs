using ShamrockRemoteAgent.ClientTester.ViewModels;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.ClientTester.Views
{
    /// <summary>
    /// Interaction logic for LoginRequestView.xaml
    /// </summary>
    public partial class LoginRequestView : UserControl
    {
        public event Action LoginCompleted;
        public LoginRequestView()
        {
            InitializeComponent();
        }
        private void OnBuildClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LoginRequestViewModel vm)
            {
                vm.BuildPacket();
                LoginCompleted?.Invoke();
            }
        }
    }
}
