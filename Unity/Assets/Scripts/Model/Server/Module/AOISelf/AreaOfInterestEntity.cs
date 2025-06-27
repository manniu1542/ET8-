using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    //感兴趣的区域
    [ComponentOf(typeof(Unit))]
    public class AreaOfInterestEntity : Entity, IAwake<int, float3>, IDestroy
    {  
        // 所属的Unit
        public Unit Unit => this.GetParent<Unit>();
        /// <summary>
        /// 可视距离
        /// </summary>
        public int ViewDistance;

        private EntityRef<AreaCell> cell;
        /// <summary>
        /// 当前自己所处的 区域
        /// </summary>
        public AreaCell Cell
        {
            get
            {    // 通过EntityRef 里面的方法互相转换
                return this.cell;
            }
            set
            {   // 通过EntityRef 里面的方法互相转换
                this.cell = value;
            }
        }
        
        
        /// <summary>
        /// 可以看到的 区域列表 (区域的Id，x是前32位，y是后32位)
        /// </summary>
        public HashSet<long> hsVisibleAreaCells = new HashSet<long>();
        
        /// <summary>
        /// 不可看到的 区域 列表 (区域的Id，x是前32位，y是后32位)
        /// </summary>
        public HashSet<long> hsInvisibleAreaCells = new HashSet<long>();
        
        
        
        /// <summary>
        /// 当前自己看到的其他 AOI单位 字典
        /// </summary>
        public Dictionary<long, EntityRef<AreaOfInterestEntity>> dicVisibleAOIUnits = new Dictionary<long, EntityRef<AreaOfInterestEntity>>();
        
        /// <summary>
        /// 当前自己看到的其他 AOI玩家单位 字典
        /// </summary>
        public Dictionary<long, EntityRef<AreaOfInterestEntity>> dicVisibleAOIPlayers = new Dictionary<long, EntityRef<AreaOfInterestEntity>>();
        
        /// <summary>
        /// 当前自己不可视的其他 AOI单位 字典
        /// </summary>
        public Dictionary<long, EntityRef<AreaOfInterestEntity>> dicInvisibleAOIUnits = new Dictionary<long, EntityRef<AreaOfInterestEntity>>();
        
        /// <summary>
        /// 当前自己不可视的其他 AOI玩家单位 字典
        /// </summary>
        public Dictionary<long, EntityRef<AreaOfInterestEntity>> dicInvisibleAOIPlayers = new Dictionary<long, EntityRef<AreaOfInterestEntity>>();

        
        
        
        
        
    }

}