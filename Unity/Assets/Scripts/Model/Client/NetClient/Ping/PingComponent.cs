namespace ET.Client
{
    [ComponentOf(typeof(Session))]
    public class PingComponent: Entity, IAwake, IDestroy
    {
        public long Ping { get; set; } //延迟值
        
        /// <summary>
        /// 普通的心跳包的ping的间隔时间
        /// </summary>
        public  long HeartIntervalTime = 2000;
        

        /// <summary>
        /// 帧同步的ping的间隔时间
        /// </summary>
        public  long FrameSyncIntervalTime = 1000;
        
        
        ///<summary>客户端最大超前服务端10帧</summary>
        public int AheadOfFrameMax = 1000;
                
        ///<summary>延迟帧数</summary>
        public int TargetAheadOfFrame;
        
        ///<summary>客户端帧数</summary>
        public int CurrentFrame = -1;
        ///<summary>服务器帧数</summary>
        public int ServerCurrentFrame;
        ///<summary>客户端超前帧数</summary>
        public int CurrentAheadOfFrame;

    }
}