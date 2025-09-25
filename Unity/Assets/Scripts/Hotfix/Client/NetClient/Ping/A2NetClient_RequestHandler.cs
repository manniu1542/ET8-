namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class LockStep2NetClient_SetPingHandler : MessageHandler<Scene, LockStep2NetClient_SetPing>
    {
        protected override async ETTask Run(Scene root, LockStep2NetClient_SetPing request)
        {
            var pingComponent = root.GetComponent<SessionComponent>().Session?.GetComponent<PingComponent>();
            pingComponent.SetUsing(request.IsNormalPing);
            await ETTask.CompletedTask;
        }
    }
}