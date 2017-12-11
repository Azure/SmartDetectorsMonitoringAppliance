using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Controls
{
    /// <summary>
    /// Interaction logic for SignalsControl.xaml
    /// </summary>
    public partial class SignalsControl : UserControl
    {
        public SignalsControl()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.BeforeStartMessage.Visibility = Visibility.Collapsed;
            this.EmulationStatusControl.Visibility = Visibility.Visible;
        }
    }
}
