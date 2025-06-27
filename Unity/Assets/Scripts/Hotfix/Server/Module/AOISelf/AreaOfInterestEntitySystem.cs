using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(AreaOfInterestEntity))]
    [FriendOf(typeof(AreaOfInterestEntity))]
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
            self.dicInvisibleAOIPlayers.Clear();
            self.dicInvisibleAOIUnits.Clear();
            self.dicVisibleAOIPlayers.Clear();
            self.dicVisibleAOIUnits.Clear();
            self.hsInvisibleAreaCells.Clear();
            self.hsVisibleAreaCells.Clear();

            self.dicInvisibleAOIPlayers = null;
            self.dicInvisibleAOIUnits = null;
            self.dicVisibleAOIPlayers = null;
            self.dicVisibleAOIUnits = null;
            self.hsInvisibleAreaCells = null;
            self.hsVisibleAreaCells = null;
        }
        
        
        
    }
}