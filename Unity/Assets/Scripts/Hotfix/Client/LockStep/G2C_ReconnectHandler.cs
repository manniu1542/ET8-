namespace ET.Client
{
    /// <summary>
    /// 玩家重连的房间消息。
    /// </summary>
    [MessageHandler(SceneType.LockStep)]
    public class G2C_ReconnectHandler: MessageHandler<Scene, G2C_Reconnect>
    {
        protected override async ETTask Run(Scene root, G2C_Reconnect message)
        {
            await LSSceneChangeHelper.SceneChangeToReconnect(root, message);
            await ETTask.CompletedTask;
        }
    }
}