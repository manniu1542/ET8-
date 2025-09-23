using System.Collections.Generic;
using System.Net;

namespace ET.Server
{
    /// <summary>
    /// 客户端与服务端的通信器,可根据需要使用对应的 tcp/udp/websocket的通信协议.来进行通信，（客户端上就是给服务端发送接收消息，反之亦然）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ProcessOuterSender: Entity, IAwake<IPEndPoint>, IUpdate, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;
        
        public int RpcId;

        public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();
        
        public AService AService;
        
        public NetworkProtocol InnerProtocol = NetworkProtocol.KCP;
    }
}