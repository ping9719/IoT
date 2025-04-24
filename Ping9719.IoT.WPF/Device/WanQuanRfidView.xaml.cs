using Ping9719.IoT.Device.Rfid;
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
    /// <summary>
    /// WanQuanRfidView.xaml 的交互逻辑
    /// </summary>
    public partial class WanQuanRfidView : UserControl
    {
        public WanQuanRfidView()
        {
            InitializeComponent();
            comboBox2.ItemsSource = new int[] { 1, 2, 3, 4 };
        }

        public WanQuanRfid DeviceData
        {
            get { return (WanQuanRfid)GetValue(DeviceDataProperty); }
            set { SetValue(DeviceDataProperty, value); }
        }

        public static readonly DependencyProperty DeviceDataProperty =
            DependencyProperty.Register("DeviceData", typeof(WanQuanRfid), typeof(WanQuanRfidView), new PropertyMetadata(null));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var result = DeviceData.ReadString((int)comboBox2.SelectedItem);
            //if (result != null && result.IsSucceed)
            //{
            //    textBox2.Text = textBox2.Text + "\r\n" + result.Value;
            //}
            //else if (result != null && result.Error.Count > 0)
            //{
            //    textBox2.Text = textBox2.Text + "\r\n" + result.ErrorText;
            //}
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            var str = textBox1.Text.Trim();
            var result = DeviceData.Write(str, (int)comboBox2.SelectedItem);
            if (result != null && !string.IsNullOrEmpty(result.ErrorText))
            {
                MessageBox.Show(result.ErrorText);
            }
            else if (result != null && result.IsSucceed)
            {
                MessageBox.Show("写入成功");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            textBox2.Text = string.Empty;
        }
    }
}
