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
    public partial class DaZhuMarkView : UserControl
    {
        public DaZhuMarkView()
        {
            InitializeComponent();
        }

        public DaZhuMark DeviceData
        {
            get { return (DaZhuMark)GetValue(DeviceDataProperty); }
            set { SetValue(DeviceDataProperty, value); }
        }

        public static readonly DependencyProperty DeviceDataProperty =
            DependencyProperty.Register("DeviceData", typeof(DaZhuMark), typeof(DaZhuMarkView), new PropertyMetadata(null));

        private void jzk(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var aa = DeviceData.GetCard();
            if (!aa.IsSucceed)
            {
                textBoxInfo.AppendText($"{aa.ErrorText}\r\n");
                return;
            }

            stackPanel.Children.Clear();
            foreach (var item in aa.Value)
            {
                stackPanel.Children.Add(new CheckBox()
                {
                    Content = $"卡{item}",
                    Tag = item,
                    Margin = new Thickness(0, 0, 5, 0),
                });
            }
            textBoxInfo.AppendText($"加载成功\r\n");
        }

        private void jzmb(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var aaa = GetSelect();
            if (aaa.Length != 1)
            {
                textBoxInfo.AppendText("请选择一个卡\r\n");
                return;
            }
            var bbb = DeviceData.Initialize(textBoxMbName.Text, aaa[0], checkBox1.IsChecked == true);
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"成功加载模板，模板中有{bbb.Value}个可替换文本\r\n");
        }

        private void ksth(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var aaa = GetSelect();
            if (aaa.Length != 1)
            {
                textBoxInfo.AppendText("请选择一个卡\r\n");
                return;
            }
            var bbb = DeviceData.Data(textBoxKey.Text, textBoxName.Text, aaa[0]);
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

            var aaa = GetSelect();
            if (aaa.Length == 0)
            {
                textBoxInfo.AppendText("请选择至少一个卡\r\n");
                return;
            }
            var bbb = DeviceData.MarkStart(60000, aaa);
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"打印完成，时间{bbb.Value}秒\r\n");
        }

        private void kshg(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var aaa = GetSelect();
            if (aaa.Length == 0)
            {
                textBoxInfo.AppendText("请选择至少一个卡\r\n");
                return;
            }
            var bbb = DeviceData.RedStart(60000, aaa);
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"红光完成，时间{bbb.Value}秒\r\n");
        }

        private void cxzt(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var aaa = GetSelect();
            if (aaa.Length != 1)
            {
                textBoxInfo.AppendText("请选择一个卡\r\n");
                return;
            }
            var bbb = DeviceData.State(aaa[0]);
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"{bbb.Value}\r\n");
        }

        private void tzsy(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var bbb = DeviceData.StopAll();
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"已停止所有\r\n");
        }

        private void cxzt2(object sender, RoutedEventArgs e)
        {
            if (DeviceData == null)
            {
                textBoxInfo.AppendText("没有初始化设备\r\n");
                return;
            }

            var bbb = DeviceData.State();
            if (!bbb.IsSucceed)
            {
                textBoxInfo.AppendText($"{bbb.ErrorText}\r\n");
                return;
            }

            textBoxInfo.AppendText($"{bbb.Value}\r\n");
        }

        public string[] GetSelect()
        {
            List<string> list = new List<string>();
            foreach (var item in stackPanel.Children)
            {
                if (item is CheckBox cb)
                {
                    if (cb.IsChecked == true)
                    {
                        list.Add(cb.Tag?.ToString() ?? "");
                    }
                }
            }
            return list.ToArray();
        }
    }
}
