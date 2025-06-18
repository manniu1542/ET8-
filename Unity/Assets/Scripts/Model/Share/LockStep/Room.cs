using System.Collections.Generic;

namespace ET
{
    [ComponentOf]
    public class Room: Entity, IScene, IAwake, IUpdate
    {
        public Fiber Fiber { get; set; }
        public SceneType SceneType { get; set; } = SceneType.Room;
        public string Name { get; set; }
        
        public long StartTime { get; set; }

        // 帧缓存
        public FrameBuffer FrameBuffer { get; set; }

        // 计算fixedTime，fixedTime在客户端是动态调整的，会做时间膨胀缩放
        public FixedTimeCounter FixedTimeCounter { get; set; }

        // 玩家id列表
        public List<long> PlayerIds { get; } = new(LSConstValue.MatchCount);
        
        // 预测帧（都是客户端运行跑的比服务端快的帧消息，因为客户端跟服务端之间有消息回传的间隔，所以客户端提前记录的帧消息）
        public int PredictionFrame { get; set; } = -1;

        // 权威帧（确定帧）
        public int AuthorityFrame { get; set; } = -1;

        // 存档
        public Replay Replay { get; set; } = new();

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
                this.AddChild(value);
                this.lsWorld = value;
            }
        }

        public bool IsReplay { get; set; }
        
        public int SpeedMultiply { get; set; }
    }
}