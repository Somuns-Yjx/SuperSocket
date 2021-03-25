using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Linq;

namespace SuperSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("请按任何键进行启动SuperSocket服务!");
            Console.ReadKey();
            Console.WriteLine();
            var appServer = new AppServer();
            //var appClient = new AppClient();

            //appServer.NewSessionConnected += new SessionHandler<AppSession>(AppServer_NewSessionConnected);
            appServer.NewRequestReceived += new RequestHandler<AppSession, StringRequestInfo>(AppServer_NewRequestReceived);

            //Setup the appServer
            if (!appServer.Setup(2021)) //Setup with listening port
            {
                Console.WriteLine("Failed to setup!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine();

            //Try to start the appServer
            if (!appServer.Start())
            {
                Console.WriteLine("Failed to start!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("The server started successfully, press key 'q' to stop it!");

            while (Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine();
                continue;
            }

            //Stop the appServer
            appServer.Stop();

            Console.WriteLine("The server was stopped!");
            Console.ReadKey();

        }
        static void AppServer_NewSessionConnected(AppSession session)
        {
            session.Send("Welcome to SuperSocket Telnet Server!");
        }

        static void AppServer_NewRequestReceived(AppSession session, StringRequestInfo requestInfo)
        {
            switch (requestInfo.Key.ToUpper())
            {
                case ("ECHO"):
                    session.Send(requestInfo.Body);
                    break;

                case ("ADD"):
                    session.Send(requestInfo.Parameters.Select(p => Convert.ToInt32(p)).Sum().ToString());
                    break;

                case ("MULT"):

                    var result = 1;

                    foreach (var factor in requestInfo.Parameters.Select(p => Convert.ToInt32(p)))
                    {
                        result *= factor;
                    }

                    session.Send(result.ToString());
                    break;
            }
        }
    }
}
