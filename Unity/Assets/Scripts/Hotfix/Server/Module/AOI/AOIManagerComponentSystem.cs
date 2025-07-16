using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(AOIManagerComponent))]
    [FriendOf(typeof(AOIEntity))]
    [FriendOf(typeof(Cell))]
    public static partial class AOIManagerComponentSystem
    {
        /// <summary>
        /// 添加AreaOfInterest实体到 AOI管理组件
        /// </summary>
        /// <param name="self"></param>
        /// <param name="aoiEntity"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Add(this AOIManagerComponent self, AOIEntity aoiEntity, float x, float y)
        {
            //浮点数运算可能会导致误差累积,尤其是 CellSize 自身如果不乘上1000，且是个小数的时候，小数/小数 ，浮点数运算出误差大
            int cellX = (int)(x * 1000) / AOIManagerComponent.CellSize;
            int cellY = (int)(y * 1000) / AOIManagerComponent.CellSize;
            //最小的可视范围
            if (aoiEntity.ViewDistance == 0)
            {
                aoiEntity.ViewDistance = 1;
            }

            //给当前的aoi进行初始化，他当前可视区域的cell的id
            AOIHelper.CalcEnterAndLeaveCell(aoiEntity, cellX, cellY, aoiEntity.SubEnterCells, aoiEntity.SubLeaveCells);

            // 遍历EnterCell
            foreach (long cellId in aoiEntity.SubEnterCells)
            {
                Cell cell = self.GetCell(cellId);
                //添加进Cell的SeeUnits中，这个cell的每一个 Unit 都进行 互相订阅  ，通知自己，别的aoi进入了
                aoiEntity.SubEnter(cell);
            }

            // 遍历LeaveCell
            foreach (long cellId in aoiEntity.SubLeaveCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.SubLeave(cell);
            }

            //管理器 更新当前 自己加入的Cell ，添加该Unit的AOI，
            Cell selfCell = self.GetCell(AOIHelper.CreateCellId(cellX, cellY));
            aoiEntity.Cell = selfCell;
            selfCell.Add(aoiEntity);
            // 通知订阅该Cell的,那些可以看到该Cell的AOI,广播给他消息，有新的Unit进入了
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in selfCell.SubsEnterEntities)
            {
                AOIEntity e = kv.Value;
                e.EnterSight(aoiEntity);
            }
        }

        /// <summary>
        /// 从AOI管理组件中移除一个AOI实体。
        /// </summary>
        /// <param name="self">AOI管理组件，作为扩展方法的实例。</param>
        /// <param name="aoiEntity">要移除的AOI实体。</param>
        public static void Remove(this AOIManagerComponent self, AOIEntity aoiEntity)
        {
            // 如果实体未分配到任何Cell，则直接返回，无需执行移除操作
            if (aoiEntity.Cell == null)
            {
                return;
            }

            // 通知订阅该Cell Leave的Unit
            aoiEntity.Cell.Remove(aoiEntity);
            //通知这个cell，刚好离开 这个cell 订阅者。有新的Unit的AOI离开了
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in aoiEntity.Cell.SubsLeaveEntities)
            {
                AOIEntity e = kv.Value;
                e?.LeaveSight(aoiEntity);
            }

            //自己看到的cell,告诉这些cell，cell就移除 他被那些AOI所能看到
            foreach (long cellId in aoiEntity.SubEnterCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.UnSubEnter(cell);
            }
            //自己看到的cell,告诉这些cell，cell就移除 他刚好被那些AOI离开
            foreach (long cellId in aoiEntity.SubLeaveCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.UnSubLeave(cell);
            }

            // 检查
            if (aoiEntity.SeeUnits.Count > 1)
            {
                Log.Error($"aoiEntity has see units: {aoiEntity.SeeUnits.Count}");
            }

            if (aoiEntity.BeSeeUnits.Count > 1)
            {
                Log.Error($"aoiEntity has beSee units: {aoiEntity.BeSeeUnits.Count}");
            }
        }

        private static Cell GetCell(this AOIManagerComponent self, long cellId)
        {
            Cell cell = self.GetChild<Cell>(cellId);
            if (cell == null)
            {
                cell = self.AddChildWithId<Cell>(cellId);
            }

            return cell;
        }

        public static void Move(AOIEntity aoiEntity, Cell newCell, Cell preCell)
        {
            aoiEntity.Cell = newCell;
            preCell.Remove(aoiEntity);
            newCell.Add(aoiEntity);
            // 通知订阅该newCell Enter的Unit
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in newCell.SubsEnterEntities)
            {
                AOIEntity e = kv.Value;
                if (e.SubEnterCells.Contains(preCell.Id))
                {   
                    continue;
                }

                e.EnterSight(aoiEntity);
            }

            // 通知订阅preCell leave的Unit
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in preCell.SubsLeaveEntities)
            {
                // 如果新的cell仍然在对方订阅的subleave中
                AOIEntity e = kv.Value;
                if (e.SubLeaveCells.Contains(newCell.Id))
                {
                    continue;
                }

                e.LeaveSight(aoiEntity);
            }
        }

        public static void Move(this AOIManagerComponent self, AOIEntity aoiEntity, int cellX, int cellY)
        {
            long newCellId = AOIHelper.CreateCellId(cellX, cellY);
            if (aoiEntity.Cell.Id == newCellId) // cell没有变化
            {
                return;
            }

            // 自己加入新的Cell
            Cell newCell = self.GetCell(newCellId);
            Move(aoiEntity, newCell, aoiEntity.Cell);

            AOIHelper.CalcEnterAndLeaveCell(aoiEntity, cellX, cellY, aoiEntity.enterHashSet, aoiEntity.leaveHashSet);

            // 算出自己leave新Cell
            foreach (long cellId in aoiEntity.leaveHashSet)
            {
                if (aoiEntity.SubLeaveCells.Contains(cellId))
                {
                    continue;
                }

                Cell cell = self.GetCell(cellId);
                aoiEntity.SubLeave(cell);
            }

            // 算出需要通知离开的Cell
            aoiEntity.SubLeaveCells.ExceptWith(aoiEntity.leaveHashSet);
            foreach (long cellId in aoiEntity.SubLeaveCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.UnSubLeave(cell);
            }

            // 这里交换两个HashSet,提高性能
            ObjectHelper.Swap(ref aoiEntity.SubLeaveCells, ref aoiEntity.leaveHashSet);

            // 算出自己看到的新Cell
            foreach (long cellId in aoiEntity.enterHashSet)
            {
                if (aoiEntity.SubEnterCells.Contains(cellId))
                {
                    continue;
                }

                Cell cell = self.GetCell(cellId);
                aoiEntity.SubEnter(cell);
            }

            // 离开的Enter
            aoiEntity.SubEnterCells.ExceptWith(aoiEntity.enterHashSet);
            foreach (long cellId in aoiEntity.SubEnterCells)
            {
                Cell cell = self.GetCell(cellId);
                aoiEntity.UnSubEnter(cell);
            }

            // 这里交换两个HashSet,提高性能
            ObjectHelper.Swap(ref aoiEntity.SubEnterCells, ref aoiEntity.enterHashSet);
        }
    }
}