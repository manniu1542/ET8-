using System.Net;
using System.Net.Sockets;

namespace ET
{
    [EntitySystemOf(typeof(NetComponent))]
    [FriendOf(typeof(NetComponent))]
    public static partial class NetComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NetComponent self, IPEndPoint address, NetworkProtocol protocol)
        {
            self.AService = new KService(address, protocol, ServiceType.Outer);
            self.AService.AcceptCallback = self.OnAccept;
            self.AService.ReadCallback = self.OnRead;
            self.AService.ErrorCallback = self.OnError;
        }
        
        [EntitySystem]
        private static void Awake(this NetComponent self, AddressFamily addressFamily, NetworkProtocol protocol)
        {
            self.AService = new KService(addressFamily, protocol, ServiceType.Outer);
            self.AService.ReadCallback = self.OnRead;
            self.AService.ErrorCallback = self.OnError;
        }
        
        [EntitySystem]
        private static void Update(this NetComponent self)
        {
            self.AService.Update();
        }

        [EntitySystem]
        private static void Destroy(this NetComponent self)
        {
            self.AService.Dispose();
        }

        private static void OnError(this NetComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
        }

        // 这个channelId是由CreateAcceptChannelId生成的
        private static void OnAccept(this NetComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;

            if (self.IScene.SceneType != SceneType.BenchmarkServer)
            {
                // 挂上这个组件，5秒就会删除session，所以客户端验证完成要删除这个组件。该组件的作用就是防止外挂一直连接不发消息也不进行权限验证
                session.AddComponent<SessionAcceptTimeoutComponent>();
                // 客户端连接，2秒检查一次recv消息，10秒没有消息则断开
                session.AddComponent<SessionIdleCheckerComponent>();
            }
        }
        
        private static void OnRead(this NetComponent self, long channelId, MemoryBuffer memoryBuffer)
        {
            //接收到网络消息的处理
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            session.LastRecvTime = TimeInfo.Instance.ClientNow();
            
            (ActorId _, object message) = MessageSerializeHelper.ToMessage(self.AService, memoryBuffer);
            self.AService.Recycle(memoryBuffer);
            
            LogMsg.Instance.Debug(self.Fiber(), message);
            //发送到指定服务器场景的消息处理事件  NetComponentOnRead 。
            EventSystem.Instance.Invoke((long)self.IScene.SceneType, new NetComponentOnRead() {Session = session, Message = message});
        }
        
        public static Session Create(this NetComponent self, IPEndPoint realIPEndPoint)
        {
            long channelId = NetServices.Instance.CreateConnectChannelId();
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = realIPEndPoint;
            if (self.IScene.SceneType != SceneType.BenchmarkClient)
            {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            
            self.AService.Create(session.Id, session.RemoteAddress);

            return session;
        }

        public static Session Create(this NetComponent self, IPEndPoint routerIPEndPoint, IPEndPoint realIPEndPoint, uint localConn)
        {
            long channelId = localConn;
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            //目标服务器消息连接地址
            session.RemoteAddress = realIPEndPoint;
            if (self.IScene.SceneType != SceneType.BenchmarkClient)
            {//如果 session超过20秒没发消息，就自动移除了。组件
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            //初次给路由服务器发送SYN包的请求
            self.AService.Create(session.Id, routerIPEndPoint);
            return session;
        }
    }
}