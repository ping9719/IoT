using Ping9719.IoT.Common;
using Ping9719.IoT.Device.Rfid;
using Ping9719.IoT.Enums;
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
    public partial class RfidView : UserControl
    {
        public RfidView()
        {
            InitializeComponent();
        }

        public IIoT DeviceData
        {
            get { return (IIoT)GetValue(DeviceDataProperty); }
            set { SetValue(DeviceDataProperty, value); }
        }

        public static readonly DependencyProperty DeviceDataProperty =
            DependencyProperty.Register("DeviceData", typeof(IIoT), typeof(RfidView), new PropertyMetadata(null));

        /// <summary>
        /// 区域
        /// </summary>
        public RfidArea Area
        {
            get { return (RfidArea)GetValue(AreaProperty); }
            set { SetValue(AreaProperty, value); }
        }

        public static readonly DependencyProperty AreaProperty =
            DependencyProperty.Register("Area", typeof(RfidArea), typeof(RfidView), new PropertyMetadata(RfidArea.EPC, (a, b) =>
            {
                if (a is RfidView view)
                {
                    if (b.NewValue is RfidArea val1)
                    {
                        view.qy.SelectedIndex = (int)val1;
                    }
                }
            }));


        /// <summary>
        /// 天线
        /// </summary>
        public int Anten
        {
            get { return (int)GetValue(AntenProperty); }
            set { SetValue(AntenProperty, value); }
        }

        public static readonly DependencyProperty AntenProperty =
            DependencyProperty.Register("Anten", typeof(int), typeof(RfidView), new PropertyMetadata(1, (a, b) =>
            {
                if (a is RfidView view)
                {
                    if (b.NewValue is int val1)
                    {
                        view.tx.Text = val1.ToString();
                    }
                }
            }));


        /// <summary>
        /// 密码
        /// </summary>
        public string Pass
        {
            get { return (string)GetValue(PassProperty); }
            set { SetValue(PassProperty, value); }
        }

        public static readonly DependencyProperty PassProperty =
            DependencyProperty.Register("Pass", typeof(string), typeof(RfidView), new PropertyMetadata("00000000", (a, b) =>
            {
                if (a is RfidView view)
                {
                    if (b.NewValue is string val1)
                    {
                        view.mm.Text = val1.ToString();
                    }
                }
            }));

        /// <summary>
        /// 编码
        /// </summary>
        public EncodingEnum Encoding
        {
            get { return (EncodingEnum)GetValue(EncodingProperty); }
            set { SetValue(EncodingProperty, value); }
        }

        public static readonly DependencyProperty EncodingProperty =
            DependencyProperty.Register("Encoding", typeof(EncodingEnum), typeof(RfidView), new PropertyMetadata(EncodingEnum.ASCII, (a, b) =>
            {
                if (a is RfidView view)
                {
                    if (b.NewValue is EncodingEnum val1)
                    {
                        view.qy.SelectedIndex = (int)val1;
                    }
                }
            }));

        /// <summary>
        /// 读取长度
        /// </summary>
        public int ReadCount
        {
            get { return (int)GetValue(ReadCountProperty); }
            set { SetValue(ReadCountProperty, value); }
        }

        public static readonly DependencyProperty ReadCountProperty =
            DependencyProperty.Register("ReadCount", typeof(int), typeof(RfidView), new PropertyMetadata(4, (a, b) =>
            {
                if (a is RfidView view)
                {
                    if (b.NewValue is int val1)
                    {
                        view.dc.Text = val1.ToString();
                    }
                }
            }));


        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                info.AppendText($"没有初始化设备\r\n");
                return;
            }

            try
            {
                var aaa = RfidAddress.GetRfidAddressStr(Area, Pass.StringToByteArray(), Anten);
                var re = DeviceData.ReadString(aaa, ReadCount, Encoding.GetEncoding());
                if (re.IsSucceed)
                    info.AppendText($"{re.Value}\r\n");
                else
                    info.AppendText($"{re.ErrorText}\r\n");
            }
            catch (Exception ex)
            {
                info.AppendText($"{ex.Message}\r\n");
            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                info.AppendText($"没有初始化设备\r\n");
                return;
            }

            try
            {
                var aaa = RfidAddress.GetRfidAddressStr(Area, Pass.StringToByteArray(), Anten);
                var re = DeviceData.WriteString(aaa, xr.Text, ReadCount, Encoding.GetEncoding());
                if (re.IsSucceed)
                    info.AppendText($"成功\r\n");
                else
                    info.AppendText($"{re.ErrorText}\r\n");
            }
            catch (Exception ex)
            {
                info.AppendText($"{ex.Message}\r\n");
            }
        }
    }
}
