namespace ET
{
    public static class LSConstValue
    {
        /// <summary>
        /// 匹配的人数
        /// </summary>
        public const int MatchCount = 1;
        /// <summary>
        /// 更新间隔 毫秒
        /// </summary>
        public const int UpdateInterval = 50;
        /// <summary>
        /// 每秒帧数
        /// </summary>
        public const int FrameCountPerSecond = 1000 / UpdateInterval;
        public const int SaveLSWorldFrameCount = 60 * FrameCountPerSecond;
    }
}