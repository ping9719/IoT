using Ping9719.IoT.Device.Scanner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class ScannerView : UserControl
    {
        public ScannerView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设备数据
        /// </summary>
        public IScannerBase DeviceData
        {
            get { return (IScannerBase)GetValue(DeviceDataProperty); }
            set { SetValue(DeviceDataProperty, value); }
        }

        public static readonly DependencyProperty DeviceDataProperty =
            DependencyProperty.Register("DeviceData", typeof(IScannerBase), typeof(ScannerView), new PropertyMetadata(null));

        private void clickSm(object sender, RoutedEventArgs e)
        {
            try
            {
                var aaa = DeviceData.ReadOne();
                if (aaa.IsSucceed)
                {
                    textBoxInfo.AppendText($"成功：{aaa.Value}\r\n");
                }
                else
                {
                    textBoxInfo.AppendText($"失败：{aaa.ErrorText}\r\n");
                }
            }
            catch (Exception ex)
            {
                textBoxInfo.AppendText($"错误：{ex.Message}\r\n");
            }
        }
    }
}
