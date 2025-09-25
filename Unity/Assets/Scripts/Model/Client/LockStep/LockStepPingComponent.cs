namespace ET.Client
{
    
    [ComponentOf(typeof(Room))]
    public class LockStepPingComponent : Entity, IAwake, IDestroy
    {
        public long Ping { get; set; } //延迟值

        /// <summary>
        /// 帧同步的ping的间隔时间
        /// </summary>
        public long FrameSyncIntervalTime = 1000;


        
        

    }
}