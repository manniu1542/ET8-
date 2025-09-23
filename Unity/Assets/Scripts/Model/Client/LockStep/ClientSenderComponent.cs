namespace ET.Client
{
    /// <summary>
    /// 客户端给NetInner纤程通信的组件 （类似服户端的MessageSender）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ClientSenderComponent: Entity, IAwake, IDestroy
    {
        public int fiberId;

        public ActorId netClientActorId;
    }
}