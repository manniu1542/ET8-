using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(AreaCellMgrComponent))]
    [FriendOf(typeof(AreaCellMgrComponent))]
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
            

     
            
        }

        /// <summary>
        /// 取消 AOI与AreaCell绑定
        /// </summary>
        /// <param name="self"></param>
        /// <param name="areaCell"></param>
        public static void UnBindAOIFormAreaCell(this AreaCellMgrComponent self, AreaOfInterestEntity aoi)
        {
        }
    }
}