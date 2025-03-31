using Ping9719.IoT.Communication;
using Ping9719.IoT.Device.Fct;
using Ping9719.IoT.PLC;
using System.Text;

namespace ConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            OmronCipClient omronCip = new OmronCipClient("172.22.124.21", 44818);
            var aaa = omronCip.Client.Open();

            //UdpClient client1 = new UdpClient("127.0.0.1", 8080);

            //client1.ConnectionMode = ConnectionMode.AutoOpen;//断线重连
            //client1.Encoding = Encoding.UTF8;//如何解析字符串
            //client1.TimeOut = 5000;//超时时间
            //client1.Opening = (a) =>
            //{
            //    Console.WriteLine("连接中");
            //    Console.Out.Flush();
            //    return true;
            //};
            //client1.Opened = (a) =>
            //{
            //    Console.WriteLine("连接成功");
            //};
            //client1.Closing = (a) =>
            //{
            //    Console.WriteLine("关闭中");
            //    return true;
            //};
            //client1.Closed = (a, b) =>
            //{
            //    Console.WriteLine("关闭成功" + b);
            //};
            //client1.Received = (a, b) =>
            //{
            //    Console.WriteLine("收到消息:" + a.Encoding.GetString(b));
            //};
            //client1.Warning = (a, b) =>
            //{
            //    Console.WriteLine("错误" + b.ToString());
            //};
            ////打开链接，设置所有属性必须在打开前
            //client1.Open();


            //client1.SendReceive("abc");
            //client1.SendReceive("abc");
            //client1.SendReceive("abc");
            ////client1.Close();
            ////client1.SendReceive("abc");
            ////client1.SendReceive("abc");
            ////client1.SendReceive("abc");
            ////client1.SendReceive("abc");
            ////client1.SendReceive("abc");
            ////client1.SendReceive("abc");
            ////client1.Send("abc");//发送
            ////client1.Receive();//等待并接受
            ////client1.Receive(ReceiveMode.ParseByteAll(6000));//读取所有，超时为6秒 
            ////client1.Receive(ReceiveMode.ParseByte(10, 6000));//读取10个字节，超时为6秒 
            ////client1.Receive(ReceiveMode.ParseToString("\n", 6000));//读取字符串结尾为\n的，超时为6秒 
            ////client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 6000));//发送并读取字符串结尾为\n的，超时为6秒 

            //Console.WriteLine("结束");
            //Console.ReadLine();
            //client1.Close();
            //Console.WriteLine("结束2");
            Console.ReadLine();
        }
    }
}
