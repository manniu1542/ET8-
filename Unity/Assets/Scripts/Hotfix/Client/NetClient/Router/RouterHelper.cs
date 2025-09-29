using System;
using System.Net;

namespace ET.Client
{
    public static partial class RouterHelper
    {
        // 注册router
        public static async ETTask<Session> CreateRouterSession(this NetComponent netComponent, IPEndPoint address, string account, string password)
        {
            //本地连接的唯一标识 （账号，密码，随机数的二进制 异或），
            uint localConn = (uint)(account.GetLongHashCode() ^ password.GetLongHashCode() ^ RandomGenerator.RandUInt32());
            //发送连接路由（0初次登陆的 连接。非零0就是游戏断线重连的登陆方式），等待路由的消息回复， 随机的路由地址是在路由管理器上存储的。
            //路由 跟NetComponent 收发消息 ， RouterSYN 客户端发给路由 =》  RouterACK 路由接收并返回给客户端
            (uint recvLocalConn, IPEndPoint routerAddress) = await GetRouterAddress(netComponent, address, localConn, 0);

            if (recvLocalConn == 0)
            {
                throw new Exception($"get router fail: {netComponent.Root().Id} {address}");
            }

            Log.Info($"get router: {recvLocalConn} {routerAddress}");
             //创建一个session（路由地址，目标服务器地址，连接路由成功的id）
            Session routerSession = netComponent.Create(routerAddress, address, recvLocalConn);
            //检测session连接的心跳包
            routerSession.AddComponent<PingComponent>();
            //检测路由是否可以用
            routerSession.AddComponent<RouterCheckComponent>();

            return routerSession;
        }

        public static async ETTask<(uint, IPEndPoint)> GetRouterAddress(this NetComponent netComponent, IPEndPoint address, uint localConn,
        uint remoteConn)
        {
            Log.Info($"start get router address: {netComponent.Root().Id} {address} {localConn} {remoteConn}");
            //return (RandomHelper.RandUInt32(), address);
            RouterAddressComponent routerAddressComponent = netComponent.Root().GetComponent<RouterAddressComponent>();
            //获取一个随机的路由地址
            IPEndPoint routerInfo = routerAddressComponent.GetAddress();
            //发送连接请求
            uint recvLocalConn = await netComponent.Connect(routerInfo, address, localConn, remoteConn);

            Log.Info($"finish get router address: {netComponent.Root().Id} {address} {localConn} {remoteConn} {recvLocalConn} {routerInfo}");
            return (recvLocalConn, routerInfo);
        }

        // 向router申请
        private static async ETTask<uint> Connect(this NetComponent netComponent, IPEndPoint routerAddress, IPEndPoint realAddress, uint localConn,
        uint remoteConn)
        {
            uint synFlag = remoteConn == 0 ? KcpProtocalType.RouterSYN : KcpProtocalType.RouterReconnectSYN;

            // 注意，session也以localConn作为id，所以这里不能用localConn作为id  ，连接标识
            long id = (long)(((ulong)localConn << 32) | remoteConn);
            Log.Info("本次路由id:" + id);
            using RouterConnector routerConnector = netComponent.AddChildWithId<RouterConnector>(id);

            int count = 20;
            byte[] sendCache = new byte[512]; //
            //占用了13个字节。  
            uint connectId = RandomGenerator.RandUInt32();
            //是否重连 1
            sendCache.WriteTo(0, synFlag);
            //本地标识 4
            sendCache.WriteTo(1, localConn);
            //远端标识 4
            sendCache.WriteTo(5, remoteConn);
            //连接标识 4 
            sendCache.WriteTo(9, connectId);
            //路由器连接的目标地址字节
            byte[] addressBytes = realAddress.ToString().ToByteArray();
            Array.Copy(addressBytes, 0, sendCache, 13, addressBytes.Length);
            TimerComponent timerComponent = netComponent.Root().GetComponent<TimerComponent>();
            Log.Info($"router connect: {localConn} {remoteConn} {routerAddress} {realAddress}");

            long lastSendTimer = 0;

            while (true)
            {
                long timeNow = TimeInfo.Instance.ClientFrameTime();
                if (timeNow - lastSendTimer > 300)
                {
                    if (--count < 0)
                    {
                        Log.Error($"router connect timeout fail! {localConn} {remoteConn} {routerAddress} {realAddress}");
                        return 0;
                    }

                    lastSendTimer = timeNow;
                    // 确保只发送 参与传输的字节数  addressBytes.Length + 13 。 
                    Log.Info($"连接路由标识{synFlag},连接的id{connectId}");
                    routerConnector.Connect(sendCache, 0, addressBytes.Length + 13, routerAddress);
                }

                await timerComponent.WaitFrameAsync();

                if (routerConnector.Flag == 0)
                {
                    continue;
                }

                return localConn;
            }
        }
    }
}