using System.Collections.Generic;

namespace ET.Server
{
    [ChildOf(typeof(AOIManagerComponent))]
    public class Cell: Entity, IAwake, IDestroy
    {
        // 处在这个cell的单位
        public Dictionary<long, EntityRef<AOIEntity>> AOIUnits = new Dictionary<long, EntityRef<AOIEntity>>();

        // 订阅了这个Cell的进入事件  （这个Cell知道了，那些个Unit的AOI是可以 看到了这个cell的）
        public Dictionary<long, EntityRef<AOIEntity>> SubsEnterEntities = new Dictionary<long, EntityRef<AOIEntity>>();

        // 订阅了这个Cell的退出事件   （这个Cell知道了，那些个Unit的AOI是可以 刚好离开了这个cell的）
        public Dictionary<long, EntityRef<AOIEntity>> SubsLeaveEntities = new Dictionary<long, EntityRef<AOIEntity>>();
    }
}