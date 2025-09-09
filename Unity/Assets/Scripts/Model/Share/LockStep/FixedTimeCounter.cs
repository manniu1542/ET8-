namespace ET
{
    // FixedTimeCounter 用于固定时间间隔的计时器（例如帧同步中的时间计算）
    public class FixedTimeCounter: Object
    {
        // 起始时间（单位可以是毫秒、微秒等，根据使用场景）
        private long startTime;

        // 起始帧
        private int startFrame;

        // 时间间隔（每帧的固定时间长度）
        public int Interval { get; private set; }

        // 构造函数，初始化计时器
        public FixedTimeCounter(long startTime, int startFrame, int interval)
        {
            this.startTime = startTime;   // 起始时间
            this.startFrame = startFrame; // 起始帧
            this.Interval = interval;     // 每帧时间间隔
        }
        
        // 修改计时器间隔，并同步调整起始时间
        // interval: 新的时间间隔
        // frame: 当前帧数
        public void ChangeInterval(int interval, int frame)
        {
            // 调整起始时间，使得新间隔计算不会跳帧或产生偏差
            this.startTime += (frame - this.startFrame) * this.Interval;

            // 更新起始帧和新的时间间隔
            this.startFrame = frame;
            this.Interval = interval;
        }

        // 计算指定帧对应的时间
        // frame: 要计算时间的帧
        public long FrameTime(int frame)
        {
            // 公式：起始时间 + (目标帧 - 起始帧) * 间隔
            return this.startTime + (frame - this.startFrame) * this.Interval;
        }
        
        // 重置计时器
        // time: 新的起始时间
        // frame: 新的起始帧
        public void Reset(long time, int frame)
        {
            this.startTime = time;
            this.startFrame = frame;
        }
    }
}