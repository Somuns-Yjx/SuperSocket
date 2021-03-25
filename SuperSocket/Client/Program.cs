using SuperSocket.ProtoBase;
using System;
using System.Net;
using System.Text;

namespace SuperSocketClient
{
    class MyBeginEndMarkReceiveFilter : BeginEndMarkReceiveFilter<StringPackageInfo>
    {
        public MyBeginEndMarkReceiveFilter(string begin, string end)
        : base(Encoding.UTF8.GetBytes(begin), Encoding.UTF8.GetBytes(end))
        {
            this.begin = begin;
            this.end = end;
        }
        string begin;
        string end;

        public override StringPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            //获取接收到的完整数据，包括头和尾
            var body = bufferStream.ReadString((int)bufferStream.Length, Encoding.ASCII);
            //掐头去尾，只返回中间的数据
            body = body.Remove(body.Length - end.Length, end.Length);
            body = body.Remove(0, begin.Length);
            return new StringPackageInfo("", body, new string[] { });
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            string ip = "127.0.0.1";
            int port = 2021;
            string startFilter = "!!!";
            string endFilter = "#";
            SocketClient sc = new SocketClient(ip, port, startFilter, endFilter);
            sc.StartComm();
            while (true)
                Console.ReadKey();
        }
    }
    class SocketClient
    {
        SuperSocket.ClientEngine.EasyClient client;
        /// <summary>
        /// 定义服务端的ip地址和端口，以及接收数据的头和尾，只有在头和尾之间的数据才算有效数据
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">服务端口</param>
        /// <param name="startFilter">数据头</param>
        /// <param name="endFilter">数据尾</param>
        public SocketClient(string ip, int port, string startFilter, string endFilter)
        {
            this.ip = ip;
            this.port = port;
            if (!string.IsNullOrEmpty(startFilter)) this.startFilter = startFilter;
            if (!string.IsNullOrEmpty(endFilter)) this.endFilter = endFilter;
            client = new SuperSocket.ClientEngine.EasyClient();
            client.Initialize(new MyBeginEndMarkReceiveFilter(this.startFilter, this.endFilter), OnReceived);
        }
        string ip;
        int port;
        string startFilter = "!!!";
        string endFilter = "";
        bool cycleSend = false;
        /// <summary>
        /// 要发送到服务端的数据
        /// </summary>
        public string Data { get; set; } = "hello,this is super client\r\n";
        /// <summary>
        /// 开始循环发送数据
        /// </summary>
        public void StartComm()
        {
            cycleSend = true;
            System.Threading.Thread _thread = new System.Threading.Thread(SendData);
            _thread.IsBackground = true;
            _thread.Start();
        }
        /// <summary>
        /// 采用线程间隔一秒发送数据，防止界面卡死
        /// </summary>
        public void SendData()
        {
            while (cycleSend)
            {
                if (!client.IsConnected)
                {
                    ConnectToServer(ip, port);
                }
                if (client.IsConnected)
                {
                    client.Send(Encoding.ASCII.GetBytes("hello,this is super client\r\n"));
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 停止循环发送数据
        /// </summary>
        public void StopComm()
        {
            cycleSend = false;
        }
        /// <summary>
        /// 连接到服务端
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public async void ConnectToServer(string ip, int port)
        {
            var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
            if (connected)
            {
                //发送连接信息
                client.Send(Encoding.ASCII.GetBytes("build connection"));
            }
        }
        public System.EventHandler newReceived;
        /// <summary>
        /// 当读取到数据，触发一个事件，方便外部接收数据
        /// </summary>
        /// <param name="stringPackageInfo"></param>
        public void OnReceived(StringPackageInfo stringPackageInfo)
        {
            Console.WriteLine(stringPackageInfo.Body);
            if (newReceived != null)
            {
                newReceived(stringPackageInfo.Body, new EventArgs());
            }
        }
    }

}
