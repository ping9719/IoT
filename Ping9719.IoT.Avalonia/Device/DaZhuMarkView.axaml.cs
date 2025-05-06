using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ping9719.IoT.Device.Mark;

namespace Ping9719.IoT.Avalonia;

public partial class DaZhuMarkView : UserControl
{
    public DaZhuMarkView()
    {
        InitializeComponent();
        Button button = new Button();
        button.Click += Button_Click;
    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    public DaZhuMark DeviceData
    {
        get => GetValue(DeviceDataProperty);
        set => SetValue(DeviceDataProperty, value);
    }

    public static readonly StyledProperty<DaZhuMark> DeviceDataProperty =
        AvaloniaProperty.Register<DaZhuMarkView, DaZhuMark>(nameof(DeviceData), null);

    private void jzk(object sender, RoutedEventArgs e)
    {
        if (DeviceData == null)
        {
            textBoxInfo.Text += ("û�г�ʼ���豸\r\n");
            return;
        }

        var aa = DeviceData.GetCard();
        if (!aa.IsSucceed)
        {
            textBoxInfo.Text += ($"{aa.ErrorText}\r\n");
            return;
        }

        stackPanel.Children.Clear();
        foreach (var item in aa.Value)
        {
            stackPanel.Children.Add(new CheckBox()
            {
                Content = $"��{item}",
                Tag = item,
                Margin = new Thickness(0, 0, 5, 0),
            });
        }
        textBoxInfo.Text += ($"���سɹ�\r\n");
    }

    private void jzmb(object sender, RoutedEventArgs e)
    {
        if (DeviceData == null)
        {
            textBoxInfo.Text += ("û�г�ʼ���豸\r\n");
            return;
        }

        var aaa = GetSelect();
        if (aaa.Length != 1)
        {
            textBoxInfo.Text += ("��ѡ��һ����\r\n");
            return;
        }
        var bbb = DeviceData.Initialize(textBoxMbName.Text, aaa[0], checkBox1.IsChecked == true);
        if (!bbb.IsSucceed)
        {
            textBoxInfo.Text += ($"{bbb.ErrorText}\r\n");
            return;
        }

        textBoxInfo.Text += ($"�ɹ�����ģ�壬ģ������{bbb.Value}�����滻�ı�\r\n");
    }

    private void ksth(object sender, RoutedEventArgs e)
    {
        if (DeviceData == null)
        {
            textBoxInfo.Text += ("û�г�ʼ���豸\r\n");
            return;
        }

        var aaa = GetSelect();
        if (aaa.Length != 1)
        {
            textBoxInfo.Text += ("��ѡ��һ����\r\n");
            return;
        }
        var bbb = DeviceData.Data(textBoxKey.Text, textBoxName.Text, aaa[0]);
        if (!bbb.IsSucceed)
        {
            textBoxInfo.Text += ($"{bbb.ErrorText}\r\n");
            return;
        }

        textBoxInfo.Text += ($"�滻�ɹ�\r\n");
    }

    private void ksky(object sender, RoutedEventArgs e)
    {
        if (DeviceData == null)
        {
            textBoxInfo.Text += ("û�г�ʼ���豸\r\n");
            return;
        }

        var aaa = GetSelect();
        if (aaa.Length == 0)
        {
            textBoxInfo.Text += ("��ѡ������һ����\r\n");
            return;
        }
        var bbb = DeviceData.MarkStart(aaa);
        if (!bbb.IsSucceed)
        {
            textBoxInfo.Text += ($"{bbb.ErrorText}\r\n");
            return;
        }

        textBoxInfo.Text += ($"��ӡ��ɣ�ʱ��{bbb.Value}��\r\n");
    }

    private void kshg(object sender, RoutedEventArgs e)
    {
        if (DeviceData == null)
        {
            textBoxInfo.Text += ("û�г�ʼ���豸\r\n");
            return;
        }

        var aaa = GetSelect();
        if (aaa.Length == 0)
        {
            textBoxInfo.Text += ("��ѡ������һ����\r\n");
            return;
        }
        var bbb = DeviceData.RedStart(aaa);
        if (!bbb.IsSucceed)
        {
            textBoxInfo.Text += ($"{bbb.ErrorText}\r\n");
            return;
        }

        textBoxInfo.Text += ($"�����ɣ�ʱ��{bbb.Value}��\r\n");
    }

    private void cxzt(object sender, RoutedEventArgs e)
    {
        if (DeviceData == null)
        {
            textBoxInfo.Text += ("û�г�ʼ���豸\r\n");
            return;
        }

        var aaa = GetSelect();
        if (aaa.Length != 1)
        {
            textBoxInfo.Text += ("��ѡ��һ����\r\n");
            return;
        }
        var bbb = DeviceData.State(aaa[0]);
        if (!bbb.IsSucceed)
        {
            textBoxInfo.Text += ($"{bbb.ErrorText}\r\n");
            return;
        }

        textBoxInfo.Text += ($"{bbb.Value}\r\n");
    }

    private void tzsy(object sender, RoutedEventArgs e)
    {
        if (DeviceData == null)
        {
            textBoxInfo.Text += ("û�г�ʼ���豸\r\n");
            return;
        }

        var bbb = DeviceData.StopAll();
        if (!bbb.IsSucceed)
        {
            textBoxInfo.Text += ($"{bbb.ErrorText}\r\n");
            return;
        }

        textBoxInfo.Text += ($"��ֹͣ����\r\n");
    }

    private void cxzt2(object sender, RoutedEventArgs e)
    {
        if (DeviceData == null)
        {
            textBoxInfo.Text += ("û�г�ʼ���豸\r\n");
            return;
        }

        var bbb = DeviceData.State();
        if (!bbb.IsSucceed)
        {
            textBoxInfo.Text += ($"{bbb.ErrorText}\r\n");
            return;
        }

        textBoxInfo.Text+=($"{bbb.Value}\r\n");
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