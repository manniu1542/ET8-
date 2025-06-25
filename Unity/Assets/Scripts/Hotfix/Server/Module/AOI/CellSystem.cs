using System.Collections.Generic;
using System.Text;

namespace ET.Server
{
    [EntitySystemOf(typeof(Cell))]
    [FriendOf(typeof(Cell))]
    public static partial class CellSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.Cell self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this Cell self)
        {
            self.AOIUnits.Clear();

            self.SubsEnterEntities.Clear();

            self.SubsLeaveEntities.Clear();
        }
        /// <summary>
        /// 当前的cell位置所有的unit单位添加他的aoi实体到字典里
        /// </summary>
        /// <param name="self"></param>
        /// <param name="aoiEntity"></param>
        public static void Add(this Cell self, AOIEntity aoiEntity)
        {
            self.AOIUnits.Add(aoiEntity.Id, aoiEntity);
        }

        public static void Remove(this Cell self, AOIEntity aoiEntity)
        {
            self.AOIUnits.Remove(aoiEntity.Id);
        }

        /// <summary>
        /// 将64位的单元格ID转换为字符串表示的形式。
        /// </summary>
        /// <param name="cellId">64位的单元格ID。</param>
        /// <returns>格式为"x:y"的字符串，其中x和y分别是单元格ID的高32位和低32位部分。</returns>
        public static string CellIdToString(this long cellId)
        {
            // 提取cellId的低32位作为y坐标    16   
            int y = (int)(cellId & 0xffffffff);
            // 提取cellId的高32位作为x坐标
            int x = (int)((ulong)cellId >> 32);
            // 返回格式化的坐标字符串
            return $"{x}:{y}";
        }


        public static string CellIdToString(this HashSet<long> cellIds)
        {
            StringBuilder sb = new StringBuilder();
            foreach (long cellId in cellIds)
            {
                sb.Append(cellId.CellIdToString());
                sb.Append(",");
            }

            return sb.ToString();
        }

    }
}