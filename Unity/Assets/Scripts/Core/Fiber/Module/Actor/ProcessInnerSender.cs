using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 进程内部消息。 纤程之间的 消息传递 发送到指定Handlr执行
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ProcessInnerSender: Entity, IAwake, IDestroy, IUpdate
    {
        public const long TIMEOUT_TIME = 40 * 1000;
        
        public int RpcId;
        /// <summary>
        /// 存储 key 消息 rpc id，value 消息体
        /// </summary>
        public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();
        
        public readonly List<MessageInfo> list = new();
    }
}