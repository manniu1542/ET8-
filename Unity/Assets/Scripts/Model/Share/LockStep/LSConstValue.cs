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
        /// <summary>
        /// 间隔几帧保存下帧同步世界的帧快照
        /// </summary>
        public const int SaveLSWorldFrameCount = 60 * FrameCountPerSecond;
        /// <summary>
        /// 动态的更新间隔
        /// </summary>
        public static int DynamicUpdateInterval = UpdateInterval;

        /// <summary>
        /// 是否开启了帧同步
        /// </summary>
        public static bool IsStartFrameSync = false;
    }
}