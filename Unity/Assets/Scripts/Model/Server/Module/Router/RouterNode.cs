using System.Net;

namespace ET.Server
{
    public enum RouterStatus
    {
        Sync,
        Msg,
    }
    /// <summary>
    /// 
    /// </summary>
    [ChildOf(typeof(RouterComponent))]
    public class RouterNode: Entity, IDestroy, IAwake
    {
        /// <summary>
        /// 路由器需要连接到的目标服务器地址 字符串
        /// </summary>
        public string InnerAddress;
        /// <summary>
        /// 路由器需要连接到的目标服务器地址
        /// </summary>
        public IPEndPoint InnerIpEndPoint;
        /// <summary>
        /// 公网地址
        /// </summary>
        public IPEndPoint OuterIpEndPoint;
        /// <summary>
        /// 客户端的(NetClinet纤程的NetComponent的udp)ip地址
        /// </summary>
        public IPEndPoint SyncIpEndPoint;
        public IKcpTransport KcpTransport;

        public uint OuterConn
        {
            get
            {
                return (uint)this.Id;
            }
        }
        public uint InnerConn;
        public uint ConnectId;
        public long LastRecvOuterTime;
        public long LastRecvInnerTime;

        public int RouterSyncCount;
        public int SyncCount;

#region 限制外网消息数量，一秒最多50个包

        public long LastCheckTime;
        public int LimitCountPerSecond;

#endregion

        public RouterStatus Status;
    }
}