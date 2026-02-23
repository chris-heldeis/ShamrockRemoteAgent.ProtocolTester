using ShamrockRemoteAgent.ClientTester.ViewModels;
using ShamrockRemoteAgent.TCPProtocol.Enums.Payloads.LoginReq;
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (serviceTypeSelect.SelectedItem == null)
                return;

            // If ServiceType is enum
            if (serviceTypeSelect != null && OtherServiceText != null)
            {
                if (serviceTypeSelect.SelectedItem.ToString() == "OTHER_SERVICE")
                {
                    OtherServiceText.IsEnabled = true;
                    OtherServiceText.Focus();
                }
                else
                {
                    OtherServiceText!.IsEnabled = false;
                    OtherServiceText!.Text = string.Empty; // optional clear
                }
            }
        }
    }
}
