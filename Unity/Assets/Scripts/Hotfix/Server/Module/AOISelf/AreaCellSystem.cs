using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(AreaCell))]
    [FriendOf(typeof(AreaCell))]
    public static partial class AreaCellSystem
    {
        [EntitySystem]
        private static void Awake(this AreaCell self)
        {
        }

        [EntitySystem]
        private static void Destroy(this AreaCell self)
        {
        }

        public static void AddAOI(this AreaCell self, AreaOfInterestEntity aoi)
        {
            self.dicAOIUnits.Add(aoi.Id, aoi);
        }
        public static void RemoveAOI(this AreaCell self, AreaOfInterestEntity aoi)
        {
            self.dicAOIUnits.Remove(aoi.Id);
        }
    }
}