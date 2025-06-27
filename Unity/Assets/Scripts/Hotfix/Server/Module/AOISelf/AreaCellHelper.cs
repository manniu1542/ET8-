namespace ET.Server
{
    [FriendOf(typeof(AreaCell))]
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
            //如果int是负数，也没有关系，uint会把负数转成正数，-1就是 uint.MaxValue最大值，从高到底排序。也可能重复，但可能性较低
            //先转成uint，这样x还是32位的。在转成ulong此时，x变为64位了里面存储的还是低位32位，左移 32位，得到64位id
            long idX = ((long)((uint)x)) << 32;
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
            x = (int)((ulong)acID >> 32);
            y = (int)(acID & 0xffffffff);
        }
    }
}