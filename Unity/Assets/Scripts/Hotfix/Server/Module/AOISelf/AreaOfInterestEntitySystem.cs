using System.Collections.Generic;
using DotRecast.Core;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(AreaOfInterestEntity))]
    [FriendOf(typeof(AreaOfInterestEntity))]
    [FriendOfAttribute(typeof(ET.Server.AreaCell))]
    public static partial class AreaOfInterestEntitySystem
    {
        [EntitySystem]
        private static void Awake(this AreaOfInterestEntity self, int distance, float3 pos)
        {
            self.ViewDistance = distance;
            self.Scene().GetComponent<AreaCellMgrComponent>().BindAOIToAreaCell(self, pos.x, pos.z);
        }

        [EntitySystem]
        private static void Destroy(this AreaOfInterestEntity self)
        {
            self.Scene().GetComponent<AreaCellMgrComponent>().UnBindAOIFormAreaCell(self);
            self.ViewDistance = 0;
            self.dicOtherPlayersVisibleSelf.Clear();
            self.dicOtherAOIUnitsVisibleSelf.Clear();
            self.dicVisibleAOIPlayers.Clear();
            self.dicVisibleAOIUnits.Clear();
            self.hsLeaveNeedCheckAreaCells.Clear();
            self.hsVisibleAreaCells.Clear();

            self.dicOtherPlayersVisibleSelf = null;
            self.dicOtherAOIUnitsVisibleSelf = null;
            self.dicVisibleAOIPlayers = null;
            self.dicVisibleAOIUnits = null;
            self.hsLeaveNeedCheckAreaCells = null;
            self.hsVisibleAreaCells = null;
        }

        public static bool IsPlayer(this AreaOfInterestEntity self)
        {
            return self.GetParent<Unit>().Type() == UnitType.Player;
        }
        /// <summary>
        ///  重置当前可视与不可视的区域格子
        /// </summary>
        /// <param name="self"></param>
        /// <param name="selfCellX"></param>
        /// <param name="selfCellY"></param>
        private static void _ResetVisibleAndLeveCheckAreaCells(this AreaOfInterestEntity self, int selfCellX, int selfCellY,ref HashSet<long> hsVisible,ref HashSet<long> hsLeaveNeedCheck)
        {
            hsVisible.Clear();
            hsLeaveNeedCheck.Clear();

            if (self.ViewDistance <= 0)
                self.ViewDistance = 1;
            //看到格子大小的尺寸  （至少是1,取值总是取ceil向上取整了）
            int viewCellSize = (self.ViewDistance - 1) / AreaCellMgrComponent.FloatToIntConversionFactor + 1;

            //检测超出视野范围的大小
            int checkOverViewSize = viewCellSize;
            //玩家检查的格子，多增加1，避免因为玩家在边界边缘的时候，刚好错过移除检查
            if (self.IsPlayer())
                checkOverViewSize += 1;

            /*
             *  □ □ □ □ □
             *  □ ● ● ● □
             *  □ ● ■ ● □  // ■=当前实体, ●=hsVisibleAreaCells, □=hsInvisibleAreaCells
             *  □ ● ● ● □
             *  □ □ □ □ □     */
            int minX = selfCellX - checkOverViewSize;
            int maxX = selfCellX + checkOverViewSize;
            int minY = selfCellY - checkOverViewSize;
            int maxY = selfCellY + checkOverViewSize;
            long areaCellId = 0;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    areaCellId = AreaCellHelper.GetACIdByAOIPos(x, y);

                    hsLeaveNeedCheck.Add(areaCellId);
                    //超出视野范围的大小 都不加入
                    if (x < selfCellX - viewCellSize || x < selfCellX + viewCellSize
                        || y < selfCellY - viewCellSize || y < selfCellY + viewCellSize)
                    {
                        continue;
                    }

                    hsVisible.Add(areaCellId);
                }
            }
        }
        /// <summary>
        ///  重置当前可视与不可视的区域格子
        /// </summary>
        /// <param name="self"></param>
        /// <param name="selfCellX"></param>
        /// <param name="selfCellY"></param>
        public static void ResetVisibleAndLeveCheckAreaCells(this AreaOfInterestEntity self, int selfCellX, int selfCellY)
        {
            self._ResetVisibleAndLeveCheckAreaCells(selfCellX, selfCellY, ref self.hsVisibleAreaCells, ref self.hsLeaveNeedCheckAreaCells);
        }
        /// <summary>
        ///  重置临时当前可视与不可视的区域格子（给临时容器，做对比使用的）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="selfCellX"></param>
        /// <param name="selfCellY"></param>
        public static void ResetTmpVisibleAndLeveCheckAreaCells(this AreaOfInterestEntity self, int selfCellX, int selfCellY)
        {
            self._ResetVisibleAndLeveCheckAreaCells(selfCellX, selfCellY, ref self.hsTmpVisibleAreaCells, ref self.hsTmpLeaveNeedCheckAreaCells);
        }
        /// <summary>
        /// AOI关联这个可视区域
        /// </summary>
        /// <param name="aoi"></param>
        /// <param name="cell"></param>
        public static void LinkToVisibleAreaCell(this AreaOfInterestEntity self, AreaCell cell)
        {
            //给这个区域AreaCell进行赋值添加
            cell.dicAOIUnitsVisibleSelf.Add(self.Id, self);

            //给当前aoi进行赋值添加
            foreach (AreaOfInterestEntity acAOI in cell.dicAOIUnits.Values)
            {
                if (acAOI.Id != self.Id)
                    self.AddVisibleOtherAOI(acAOI);
            }
        }
        /// <summary>
        ///  AOI取消关联这个可视区域
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cell"></param>
        public static void UnLinkToVisibleAreaCell(this AreaOfInterestEntity self, AreaCell cell)
        {
            cell.dicAOIUnitsVisibleSelf.Remove(self.Id);
        }
        public static void AddVisibleOtherAOI(this AreaOfInterestEntity self, AreaOfInterestEntity other)
        {
            //检测otherAoi已经被销毁了
            if (other == null) return;
            //可视区域已经添加过这个aoi
            if (self.dicVisibleAOIUnits.ContainsKey(other.Id))
                return;

            //tips:可加入两个AOI之间的可视野检测，other可不可以被这个self看到 ，不可被看到的return

            //当前的aoi 可是unit加入这个aoi
            self.dicVisibleAOIUnits.Add(other.Id, other);
            if (other.IsPlayer())
                self.dicVisibleAOIPlayers.Add(other.Id, other);

            //other 可以看到这个aoi
            other.dicOtherAOIUnitsVisibleSelf.Add(self.Id, self);
            if (self.IsPlayer())
                other.dicOtherPlayersVisibleSelf.Add(self.Id, self);
            EventSystem.Instance.PublishAsync(self.Scene(), new UnitAOIAVisibleBEvent() { A = self, B = other }).Coroutine();
        }

        /// <summary>
        /// 连接离开的时候需要检查的格子
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cell"></param>
        public static void LinkToLeaveNeedCheckAreaCell(this AreaOfInterestEntity self, AreaCell cell)
        {
            //给这个区域AreaCell进行赋值添加
            cell.dicAOILeaveNeedCheckSelf.Add(self.Id, self);
        }
        
  
        
        
        /// <summary>
        /// 取消链接离开的时候需要检查的格子
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cell"></param>
        public static void UnLinkToLeaveNeedCheckAreaCell(this AreaOfInterestEntity self, AreaCell cell)
        {
            AreaOfInterestEntity otherAoi = null;
            //遍历这些自己能够看到的格子，遍历他的aoi，并给客户端的自己下发通知自己能看到的aoi 需要被移除掉了。（因为自己的离开的关系，他们在自己中看不到了）
            foreach (var aoi in cell.dicAOIUnits.Values)
            {
                otherAoi = aoi;
                if (otherAoi.Id != self.Id)
                    self.RemoveVisibleOtherAOI(otherAoi);
            }
            cell.dicAOILeaveNeedCheckSelf.Remove(self.Id);
        }

        /// <summary>
        /// 移除可视野看到的aoi单位
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        public static void RemoveVisibleOtherAOI(this AreaOfInterestEntity self, AreaOfInterestEntity other)
        {
            //检测otherAoi已经被销毁了
            if (other == null) return;
            //可视区域已经添加过这个aoi
            if (self.dicVisibleAOIUnits.ContainsKey(other.Id))
                return;

            //tips:可加入两个AOI之间的可视野检测，other可不可以被这个self看到 ，不可被看到的return

            //当前的aoi 可是unit加入这个aoi
            self.dicVisibleAOIUnits.Remove(other.Id);
            if (other.IsPlayer())
                self.dicVisibleAOIPlayers.Remove(other.Id);

            //other 可以看到这个aoi
            other.dicOtherAOIUnitsVisibleSelf.Remove(self.Id);
            if (self.IsPlayer())
                other.dicOtherPlayersVisibleSelf.Remove(self.Id);
            EventSystem.Instance.PublishAsync(self.Scene(), new UnitAOIAInVisibleBEvent() { A = self, B = other }).Coroutine();
        }
    }
}