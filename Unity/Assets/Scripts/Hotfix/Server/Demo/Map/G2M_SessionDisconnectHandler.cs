namespace ET.Server
{
    [MessageLocationHandler(SceneType.Map)]
    public class G2M_SessionDisconnectHandler: MessageLocationHandler<Unit, G2M_SessionDisconnect>
    {
        protected override async ETTask Run(Unit unit, G2M_SessionDisconnect message)
        {
            Log.Error("退出了，该地图" + unit.Id);
            await ETTask.CompletedTask;
        }
    }
}