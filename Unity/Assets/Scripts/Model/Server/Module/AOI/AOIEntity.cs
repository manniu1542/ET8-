using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class AOIEntity: Entity, IAwake<int, float3>, IDestroy
    {
        public Unit Unit => this.GetParent<Unit>();

        public int ViewDistance;

        private EntityRef<Cell> cell;

        public Cell Cell
        {
            get
            {
                return this.cell;
            }
            set
            {
                this.cell = value;
            }
        }

        // 观察进入视野的Cell (每个Unit单位的可视范围不一样，进入的cell范围数量就不同)
        public HashSet<long> SubEnterCells = new HashSet<long>();

        // 观察离开视野的Cell (每个Unit单位的可视范围不一样，进入的cell范围数量就不同)
        public HashSet<long> SubLeaveCells = new HashSet<long>();
        
        // 观察进入视野的Cell
        public HashSet<long> enterHashSet = new HashSet<long>();

        // 观察离开视野的Cell
        public HashSet<long> leaveHashSet = new HashSet<long>();

        // 我看的见的Unit （统计所能看到的Cell，每个cell下面的所有Unit）
        public Dictionary<long, EntityRef<AOIEntity>> SeeUnits = new Dictionary<long, EntityRef<AOIEntity>>();
        
        // 看见我的Unit   （可视范围不一样导致,别的unit可以看到自己，自己看不到别的Unit,别的Unit看到自己的所在cell，记录这些unit的aoi）
        public Dictionary<long, EntityRef<AOIEntity>> BeSeeUnits = new Dictionary<long, EntityRef<AOIEntity>>();
        
        // 我看的见的Player       （统计所能看到的Cell，每个cell下面的所有Unit中的玩家单位）
        public Dictionary<long, EntityRef<AOIEntity>> SeePlayers = new Dictionary<long, EntityRef<AOIEntity>>();

        // 看见我的Player单独放一个Dict，用于广播       （别的unit类型是玩家单位，看到自己的所在cell，记录这些unit类型是玩家单位的aoi）
        public Dictionary<long, EntityRef<AOIEntity>> BeSeePlayers = new Dictionary<long, EntityRef<AOIEntity>>();
    }
}