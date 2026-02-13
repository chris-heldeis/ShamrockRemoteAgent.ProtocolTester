using ShamrockRemoteAgent.ClientTester.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ShamrockRemoteAgent.ClientTester.Views
{
    /// <summary>
    /// Interaction logic for LoginRequestView.xaml
    /// </summary>
    public partial class LoginRequestView : UserControl
    {
        public LoginRequestView()
        {
            InitializeComponent();
        }
        private void OnBuildClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginRequestViewModel vm)
            {
                if (!vm.Validate(out string error))
                {
                    MessageBox.Show(error,
                                    "Validation Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                vm.SendLoginRequest();
            }
        }
    }
}
