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

            aoi.ResetVisibleAndInVisibleAreaCells(acX, acY);
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

            //通知订阅该Cell的,那些可以看到该Cell的AOI,广播给他消息，其他的AOI也设置看到了该aoi的广播，相互关联上
            AreaOfInterestEntity otherAoi = null;
            foreach (var kv in ac.dicAOIUnitsVisibleSelf)
            {
                otherAoi = kv.Value;
                if (otherAoi.Id != aoi.Id)
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
            //通知能够看到自己的aoi，现在都看不到自己
            AreaOfInterestEntity otherAoi = null;
            AreaCell ac = null;
            //取消关联aoi与ac
            aoi.Cell.RemoveAOI(aoi);
            aoi.Cell = null;
            foreach (long cellId in aoi.hsLeaveNeedCheckAreaCells)
            {
                ac = self.GetOrCreateAreaCell(cellId);
                ac.dicAOILeaveNeedCheckSelf
                
            }
       
        }
    }
}