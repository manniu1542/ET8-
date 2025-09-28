using System.Collections.Generic;

namespace ET
{
    [ComponentOf]
    public class Room: Entity, IScene, IAwake, IUpdate
    {
        /// <summary>
        /// 继承了IScene接口。在Room被Add的时候因为他是IScene类型，会在Entity上给Fiber赋值的。并且把当前的纤程场景给到Rooom的Fiber属性中。
        /// </summary>
        public Fiber Fiber { get; set; }
        public SceneType SceneType { get; set; } = SceneType.Room;
        public string Name { get; set; }
        
        public long StartTime { get; set; }

        // 帧缓存(玩家操作输入的网络帧消息,帧快照 网络帧消息的字节流缓存, 字节流缓存的哈希值)  
        public FrameBuffer FrameBuffer { get; set; }

        // 计算fixedTime，fixedTime在客户端是动态调整的，会做时间膨胀缩放
        public FixedTimeCounter FixedTimeCounter { get; set; }

        // 玩家id列表
        public List<long> PlayerIds { get; } = new(LSConstValue.MatchCount);
    
        /// <summary>
        /// 最多预测几帧 
        /// </summary>
        public int MaxPredictionCount { get; set; } =  LSConstValue.DefaultMaxPredictionCount;
        // 预测帧 （客户端执行的帧）
        public int PredictionFrame { get; set; } = -1;

        // 权威帧 (服务器执行的，客户端接收到服务器的)
        public int AuthorityFrame { get; set; } = -1;

        // 存档
        public Replay Replay { get; set; } = new();

        /// <summary>
        /// 帧同步世界
        /// </summary>
        private EntityRef<LSWorld> lsWorld;

        // LSWorld做成child，可以有多个lsWorld，比如守望先锋有两个
        public LSWorld LSWorld
        {
            get
            {
                return this.lsWorld;
            }
            set
            {
                //这一步就确定Room的fiber纤程。跟LSWord的fiber是同一个，以及他们的Scene也是不同的。到时候发消息给Room或着给LSWord的时候，都会走这个fiber的。
                this.AddChild(value);
                this.lsWorld = value;
            }
        }

        public bool IsReplay { get; set; }
        
        public int SpeedMultiply { get; set; }
    }
}