namespace ET
{
    /// <summary>
    /// rpc消息的UnOrderedMessage 消息类型的 处理
    /// </summary>
    [Invoke((long)MailBoxType.UnOrderedMessage)]
    public class MailBoxType_UnOrderedMessageHandler: AInvokeHandler<MailBoxInvoker>
    {
        public override void Handle(MailBoxInvoker args)
        {
            HandleAsync(args).Coroutine();
        }
        
        private static async ETTask HandleAsync(MailBoxInvoker args)
        {
            MailBoxComponent mailBoxComponent = args.MailBoxComponent;
            
            MessageObject messageObject = args.MessageObject;
            
            await MessageDispatcher.Instance.Handle(mailBoxComponent.Parent, args.FromAddress, messageObject);
        }
    }
}