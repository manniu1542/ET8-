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
        
        
        


    }
}