using Ping9719.IoT.Device.Rfid;
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
    public partial class BeiJiaFuRfidView : UserControl
    {
        public BeiJiaFuRfidView()
        {
            InitializeComponent();
        }

        public BeiJiaFuRfid DeviceData
        {
            get { return (BeiJiaFuRfid)GetValue(DeviceDataProperty); }
            set { SetValue(DeviceDataProperty, value); }
        }

        public static readonly DependencyProperty DeviceDataProperty =
            DependencyProperty.Register("DeviceData", typeof(BeiJiaFuRfid), typeof(BeiJiaFuRfidView), new PropertyMetadata(null));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBox2.AppendText($"没有初始化设备\r\n");
                return;
            }

            try
            {
                var re = DeviceData.Read(comboBox1.SelectedIndex);
                if (re.IsSucceed)
                    textBox2.AppendText($"{re.Value}\r\n");
                else
                    textBox2.AppendText($"{re.ErrorText}\r\n");
            }
            catch (Exception ex)
            {
                textBox2.AppendText($"{ex.Message}\r\n");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBox2.AppendText($"没有初始化设备\r\n");
                return;
            }

            try
            {
                var re = DeviceData.Write(textBox1.Text, comboBox1.SelectedIndex);
                if (re.IsSucceed)
                    textBox2.AppendText($"通道[{comboBox1.SelectedIndex + 1}]写入成功\r\n");
                else
                    textBox2.AppendText($"{re.ErrorText}\r\n");
            }
            catch (Exception ex)
            {
                textBox2.AppendText($"{ex.Message}\r\n");
            }
        }
    }
}
