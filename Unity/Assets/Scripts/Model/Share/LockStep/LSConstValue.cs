namespace ET
{
    public static class LSConstValue
    {
        /// <summary>
        /// 匹配的人数
        /// </summary>
        public const int MatchCount = 1;
   
        /// <summary>
        /// 最大领先帧数 调整更新间隔使用
        /// </summary>
        public const int MaxAheadOfFrameCount = 10;
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

        /// <summary>
        /// 默认最多预测帧   格斗类2帧， 多人在线的话就 5-8帧
        /// </summary>
        public const int DefaultMaxPredictionCount = 2;
        
        /// <summary>
        /// 运行对齐帧数  帧同步开始先对其帧数(客户端不输入具体内容，就不会导致回滚问题，先稳定运行一定时间)， 对齐帧数后开始正常帧同步
        /// </summary>
        public const int RunAlignmentFrames = 100;


    }
}