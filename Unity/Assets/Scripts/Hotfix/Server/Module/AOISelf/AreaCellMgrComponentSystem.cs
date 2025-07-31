using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(AreaCellMgrComponent))]
    [FriendOf(typeof(AreaCellMgrComponent))]
    [FriendOfAttribute(typeof(ET.Server.AreaOfInterestEntity))]
    [FriendOfAttribute(typeof(ET.Server.AreaCell))]
    public static partial class AreaCellMgrComponentSystem
    {
        [EntitySystem]
        private static void Awake(this AreaCellMgrComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this AreaCellMgrComponent self)
        {
        }

        /// <summary>
        /// 根据id获取AreaCell
        /// </summary>
        /// <param name="self"></param>
        /// <param name="areaCell"></param>
        public static AreaCell GetOrCreateAreaCell(this AreaCellMgrComponent self, long acId)
        {
            AreaCell ac = self.GetChild<AreaCell>(acId);
            if (ac == null)
            {
                ac = self.AddChildWithId<AreaCell>(acId);
            }

            return ac;
        }

        /// <summary>
        /// 绑定 AOI到AreaCell
        /// </summary>
        /// <param name="self"></param>
        /// <param name="areaCell"></param>
        public static void BindAOIToAreaCell(this AreaCellMgrComponent self, AreaOfInterestEntity aoi, float x, float y)
        {
            int acX = (int)(x * AreaCellMgrComponent.FloatToIntConversionFactor) / AreaCellMgrComponent.AreaCellSize;
            int acY = (int)(y * AreaCellMgrComponent.FloatToIntConversionFactor) / AreaCellMgrComponent.AreaCellSize;

            aoi.ResetVisibleAndLeveCheckAreaCells(acX, acY);
            AreaCell ac = null;
            //AOI设置可视区域
            foreach (var cellId in aoi.hsVisibleAreaCells)
            {
                ac = self.GetOrCreateAreaCell(cellId);
                aoi.LinkToVisibleAreaCell(ac);
            }

            //AOI设置自己离开时候可能需要检查的
            foreach (var cellId in aoi.hsLeaveNeedCheckAreaCells)
            {
                ac = self.GetOrCreateAreaCell(cellId);
                aoi.LinkToLeaveNeedCheckAreaCell(ac);
            }

            //绑定aoi跟ac关联
            ac = self.GetOrCreateAreaCell(AreaCellHelper.GetACIdByAOIPos(acX, acY));
            ac.AddAOI(aoi);
            aoi.Cell = ac;

            //通知订阅该Cell的,那些可以看到该Cell的AOI,广播给他消息，其他的AOI也设置看到了该aoi的广播，
            AreaOfInterestEntity otherAoi = null;
            foreach (var kv in ac.dicAOIUnitsVisibleSelf)
            {
                otherAoi = kv.Value;
                otherAoi.AddVisibleOtherAOI(aoi);
            }
        }

        /// <summary>
        /// 取消 AOI与AreaCell绑定
        /// </summary>
        /// <param name="self"></param>
        /// <param name="areaCell"></param>
        public static void UnBindAOIFormAreaCell(this AreaCellMgrComponent self, AreaOfInterestEntity aoi)
        {
            //aoi与ac的绑定还没建立起来联系
            if (aoi == null || aoi.Cell == null) return;

            #region 解绑从AOI自己出发

            //取消aoi可视区域内的格子对的他的关联。
            AreaCell ac = null;
            foreach (long cellId in aoi.hsVisibleAreaCells)
            {
                ac = self.GetOrCreateAreaCell(cellId);
                aoi.UnLinkToVisibleAreaCell(ac);
            }

            //aoi离开时候需要检查的格子（比可视野范围内多一格子）。通知自己取消对那些 自己看到的aoi的关联
            foreach (long cellId in aoi.hsLeaveNeedCheckAreaCells)
            {
                ac = self.GetOrCreateAreaCell(cellId);
                aoi.UnLinkToLeaveNeedCheckAreaCell(ac);
            }

            //检查 自己能看到的aoi移除情况
            if (aoi.dicVisibleAOIUnits.Count > 0)
            {
                string error = "AOI :" + aoi.Id + " 能看到的其他的AOI没有移除完毕还有";
                foreach (var aoiId in aoi.dicVisibleAOIUnits.Keys)
                {
                    error += " aoi_id:" + aoiId;
                }

                Log.Error(error);
            }

            #endregion

            #region 解绑从AOI所关联的Cell出发

            AreaOfInterestEntity aoiInCell = null;
            //从当前aoi所在的cell中，把所有 移除时候需要检查关联该格子的aoi，检查一遍那些看到自己了的aoi，发送移除掉自己的消息
            foreach (var aoiInCellTmp in aoi.Cell.dicAOILeaveNeedCheckSelf.Values)
            {
                aoiInCell = aoiInCellTmp;
                aoiInCell.RemoveVisibleOtherAOI(aoi);
            }

            //检查 aoi所在的格子,所需要监视该格子的aoi在自己销毁的时候。其他的aoi移除自己的情况
            if (aoi.dicOtherAOIUnitsVisibleSelf.Count > 0)
            {
                string error = "AOI:" + aoi.Id + " 还有部分其他的AOI,在它被移除的时候没有移除它";
                foreach (var aoiId in aoi.dicOtherAOIUnitsVisibleSelf.Keys)
                {
                    error += " aoi_id:" + aoiId;
                }

                Log.Error(error);
            }

            #endregion

            //取消ac的关联。
            aoi.Cell.RemoveAOI(aoi);
            aoi.Cell = null;
        }

        /// <summary>
        /// 需要检测当前的移动 距离 不能大于 1格的一半，要检测。这个移动过大了，可能需要调整1格的大小，或者他的可视范围的大小了。
        /// </summary>
        /// <param name="self"></param>
        public static void Move(this AreaCellMgrComponent self, AreaOfInterestEntity aoi, int newCellX, int newCellY)
        {
           
            AreaCell oldCell = aoi.Cell;
            long nweAreaCellId = AreaCellHelper.GetACIdByAOIPos(newCellX, newCellY);
            //所在的格子 没有发生改变
            if (nweAreaCellId == oldCell.Id) return;

            //从aoi本身角度 来管理 他存储的其他aoi 对比 所需要 通知的 aoi
            self.UpdateAOIForAOISelfChange(aoi, newCellX, newCellY);

            AreaCell newCell = self.GetOrCreateAreaCell(nweAreaCellId);
            //因为当前aoi所在的格子放生变化了。 从格子角度 来管理 新/旧 格子对他们的aoi产生的变化通知 
            self.UpdateAOIForCellChange(aoi, newCell, oldCell);
        }

        /// <summary>
        /// 从aoi本身角度 来管理 他存储的其他aoi 对比 所需要 通知的 aoi
        /// <param name="self"></param>
        /// <param name="aoi"></param>
        /// <param name="newCell"></param>
        /// <param name="oldCell"></param>
        public static void UpdateAOIForAOISelfChange(this AreaCellMgrComponent self, AreaOfInterestEntity aoi, int CellX, int CellY)
        {
            aoi.ResetTmpVisibleAndLeveCheckAreaCells(CellX, CellY);
            AreaCell acTmp = null;
            // ⊕=需要通知取消关联的  ■=当前实体, ●=hsVisibleAreaCells, □=hsLeaveNeedCheck
            /*  例如向右移动一格
             *  □ □ □ □ □
             *  □ ● ● ● □
             *  □ ● ■ ● □
             *  □ ● ● ● □1
             *  □ □ □ □ □
             *
             *  ⊕ □ □ □ □ □
             *  ⊕ □ ● ● ● □
             *  ⊕ □ ● ■ ● □
             *  ⊕ □ ● ● ● □
             *  ⊕ □ □ □ □ □
             * */

            #region hsLeaveNeedCheckAreaCells  更新

            //更新 关联下新增的 需要 检测的格子
            foreach (long acID in aoi.hsTmpLeaveNeedCheckAreaCells)
            {
                if (aoi.hsLeaveNeedCheckAreaCells.Contains(acID))
                {
                    continue;
                }

                acTmp = self.GetOrCreateAreaCell(acID);
                //格子需要检查下 该aoi离开以后 ，格子里面的aoi检查下
                aoi.LinkToLeaveNeedCheckAreaCell(acTmp);
            }

            //在与hsLeaveNeedCheckAreaCells容器中 ，移除hsTmpLeaveNeedCheckAreaCells 与hsLeaveNeedCheckAreaCells 相同的元素。
            aoi.hsLeaveNeedCheckAreaCells.ExceptWith(aoi.hsTmpLeaveNeedCheckAreaCells);

            //更新离开需要检测的格子
            foreach (long acID in aoi.hsLeaveNeedCheckAreaCells)
            {
                acTmp = self.GetOrCreateAreaCell(acID);
                //格子需要检查下 该aoi离开以后 ，格子里面的aoi检查下
                aoi.UnLinkToLeaveNeedCheckAreaCell(acTmp);
            }

            ObjectHelper.Swap(ref aoi.hsLeaveNeedCheckAreaCells, ref aoi.hsTmpLeaveNeedCheckAreaCells);

            #endregion

            #region hsVisibleAreaCells 更新

            //更新 关联下新增的 需要 检测的格子
            foreach (long acID in aoi.hsTmpVisibleAreaCells)
            {
                if (aoi.hsVisibleAreaCells.Contains(acID))
                {
                    continue;
                }

                acTmp = self.GetOrCreateAreaCell(acID);
                aoi.LinkToVisibleAreaCell(acTmp);
            }

            //在与hsLeaveNeedCheckAreaCells容器中 ，移除hsTmpLeaveNeedCheckAreaCells 与hsLeaveNeedCheckAreaCells 相同的元素。
            aoi.hsVisibleAreaCells.ExceptWith(aoi.hsTmpVisibleAreaCells);

            foreach (long acID in aoi.hsVisibleAreaCells)
            {
                acTmp = self.GetOrCreateAreaCell(acID);
                aoi.UnLinkToVisibleAreaCell(acTmp);
            }

            ObjectHelper.Swap(ref aoi.hsVisibleAreaCells, ref aoi.hsTmpVisibleAreaCells);

            #endregion
        }

        /// <summary>
        ///   更新相近的aoi, 因为这个aoi的所在格子的更换  ,导致跟新/旧关联的 格子中aoi的增删
        /// </summary>
        /// <param name="self"></param>
        /// <param name="aoi"></param>
        /// <param name="newCell"></param>
        /// <param name="oldCell"></param>
        public static void UpdateAOIForCellChange(this AreaCellMgrComponent self, AreaOfInterestEntity aoi, AreaCell newCell,
        AreaCell oldCell)
        {
            aoi.Cell = newCell;
            oldCell.RemoveAOI(aoi);
            newCell.AddAOI(aoi);
            //这个格子 有变动 离开的时候 需要通知的 aoi
            foreach (AreaOfInterestEntity aoiLeveCheck in oldCell.dicAOILeaveNeedCheckSelf.Values)
            {
                aoiLeveCheck.RemoveVisibleOtherAOI(aoi);
            }

            //这个格子 有变动 新增的 需要通知的 aoi
            foreach (AreaOfInterestEntity aoiVisible in newCell.dicAOIUnitsVisibleSelf.Values)
            {
                aoiVisible.AddVisibleOtherAOI(aoi);
            }
        }
    }
}