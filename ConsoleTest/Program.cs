using Ping9719.IoT;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using Ping9719.IoT.Device.Fct;
using Ping9719.IoT.Device.Rfid;
using Ping9719.IoT.Device.Weld;
using Ping9719.IoT.Hid;
using Ping9719.IoT.Modbus;
using Ping9719.IoT.PLC;
using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HttpClient = Ping9719.IoT.Communication.HttpClient;

namespace ConsoleTest
{
    internal class Program
    {
        public class MyJsonParse : IJsonParse
        {
            public T DeserializeObject<T>(string json) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            public string SerializeObject(object obj) => Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        public class test
        {
            public Int16 aa { get; set; }
            public Int16 bb { get; set; }

            [IoT(IsIgnore = true)]
            public Int16 cc { get; set; }
        }

        private static async Task Main(string[] args)
        {
            JBCWeld jBCWeld = new JBCWeld("COM5");
            jBCWeld.Client.Open();
            while (true)
            {
                var assa = jBCWeld.ReadInfo();
                Console.WriteLine(assa.IsSucceed ? assa.Value : assa.ErrorText);
                Thread.Sleep(200);
            }

            ////var testArr = new byte[] { 205,};
            //var testArr = new byte[] { 0, 1, 0, 2, 0, 3, 0, 4 };
            ////添加自定义转换器（如果需要）
            //Dictionary<Type, IByteConverter> converterDict = new Dictionary<Type, IByteConverter>()
            //{
            //    { typeof(bool), new BoolBitByteConverter()}
            //};

            ////解析单个
            //var int16 = ByteData.GetValues<bool>(testArr,2, EndianFormat.ABCD, converterDict);//1
            //var obj = ByteData.GetValue<test>(testArr, EndianFormat.ABCD, converterDict);//{"aa":1,"bb":2,"cc":0}
            ////解析全部
            //var int16s = ByteData.GetValue<Int16[]>(testArr, EndianFormat.ABCD, converterDict);//[1,2,3,4]
            //var objs = ByteData.GetValue<test[]>(testArr, EndianFormat.ABCD, converterDict);//[{"aa":1,"bb":2,"cc":0},{"aa":3,"bb":4,"cc":0}]
            ////解析指定个数
            //var int16ss = ByteData.GetValues<Int16>(testArr, 2, EndianFormat.ABCD, converterDict);//[1,2]
            //var objss = ByteData.GetValues<test>(testArr, 2, EndianFormat.ABCD, converterDict);//[{"aa":1,"bb":2,"cc":0},{"aa":3,"bb":4,"cc":0}]
            ////反向解析
            //var bs = ByteData.ToBytes(obj, EndianFormat.ABCD);//[0,1,0,2]

            ////特殊情况，1byte=8bool
            //var bools = ByteData.GetValues<bool>(testArr, 1, EndianFormat.ABCD, converterDict);//[F,F,F,F,F,F,F,F]

            Console.ReadKey();
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