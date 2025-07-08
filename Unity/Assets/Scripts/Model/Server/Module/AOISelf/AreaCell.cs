using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    //区域,(对AreaOfInterestEntity的辅助)
    [FriendOf(typeof(AreaOfInterestEntity))]
    [ChildOf(typeof(AreaCellMgrComponent))]
    public class AreaCell : Entity, IAwake, IDestroy
    {
        // 当前区域所包含的AOI单位 ，EntityRef<AreaOfInterestEntity>安全的引用类型，他会对比instance是否发生变化了
        public Dictionary<long, EntityRef<AreaOfInterestEntity>> dicAOIUnits = new Dictionary<long, EntityRef<AreaOfInterestEntity>>();
        
        //  哪些AOI单位 看到 当前区域，（当前区域 的aoi有变动 ，都需要通知这里面存储的aoi）
        public Dictionary<long, EntityRef<AreaOfInterestEntity>> dicAOIUnitsVisibleSelf = new Dictionary<long, EntityRef<AreaOfInterestEntity>>();
        
        // 哪些AOI单位的离开需要检测通知存储在里面的aoi
        public Dictionary<long, EntityRef<AreaOfInterestEntity>> dicAOILeaveNeedCheckSelf = new Dictionary<long, EntityRef<AreaOfInterestEntity>>();
        
    }

}