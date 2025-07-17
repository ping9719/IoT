using Ping9719.IoT;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using Ping9719.IoT.Device.Fct;
using Ping9719.IoT.Device.Rfid;
using Ping9719.IoT.Modbus;
using Ping9719.IoT.PLC;
using System.Text;

namespace ConsoleTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // 测试三菱PLC MC协议客户端
            var client = new MitsubishiMcClient(
                MitsubishiVersion.Qna_3E,
                "127.0.0.1",
                6000
            );

            client.Client.Open();

            // 测试short的批量读写
            //TestShortReadWrite(client);

            // 测试bool的读写
            //TestBoolReadWrite(client);

            //TestFloatReadWrite(client);
            //TestDoubleReadWrite(client);

            //TestInt32ReadWrite(client);
            TestStringReadWrite(client);
        }

        /// <summary>
        /// 测试short类型的批量读写
        /// </summary>
        /// <param name="client">三菱PLC客户端</param>
        private static void TestShortReadWrite(MitsubishiMcClient client)
        {
            string address = "D100";
            short[] valuesToWrite = { 1234, 5678, -100, 4321 };
            Console.WriteLine("\n--- 批量写入 short ---");

            // 提取前缀和起始数字
            string prefix = new string(address.TakeWhile(char.IsLetter).ToArray());
            int startNum = int.Parse(new string(address.SkipWhile(char.IsLetter).ToArray()));

            // 真正的批量写入
            var batchWrite = client.Write<short>(address, valuesToWrite);
            Console.WriteLine($"批量写入 {address} 结果: {batchWrite.IsSucceed}");

            // 批量读取
            Console.WriteLine("\n--- 批量读取 short ---");
            var readMulti = client.Read<short>(address, valuesToWrite.Length);
            if (readMulti.IsSucceed)
            {
                int idx = 0;
                for (int i = 0; i < valuesToWrite.Length; i++)
                {
                    string readAddr = prefix + (startNum + i);
                    Console.WriteLine($"{readAddr} 读取值: {readMulti.Value.ElementAt(idx)}");
                    idx++;
                }
            }
            else
            {
                Console.WriteLine($"批量读取失败: {readMulti.Error}");
            }

            var aaaa = client.Write<short>(address, 34);
            var readSingle = client.Read<short>(address);
            Console.WriteLine($" 单个读取值: {readSingle.Value}");
        }

        /// <summary>
        /// 测试bool类型的单点和连续读写
        /// </summary>
        /// <param name="client">三菱PLC客户端</param>
        private static void TestBoolReadWrite(MitsubishiMcClient client)
        {
            string address = "M100";
            Console.WriteLine("\n--- Bool 单点写入 true ---");
            var writeTrue = client.Write<bool>(address, true);
            Console.WriteLine($"写入{address}=true 结果: {writeTrue.IsSucceed}");
            var readTrue = client.Read<bool>(address);
            Console.WriteLine($"读取{address} 结果: {readTrue.IsSucceed}, 值: {readTrue.Value}");

            //Console.WriteLine("\n--- Bool 单点写入 false ---");
            //var writeFalse = client.Write(address, false);
            //Console.WriteLine($"写入{address}=false 结果: {writeFalse.IsSucceed}");
            //var readFalse = client.ReadBoolean(address);
            //Console.WriteLine($"读取{address} 结果: {readFalse.IsSucceed}, 值: {readFalse.Value}");

            //// 连续写入
            string[] boolAddresses = { "M101", "M102", "M103", "M104" };
            bool[] boolValues = { false, true, false, true };
            Console.WriteLine("\n--- 连续写入 bool ---");
            client.Write<bool>("M101", boolValues);

            // 连续读取
            Console.WriteLine("\n--- 连续读取 bool ---");
            var readMulti = client.Read<bool>("M101", 4);
            if (readMulti.IsSucceed)
            {
                foreach (var kv in readMulti.Value)
                {
                    Console.WriteLine($" 值: {kv}");
                }
            }
        }

        /// <summary>
        /// 测试float类型的单个和连续读写
        /// </summary>
        /// <param name="client">三菱PLC客户端</param>
        private static void TestFloatReadWrite(MitsubishiMcClient client)
        {
            string address = "D200";
            float[] valuesToWrite = { 1.23f, 4.56f, -7.89f, 11.11f };
            Console.WriteLine("\n--- 批量写入 float ---");

            // 提取前缀和起始数字
            string prefix = new string(address.TakeWhile(char.IsLetter).ToArray());
            int startNum = int.Parse(new string(address.SkipWhile(char.IsLetter).ToArray()));

            // 真正的批量写入
            var batchWrite = client.Write<float>(address, valuesToWrite);
            Console.WriteLine($"批量写入 {address} 结果: {batchWrite.IsSucceed}");

            // 批量读取
            Console.WriteLine("\n--- 批量读取 float ---");
            var readMulti = client.Read<float>(address, valuesToWrite.Length);
            if (readMulti.IsSucceed)
            {
                int idx = 0;
                for (int i = 0; i < valuesToWrite.Length; i++)
                {
                    string readAddr = prefix + (startNum + i * 2);
                    Console.WriteLine($"{readAddr} 读取值: {readMulti.Value.ElementAt(idx)}");
                    idx++;
                }
            }
            else
            {
                Console.WriteLine($"批量读取失败: {readMulti.Error}");
            }

            // 单个写入和读取
            var singleWrite = client.Write<float>(address, 3.14f);
            Console.WriteLine($"单个写入{address}=3.14 结果: {singleWrite.IsSucceed}");
            var readSingle = client.Read<float>(address);
            Console.WriteLine($"单个读取值: {readSingle.Value}");
        }

        /// <summary>
        /// 测试double类型的单个和连续读写
        /// </summary>
        /// <param name="client">三菱PLC客户端</param>
        private static void TestDoubleReadWrite(MitsubishiMcClient client)
        {
            string address = "D300";
            double[] valuesToWrite = { 123.456, -789.012, 3456.789, 0.00123 };
            Console.WriteLine("\n--- 批量写入 double ---");

            // 提取前缀和起始数字
            string prefix = new string(address.TakeWhile(char.IsLetter).ToArray());
            int startNum = int.Parse(new string(address.SkipWhile(char.IsLetter).ToArray()));

            // 真正的批量写入
            var batchWrite = client.Write<double>(address, valuesToWrite);
            Console.WriteLine($"批量写入 {address} 结果: {batchWrite.IsSucceed}");

            // 批量读取
            Console.WriteLine("\n--- 批量读取 double ---");
            var readMulti = client.Read<double>(address, valuesToWrite.Length);
            if (readMulti.IsSucceed)
            {
                int idx = 0;
                for (int i = 0; i < valuesToWrite.Length; i++)
                {
                    string readAddr = prefix + (startNum + i * 4);
                    Console.WriteLine($"{readAddr} 读取值: {readMulti.Value.ElementAt(idx)}");
                    idx++;
                }
            }
            else
            {
                Console.WriteLine($"批量读取失败: {readMulti.Error}");
            }

            // 单个写入和读取
            var singleWrite = client.Write<double>(address, 3.1415926);
            Console.WriteLine($"单个写入{address}=3.1415926 结果: {singleWrite.IsSucceed}");
            var readSingle = client.Read<double>(address);
            Console.WriteLine($"单个读取值: {readSingle.Value}");
        }

        private static void TestStringReadWrite(MitsubishiMcClient client)
        {
            //下面两种方式写入都可以
            client.Write<string>("D500", "ABCD1234");
            client.WriteString("D500", "124234da", 0, Encoding.ASCII);

            var aaaa = client.ReadString("D500", 8, Encoding.ASCII);

            Console.WriteLine(aaaa.Value);
        }

        private void Test1()
        {
            //        private List<GaleShapleyItem<T>> mans;
            //private List<T> womens;

            //1.参与配对的所有人
            var ms = new string[] { "M0", "M1", "M2", "M3", "M4", };
            var ws = new string[] { "W0", "W1", "W2", "W3", "W4", };
            //2.转为项
            var msi = ms.Select(o => new GaleShapleyItem<string>(o)).ToList();
            var wsi = ms.Select(o => new GaleShapleyItem<string>(o)).ToList();
            //3.配置偏好列表。假如喜欢关系如下：（喜欢程度从高到低）
            //M0❤W1,W0   M1❤W0,W1   M2❤W0,W1,W2,W3,W4   M3❤W3   M4❤W3
            msi[0].Preferences = new List<GaleShapleyItem<string>>() { wsi[1], wsi[0] };
            msi[1].Preferences = new List<GaleShapleyItem<string>>() { wsi[0], wsi[1] };
            msi[2].Preferences = new List<GaleShapleyItem<string>>() { wsi[0], wsi[1], wsi[2], wsi[3], wsi[4] };
            msi[3].Preferences = new List<GaleShapleyItem<string>>() { wsi[3] };
            msi[4].Preferences = new List<GaleShapleyItem<string>>() { wsi[3] };
            //4.开始计算
            GaleShapleyAlgorithm.Run(msi);
            //5.打印结果
            foreach (var item in msi)
            {
                //M0❤M1   M1❤M0   M2❤M2   M3❤M3   M4❤null
                Console.Write($"{item.Item}❤{(item.Match?.Item) ?? "null"}   ");
            }

            //DCBA:0100
            var aaaa = BitConverter.GetBytes((long)1161981756646125696);
            //var asdasd = ModbusInfo.AddressAnalysis("100", 1);
            //var asda1 = asdasd.Value.GetModbusTcpCommand<UInt16>(0, new UInt16[] { 2,666 }, 2, null);

            ModbusTcpClient client = new ModbusTcpClient("127.0.0.1", 502, format: EndianFormat.ABCD);
            client.Client.Open();
            client.Read<Int16>("100");//读寄存器
            client.Read<Int16>("100.1");//读寄存器中的位，读位只支持单个读，最好是uint16,int16
            client.Read<Int16>("s=2;x=3;100");//读寄存器，对应站号，功能码，地址
            client.Read<bool>("100");//读线圈
            client.Read<bool>("100", 10);//读多个线圈
            client.Write<Int16>("100", 100);//写寄存器
            client.Write<Int16>("100", 100, 110);//写多个寄存器
            client.ReadString("500", 5, Encoding.ASCII);//读字符串
            client.ReadString("500", 5, null);//读字符串，以16进制的方式

            var aa1 = client.Write<Int16>("100", 1, 2, 5070, 4128);
            var aa2 = client.Write<Int32>("200", 1, 2, 5070, 270544960);
            var aa3 = client.Write<Int64>("300", 1, 2, 5070, 1161981756646125696);
            var aa4 = client.Write<float>("400", 1f, 2.45f, 5070.45454f, 335282.454f);
            var aa51 = client.WriteString("500", "abcd", 10, Encoding.ASCII);
            var aa5 = client.ReadString("500", 5, Encoding.ASCII);

            var asda = WordHelp.SplitBlock<int>(Enumerable.Range(1, 0), 3, 2, 0);
            var aaa11 = BitConverter.GetBytes((Int16)1);
            var aaa = DataConvert.StringToByteArray("010203");

            WanQuanRfid wanQuan2Rfid = new WanQuanRfid(WanQuanRfidVer.IR610P_HF, ip: "192.168.0.90", 502);
            //var aaa = wanQuan2Rfid.Client.Open();
            var aaaa1 = wanQuan2Rfid.WriteString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693), "0102");
            var aaaa2 = wanQuan2Rfid.ReadString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693), 4);
            var aaa1122 = 1;
            //OmronCipClient omronCip = new OmronCipClient("172.22.124.21", 44818);
            //var aaa = omronCip.Client.Open();

            //var client1 = new TcpClient("127.0.0.1", 8080);
            // var client1 = new SerialPortClient("COM9", 9600);

            // client1.ConnectionMode = ConnectionMode.AutoOpen;//断线重连
            // client1.Encoding = Encoding.ASCII;//如何解析字符串
            // client1.TimeOut = 5000;//超时时间
            // client1.Opening = (a) =>
            // {
            //     Console.WriteLine("连接中");
            //     Console.Out.Flush();
            //     return true;
            // };
            // client1.Opened = (a) =>
            // {
            //     Console.WriteLine("连接成功");
            // };
            // client1.Closing = (a) =>
            // {
            //     Console.WriteLine("关闭中");
            //     return true;
            // };
            // client1.Closed = (a, b) =>
            // {
            //     Console.WriteLine("关闭成功" + b);
            // };
            // int i = 0;
            // client1.Received = (a, b) =>
            // {
            //     i++;
            //     Console.WriteLine($"收到消息{i}:" + a.Encoding.GetString(b));
            // };
            // //打开链接，设置所有属性必须在打开前
            // client1.Open();

            //var aaa= client1.SendReceive("abc");
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
            ////client1.Receive();//等待并接收
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

        /// <summary>
        /// 测试int32类型的单个和连续读写
        /// </summary>
        /// <param name="client">三菱PLC客户端</param>
        private static void TestInt32ReadWrite(MitsubishiMcClient client)
        {
            string address = "D400";
            int[] valuesToWrite = { 123456, -789012, 3456789, 0 };
            Console.WriteLine("\n--- 批量写入 int32 ---");

            // 提取前缀和起始数字
            string prefix = new string(address.TakeWhile(char.IsLetter).ToArray());
            int startNum = int.Parse(new string(address.SkipWhile(char.IsLetter).ToArray()));

            // 真正的批量写入
            var batchWrite = client.Write<int>(address, valuesToWrite);
            Console.WriteLine($"批量写入 {address} 结果: {batchWrite.IsSucceed}");

            // 批量读取
            Console.WriteLine("\n--- 批量读取 int32 ---");
            var readMulti = client.Read<int>(address, valuesToWrite.Length);
            if (readMulti.IsSucceed)
            {
                int idx = 0;
                for (int i = 0; i < valuesToWrite.Length; i++)
                {
                    string readAddr = prefix + (startNum + i * 2);
                    Console.WriteLine($"{readAddr} 读取值: {readMulti.Value.ElementAt(idx)}");
                    idx++;
                }
            }
            else
            {
                Console.WriteLine($"批量读取失败: {readMulti.Error}");
            }

            // 单个写入和读取
            var singleWrite = client.Write<int>(address, 3141592);
            Console.WriteLine($"单个写入{address}=3141592 结果: {singleWrite.IsSucceed}");
            var readSingle = client.Read<int>(address);
            Console.WriteLine($"单个读取值: {readSingle.Value}");
        }
    }
}