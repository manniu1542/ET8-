namespace ET.Server
{
    // 进入视野通知 A 客户端，自己的视野B出现了
    [Event(SceneType.Map)]
    public class UnitAOIAVisibleBEvent_NotifyClient : AEvent<Scene, UnitAOIAVisibleBEvent>
    {
        protected override async ETTask Run(Scene scene, UnitAOIAVisibleBEvent args)
        {
            AreaOfInterestEntity a = args.A;
            AreaOfInterestEntity b = args.B;
            if (a.Id == b.Id)return;
            //A不是玩家就不用通知了
            if (!a.IsPlayer()) return;
            
            Unit ua = a.GetParent<Unit>();
            Unit ub = b.GetParent<Unit>();
            Log.Error($"a {ua.Id} add b{ub.Id}:");
            MapMessageHelper.NoticeUnitAdd(ua, ub);

            await ETTask.CompletedTask;
        }
    }
}