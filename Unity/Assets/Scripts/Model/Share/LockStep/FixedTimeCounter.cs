namespace ET
{
    public class FixedTimeCounter: Object
    {
        /// <summary>
        /// 本剧游戏开始的服务器时间
        /// </summary>
        private long startTime;
        /// <summary>
        /// 本局游戏开始时服务器的帧数
        /// </summary>
        private int startFrame;
        /// <summary>
        /// 帧间隔时间 大约50毫秒，间隔1秒钟 服务器会根据客户端的运行调整一次
        /// </summary>
        public int Interval { get; private set; }

        public FixedTimeCounter(long startTime, int startFrame, int interval)
        {
            this.startTime = startTime;
            this.startFrame = startFrame;
            this.Interval = interval;
        }
        
        public void ChangeInterval(int interval, int frame)
        {
            this.startTime += (frame - this.startFrame) * this.Interval;
            this.startFrame = frame;
            this.Interval = interval;
        }
        /// <summary>
        /// 获取指定帧的时间
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public long FrameTime(int frame)
        {
            return this.startTime + (frame - this.startFrame) * this.Interval;
        }
        
        public void Reset(long time, int frame)
        {
            this.startTime = time;
            this.startFrame = frame;
        }
    }
}