using System.Collections.Generic;
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
            self.hsInvisibleAreaCells.Clear();
            self.hsVisibleAreaCells.Clear();

            self.dicOtherPlayersVisibleSelf = null;
            self.dicOtherAOIUnitsVisibleSelf = null;
            self.dicVisibleAOIPlayers = null;
            self.dicVisibleAOIUnits = null;
            self.hsInvisibleAreaCells = null;
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
        public static void ResetVisibleAndInVisibleAreaCells(this AreaOfInterestEntity self, int selfCellX, int selfCellY)
        {
            self.hsVisibleAreaCells.Clear();
            self.hsInvisibleAreaCells.Clear();

            if (self.ViewDistance <= 0)
                self.ViewDistance = 1;
            //看到格子大小的尺寸 限制 （至少是1,取值总是取ceil向上取整了）
            int viewCellSizeLimit = (self.ViewDistance - 1) / AreaCellMgrComponent.FloatToIntConversionFactor + 1;

            //玩家可见格子尺寸,增加1 避免频繁增删（增加客户端渲染压力）。
            if (self.IsPlayer())
                viewCellSizeLimit += 1;

            /*
             *  □ □ □ □ □
             *  □ ● ● ● □
             *  □ ● ■ ● □  // ■=当前实体, ●=hsVisibleAreaCells, □=hsInvisibleAreaCells
             *  □ ● ● ● □
             *  □ □ □ □ □     */
            int minX = selfCellX - viewCellSizeLimit;
            int maxX = selfCellX + viewCellSizeLimit;
            int minY = selfCellY - viewCellSizeLimit;
            int maxY = selfCellY + viewCellSizeLimit;
            long areaCellId = 0;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    areaCellId = AreaCellHelper.GetACIdByAOIPos(x, y);

                    //处于边界的格子区域，统统加入不可看的格子 TODO:为什么他写的是这里面所有格子都要加入不可看到的区域呢？
                    if (x >= minX || x <= maxX || y >= minY || y <= maxY)
                    {
                        self.hsInvisibleAreaCells.Add(areaCellId);
                    }
                    else
                    {
                        self.hsVisibleAreaCells.Add(areaCellId);
                    }
                }
            }
        }

        /// <summary>
        /// 添加可视区域
        /// </summary>
        /// <param name="aoi"></param>
        /// <param name="cell"></param>
        public static void AddVisibleAreaCell(this AreaOfInterestEntity self, AreaCell cell)
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
        /// 添加不可视区域
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cell"></param>
        public static void AddInVisibleAreaCell(this AreaOfInterestEntity self, AreaCell cell)
        {
            //给这个区域AreaCell进行赋值添加
            cell.dicAOIUnitsInvisibleSelf.Add(self.Id, self);
            
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