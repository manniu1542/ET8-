namespace ET.Server
{
    [FriendOf(typeof(AreaCell))]
    [FriendOfAttribute(typeof(ET.Server.AreaOfInterestEntity))]
    public static class AreaCellHelper
    {
        /// <summary>
        /// 获取AreaCell 的id 通过 坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static long GetACIdByAOIPos(int x, int y)
        {
            //高32位保留x，及时x是负数。也是高32位的负数。他左移后。地位32 始终是 0，影响不了y的值。
            long idX = (long)x << 32;
            //y必须是uint,否则 在进行 位于运算的|时候。 y会默认转成long。y是负数。那么 高位就被 影响了。
            return (idX | (uint)y);
        }

        /// <summary>
        /// 通过AreaCell id获取 AreaCell的中心点位置
        /// </summary>
        /// <param name="acID"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void GetACMiddlePosByACId(long acID, out int x, out int y)
        {
            x = (int)(acID >> 32);
            y = (int)(acID & 0xffffffff);
        }

    

    
    }
}