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
        

        
        public bool IsUsing = true;
        
        
        
        
        
        
  

    }
}