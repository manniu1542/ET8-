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
    // 清空进入和离开的cell集合，为新一轮的计算做准备
    enterCell.Clear();
    leaveCell.Clear();

    // 计算进入的cell的半径，确保覆盖实体的可视距离
    int r = (aoiEntity.ViewDistance - 1) / AOIManagerComponent.CellSize + 1;
    // 初始化离开的cell的半径，通常比进入的半径大，以减少频繁的增删操作
    int leaveR = r;

    // 玩家类型 比 其他在场景的类型 可视半径加大（避免玩家的频繁增删）
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

    // 遍历以当前cell为中心，半径为leaveR的正方形区域
    for (int i = cellX - leaveR; i <= cellX + leaveR; ++i)
    {
        for (int j = cellY - leaveR; j <= cellY + leaveR; ++j)
        {
            // 生成cell的唯一标识
            long cellId = CreateCellId(i, j);
            // 将所有cell添加到离开的集合中
            leaveCell.Add(cellId);

            // 如果当前cell在进入的半径之外，则跳过
            if (i > cellX + r || i < cellX - r || j > cellY + r || j < cellY - r)
            {
                continue;
            }

            // 将在进入的半径之内的cell添加到进入的集合中
            enterCell.Add(cellId);
        }
    }
}

    }
}