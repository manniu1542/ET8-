namespace ET.Server
{
    //  进入视野通知 A 客户端，自己视野里看不到B了
    [Event(SceneType.Map)]
    public class UnitAOIAInVisibleBEvent_NotifyClient: AEvent<Scene, UnitAOIAInVisibleBEvent>
    {
        protected override async ETTask Run(Scene scene, UnitAOIAInVisibleBEvent args)
        {
            await ETTask.CompletedTask;
            AreaOfInterestEntity a = args.A;
            AreaOfInterestEntity b = args.B;
            
            //如果被通知的一方不是玩家实体,也就无需广播了
            if (!a.IsPlayer()) return;
            MapMessageHelper.NoticeUnitRemove(a.GetParent<Unit>(), b.GetParent<Unit>());
        }
    }
}