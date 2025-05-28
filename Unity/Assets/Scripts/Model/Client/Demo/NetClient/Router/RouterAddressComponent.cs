using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    /// <summary>
    /// 路由地址管理组件 （有路由管理器的ip地址，通过访问获取路由管理器上的 所有路由）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class RouterAddressComponent: Entity, IAwake<string, int>
    {
        public IPAddress RouterManagerIPAddress { get; set; }
        public string RouterManagerHost;
        public int RouterManagerPort;
        public HttpGetRouterResponse Info;
        /// <summary>
        /// 配合 随机路由使用的自增索引
        /// </summary>
        public int RouterIndex;
    }
}