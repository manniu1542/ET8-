using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 服务端给NetInner纤程通信的组件 （类似客户端的ClientSenderComponent）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class MessageSender: Entity, IAwake, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;

        public int RpcId;

        public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();
    }
}