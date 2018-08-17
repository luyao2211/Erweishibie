using ImageSample_1;
using SocketSerialTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApplication1.CommonLibrary;

namespace WebApplication1.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*"), WebApiTracker]
    public class ValuesController : ApiController
    {
        static private System.IO.Ports.SerialPort serialPort = new System.IO.Ports.SerialPort();
        static List<string> sl = new List<string>();
        static string received = "";
        static string ErweiDir = "D:\\数据追溯\\";

        [HttpGet]
        [Route("Api/v1/readFromCom")]
        public string readFromCom()
        {
            serialPort = new System.IO.Ports.SerialPort();
            sl = new List<string>();
            received = "";
            init_Serial_List();
            serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPort_DataReceived);
            open_Serial(0);
            while (1 != 0)
            {
                Thread.Sleep(1000);
                if (received != "")
                {
                    string filePath = ErweiDir + received.Split('\r').ToList()[0] + ".txt";
                    if (!System.IO.File.Exists(filePath))
                    {
                        var request = (HttpWebRequest)WebRequest.Create("http://121.43.107.106:8063/erwei/" + received.Split('\r').ToList()[0] + ".txt");
                        var response = (HttpWebResponse)request.GetResponse();
                        close_Serial();
                        return new StreamReader(response.GetResponseStream()).ReadToEnd().Split('\r').ToList()[0];
                    }
                    else
                    {
                        StreamReader sr = new StreamReader(filePath, Encoding.Default);
                        string result = sr.ReadLine();
                        sr.Close();
                        close_Serial();
                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// 获取串口列表
        /// </summary>
        static private void init_Serial_List()
        {
            sl = SerialPortTool.GetSerialPortList();
            if (sl == null)
            {
                Console.WriteLine("读取串口列表失败");
                return;
            }
            return;
        }

        /// <summary>
        /// 读取串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[1024];
            int n = serialPort.Read(buffer, 0, 1024);
            received = System.Text.Encoding.UTF8.GetString(buffer, 0, n);
            return;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="duankouhao"></param>
        /// <returns></returns>
        static private bool open_Serial(int duankouhao)
        {
            if (serialPort.IsOpen)
            {
                return true;
            }
            int baud;
            if (!int.TryParse("9600", out baud))
            {
                return false;
            }
            serialPort.PortName = SerialPortTool.GetSerialPortByName(sl[duankouhao]);
            serialPort.BaudRate = baud;
            try
            {
                serialPort.Open();
            }
            catch (System.IO.IOException ioe)
            {
                Console.WriteLine(ioe.Message);
            }
            catch (System.UnauthorizedAccessException ioe)
            {
                Console.WriteLine(ioe.Message);
                return false;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            if (!serialPort.IsOpen)
            {
                Console.WriteLine(serialPort.PortName + ": 打开串口失败");
                return false;
            }
            Console.WriteLine(serialPort.PortName + ": 打开成功, 速率: " + baud);
            return true;
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        static private void close_Serial()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                Console.WriteLine("串口关闭成功");
            }
        }
    }
}
