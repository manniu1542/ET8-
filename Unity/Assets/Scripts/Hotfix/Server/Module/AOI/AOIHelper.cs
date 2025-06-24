using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(AOIEntity))]
    public static partial class AOIHelper
    {
        /// <summary>
        /// 给当前所处的位置的cell做成唯一id
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static long CreateCellId(int x, int y)
        {
            return (long) ((ulong) x << 32) | (uint) y;
        }
        /// <summary>
        /// 计算当前的进入离开自己的cell列表 (正方形区域，可改做圆形但是会增加计算量)
        /// </summary>
        /// <param name="aoiEntity"></param>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="enterCell"></param>
        /// <param name="leaveCell"></param>
        public static void CalcEnterAndLeaveCell(AOIEntity aoiEntity, int cellX, int cellY, HashSet<long> enterCell, HashSet<long> leaveCell)
        {
            enterCell.Clear();
            leaveCell.Clear();
            int r = (aoiEntity.ViewDistance - 1) / AOIManagerComponent.CellSize + 1;
            int leaveR = r;
            //玩家类型 比 其他在场景的类型 可视半径加大（避免玩家的频繁增删）
            if (aoiEntity.Unit.Type() == UnitType.Player)
            {
                leaveR += 1;
            }
            /*
             *  □ □ □ □ □
             *  □ ● ● ● □
             *  □ ● ■ ● □  // ■=当前实体, ●=enterCell, □=仅leaveCell
             *  □ ● ● ● □
             *  □ □ □ □ □     */ 
            
            for (int i = cellX - leaveR; i <= cellX + leaveR; ++i)
            {
                for (int j = cellY - leaveR; j <= cellY + leaveR; ++j)
                {
                    long cellId = CreateCellId(i, j);
                    leaveCell.Add(cellId);

                    if (i > cellX + r || i < cellX - r || j > cellY + r || j < cellY - r)
                    {
                        continue;
                    }

                    enterCell.Add(cellId);
                }
            }
            
        }
    }
}