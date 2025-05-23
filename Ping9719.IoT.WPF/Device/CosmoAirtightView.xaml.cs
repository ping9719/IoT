using Ping9719.IoT.Device.Airtight;
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

namespace Ping9719.IoT.WPF
{
    public partial class CosmoAirtightView : UserControl
    {
        public CosmoAirtightView()
        {
            InitializeComponent();
        }

        public CosmoAirtight DeviceData
        {
            get { return (CosmoAirtight)GetValue(DeviceDataProperty); }
            set { SetValue(DeviceDataProperty, value); }
        }

        public static readonly DependencyProperty DeviceDataProperty =
            DependencyProperty.Register("DeviceData", typeof(CosmoAirtight), typeof(CosmoAirtightView), new PropertyMetadata(null));

        private void szpb(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            int.TryParse(textBoxMbName.Text, out int pd);
            var bbb = DeviceData.SetChannel(pd);
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"成功设置为{pd}\r\n");
        }

        private void dqsj(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var bbb = DeviceData.ReadTestData();
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.Value}；{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"{bbb.Value}\r\n");
        }

        private void ksky(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var bbb = DeviceData.Start();
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"失败；{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"成功\r\n");
        }

        private void cxzt(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var bbb = DeviceData.Stop();
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"失败；{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"成功\r\n");
        }
    }
}
