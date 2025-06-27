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
        
        // 那些AOI单位 可视 当前区域
        public Dictionary<long, EntityRef<AreaOfInterestEntity>> dicAOIUnitsVisibleSelf = new Dictionary<long, EntityRef<AreaOfInterestEntity>>();
        
        // 那些AOI单位不可视当前区域了
        public Dictionary<long, EntityRef<AreaOfInterestEntity>> dicAOIUnitsInvisibleSelf = new Dictionary<long, EntityRef<AreaOfInterestEntity>>();
        
    }

}