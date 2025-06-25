namespace ET.Server
{
    // 进入视野通知
    [Event(SceneType.Map)]
    public class UnitEnterSightRange_NotifyClient: AEvent<Scene, UnitEnterSightRange>
    {
        protected override async ETTask Run(Scene scene, UnitEnterSightRange args)
        {
            AOIEntity a = args.A;
            AOIEntity b = args.B;
            if (a.Id == b.Id)
            {
                return;
            }
            //A看到B进入了,A的视野了
            Unit ua = a.GetParent<Unit>();
            if (ua.Type() != UnitType.Player)
            {
                return;
            }

            Unit ub = b.GetParent<Unit>();

            MapMessageHelper.NoticeUnitAdd(ua, ub);
            
            await ETTask.CompletedTask;
        }
    }
}