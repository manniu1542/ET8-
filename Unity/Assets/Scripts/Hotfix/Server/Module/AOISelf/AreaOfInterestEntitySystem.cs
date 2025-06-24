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



        }
        [EntitySystem]
        private static void Destroy(this AreaOfInterestEntity self)
        {

        }


    }
}