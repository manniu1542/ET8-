using System;
using System.IO;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSClientUpdater))]
    [FriendOf(typeof(LSClientUpdater))]
    public static partial class LSClientUpdaterSystem
    {
        [EntitySystem]
        private static void Awake(this LSClientUpdater self)
        {
            Room room = self.GetParent<Room>();
            self.MyId = room.Root().GetComponent<PlayerComponent>().MyId;
        }

        [EntitySystem]
        private static void Update(this LSClientUpdater self)
        {
            Room room = self.GetParent<Room>();
            long timeNow = TimeInfo.Instance.ServerNow();
            Scene root = room.Root();

            int i = 0;
            while (true)
            {
                // 如果当前时间小于预测帧的帧时间，就跳出
                if (timeNow < room.FixedTimeCounter.FrameTime(room.PredictionFrame + 1))
                {
                    return;
                }

                // 预测帧 如果 比实际 小的时候。就继续运行。
                if (room.PredictionFrame - room.AuthorityFrame > room.MaxPredictionCount)
                {
                    return;
                }

                ++room.PredictionFrame;
                OneFrameInputs oneFrameInputs = self.GetOneFrameMessages(room.PredictionFrame);
                //根据这一帧的输入数据。来驱动 帧同步逻辑（房间的 数据缓存，以及帧同步世界的玩家 逻辑表现）
                room.Update(oneFrameInputs);
                //发送这一帧的哈希值比对到服务器（让服务器来校验这一帧 ，客户的输入是否有问题）
                room.SendHash(room.PredictionFrame);

                room.SpeedMultiply = ++i;
                //这一帧的玩家输入消息，发给服务器
                FrameMessage frameMessage = FrameMessage.Create();
                frameMessage.Frame = room.PredictionFrame;
                frameMessage.Input = self.Input;
             
                root.GetComponent<ClientSenderComponent>().Send(frameMessage);
                Log.Error($"客户端：发送{room.PredictionFrame}输入消息：{self.Input}");
                // 如果处理超过 5 毫秒，就先跳出，避免一帧内处理太久，避免让客户端根服务端差距太大。导致客户端一直回滚数据，回滚数据过大导致客户端卡死
                long timeNow2 = TimeInfo.Instance.ServerNow();
                if (timeNow2 - timeNow > 5)
                {
                    break;
                }
            }
        }

        private static OneFrameInputs GetOneFrameMessages(this LSClientUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            //如果这一帧是确定帧的话。就直接那一帧的输入消息
            if (frame <= room.AuthorityFrame)
            {
                return frameBuffer.FrameInputs(frame);
            }

            // predict  获取预测帧的输入数据，只把当前自己输入丢入进去
            OneFrameInputs predictionFrame = frameBuffer.FrameInputs(frame);
            // 推进 FrameBuffer 到当前帧（准备好数据结构）
            frameBuffer.MoveForward(frame);

            if (frameBuffer.CheckFrame(room.AuthorityFrame))
            {
                OneFrameInputs authorityFrame = frameBuffer.FrameInputs(room.AuthorityFrame);
                authorityFrame.CopyTo(predictionFrame);
            }

            predictionFrame.Inputs[self.MyId] = self.Input;

            return predictionFrame;
        }
    }
}