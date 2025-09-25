using TrueSync;

namespace ET
{
    public static class LSConstValue
    {
        /// <summary>
        /// 匹配的人数
        /// </summary>
        public const int MatchCount = 2;
        
        /// <summary>
        /// 最大领先帧数
        /// </summary>
        public const int MaxAheadOfFrameCount = 1000;
        
        /// <summary>
        /// 更新间隔 毫秒
        /// </summary>
        public const int MinUpdateInterval = 35;
        
        /// <summary>
        /// 最大间隔 毫秒
        /// </summary>
        public const int MaxUpdateInterval = 80;
        
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


    }
}