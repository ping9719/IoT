using Ping9719.IoT.Device.Mark;
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
    public partial class HuaPuMarkView : UserControl
    {
        public HuaPuMarkView()
        {
            InitializeComponent();
        }

        public HuaPuMark DeviceData
        {
            get { return (HuaPuMark)GetValue(DeviceDataProperty); }
            set { SetValue(DeviceDataProperty, value); }
        }

        public static readonly DependencyProperty DeviceDataProperty =
            DependencyProperty.Register("DeviceData", typeof(HuaPuMark), typeof(HuaPuMarkView), new PropertyMetadata(null));

        private void jzmb(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var bbb = DeviceData.Initialize(textBoxMbName.Text);
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"成功加载模板");
        }

        private void ksth(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var bbb = DeviceData.Data(textBoxKey.Text, textBoxName.Text);
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"替换成功\r\n");
        }

        private void ksky(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var bbb = DeviceData.MarkStart(checkBox1.IsChecked ?? false);
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"打印完成，时间{bbb.TimeConsuming ?? 0}秒\r\n");
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
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"停止成功\r\n");
        }
    }
}
