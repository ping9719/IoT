using Ping9719.IoT.Device.Rfid;
using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Shapes;


namespace Ping9719.IoT.WPF
{
    public partial class TaiHeSenRfidView : UserControl
    {
        public TaiHeSenRfidView()
        {
            InitializeComponent();
        }

        
        public TaiHeSenRfid DeviceData
        {
            get { return (TaiHeSenRfid)GetValue(DeviceDataProperty); }
            set { SetValue(DeviceDataProperty, value); }
        }

        
        public static readonly DependencyProperty DeviceDataProperty =
            DependencyProperty.Register("DeviceData", typeof(TaiHeSenRfid), typeof(TaiHeSenRfidView), new PropertyMetadata(null));
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBox2.AppendText($"没有初始化设备\r\n");
                return;
            }

            try
            {
                var re = DeviceData.Read<byte[]>();
                if (re.IsSucceed)
                {
                    var ddd = MkyRfid.ReadAna(re.Value);
                    textBox2.AppendText($"{ddd}\r\n");
                }
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
                var ddd = MkyRfid.WriteAna(textBox1.Text);
                var re = DeviceData.Write(ddd);
                if (re.IsSucceed)
                    textBox2.AppendText($"写入成功\r\n");
                else
                    textBox2.AppendText($"{re.ErrorText}\r\n");
            }
            catch (Exception ex)
            {
                textBox2.AppendText($"{ex.Message}\r\n");
            }

        }

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            textBox2.Text=string.Empty;
        }
    }

    class MkyRfid
    {
        public static string ReadAna(byte[] bytes)
        {
            if (bytes == null)
                return null;

            return bytes.ByteArrayToString().Replace(" ", "").TrimStart('0').PadLeft(5, '0');
        }

        public static byte[] WriteAna(string bytes)
        {
            return bytes.Replace(" ", "").TrimStart('0').PadLeft(8, '0').StringToByteArray(false);
        }
    }
}
