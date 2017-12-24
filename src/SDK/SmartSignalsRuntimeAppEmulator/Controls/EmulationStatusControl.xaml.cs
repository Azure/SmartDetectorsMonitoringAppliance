//-----------------------------------------------------------------------
// <copyright file="EmulationStatusControl.xaml.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

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
    /// Interaction logic for EmulationStatusControl.xaml
    /// </summary>
    public partial class EmulationStatusControl : UserControl
    {
        public EmulationStatusControl()
        {
            InitializeComponent();

            // Temporary code until we have a proper view model
            this.TracerBox.AppendText("[12/1/2017 9:46:12] Lorem ipsum dolor sit amet…\r\n");
            this.TracerBox.AppendText("[12/1/2017 9:46:14] consectetur adipiscing elit. Aenean sit amet libero vitae lectus porta blandit et vel velit. \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:46:19] Nunc aliquam nulla quis vehicula convallis. Proin mollis commodo scelerisque\r\n");
            this.TracerBox.AppendText("[12/1/2017 9:47:05] Sed ut dolor nibh. Aenean bibendum vel odio eget euismod. Maecenas at nisi sit amet \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:47:12] nunc mollis pulvinar sed eu est. Aliquam sit amet quam sem. Phasellus eget nisl eleifend, \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:47:22] fringilla tortor convallis, efficitur nibh. Proin hendrerit iaculis lacus, aliquam luctus nisl \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:48:12] ultrices a. Maecenas quis dapibus mauris. Phasellus faucibus orci sit amet diam interdum, \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:48:47] sed cursus sem posuere. In sit amet tempus leo. Proin ac velit eu enim gravida ornare \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:49:41] non eget ipsum. Duis faucibus dignissim pharetra.\r\n");
            this.TracerBox.AppendText("[12/1/2017 9:49:55] Maecenas gravida velit nisi, et posuere quam condimentum vitae. Class aptent taciti \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:50:12] sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nulla \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:50:26] consectetur leo eget mauris pulvinar, a tincidunt est sollicitudin. Vestibulum sit amet dui \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:50:32] aliquet neque porttitor pretium. Orci varius natoque penatibus et magnis dis parturient \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:51:49] montes, nascetur ridiculus mus. Sed mattis turpis id ex vulputate sodales. Vivamus \r\n");
            this.TracerBox.AppendText("[12/1/2017 9:52:55] pulvinar ipsum ut ligula molestie volutpat.\r\n");
        }
    }
}
