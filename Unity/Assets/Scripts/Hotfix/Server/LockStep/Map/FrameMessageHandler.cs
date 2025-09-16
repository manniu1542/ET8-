using System;
using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 收到客户端的帧消息 处理
    /// </summary>
    [MessageHandler(SceneType.RoomRoot)]
    public class FrameMessageHandler : MessageHandler<Scene, FrameMessage>
    {
        protected override async ETTask Run(Scene root, FrameMessage message)
        {
            using FrameMessage _ = message; // 让消息回到池中

            Room room = root.GetComponent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            AdjustUpdateTime(room, message);
            // if (message.Frame % (1000 / LSConstValue.UpdateInterval) == 0)
            // {
            //     // 当前帧的理论时间
            //     long nowFrameTime = room.FixedTimeCounter.FrameTime(message.Frame);
            //     // 当前帧时间与服务器真实帧时间的差值（单位毫秒）
            //     int diffTime = (int)(nowFrameTime - TimeInfo.Instance.ServerFrameTime());
            //
            //     Room2C_AdjustUpdateTime room2CAdjustUpdateTime = Room2C_AdjustUpdateTime.Create();
            //     room2CAdjustUpdateTime.DiffTime = diffTime;
            //     room.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(message.PlayerId, room2CAdjustUpdateTime);
            // }
            if (message.Frame < room.AuthorityFrame) // 小于AuthorityFrame，丢弃  过时 → 丢弃   
            {
                Log.Warning($"FrameMessage < AuthorityFrame discard: {message}");
                return;
            }

            if (message.Frame > room.AuthorityFrame + 10) // 大于AuthorityFrame + 10，丢弃  超前太多 → 丢弃
            {
                Log.Warning($"FrameMessage > AuthorityFrame + 10 discard: {message}");
                return;
            }

            //获取到当前帧的缓存输入。 覆盖掉自己的输入
            OneFrameInputs oneFrameInputs = frameBuffer.FrameInputs(message.Frame);
            if (oneFrameInputs == null)
            {
                Log.Error($"FrameMessageHandler get frame is null: {message.Frame}, max frame: {frameBuffer.MaxFrame}");
                return;
            }

            oneFrameInputs.Inputs[message.PlayerId] = message.Input;

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 使客户端的刷新频率 趋近于 服务端的 刷新频率  ,服务端就是完全理想频率再刷新 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="message"></param>
        public static void AdjustUpdateTime(Room room, FrameMessage message)
        {
            //1秒钟 做一次 客户端的时间矫正。让客户端的刷新率 同步到跟服务端 刷新率 大致相同，让客户端追帧 或者是减缓 每帧的刷新
            int timeSync = 1000;
            //在这么timeSync时间中运行了多少帧。 多久让客户端刷新率 趋向服务端 刷新率
            int syncFrameCount = timeSync / LSConstValue.UpdateInterval;
            if (message.Frame % syncFrameCount == 0)
            {
                //服务器的当前帧时间
                long nowFrameTime = room.FixedTimeCounter.FrameTime(message.Frame);
                // 当前帧时间与服务器真实帧时间的差值（单位毫秒）
                int diffTime = (int)(nowFrameTime - TimeInfo.Instance.ServerFrameTime());

                Room2C_AdjustUpdateTime room2CAdjustUpdateTime = Room2C_AdjustUpdateTime.Create();
                room2CAdjustUpdateTime.DiffTime = diffTime;
                room.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession)
                        .Send(message.PlayerId, room2CAdjustUpdateTime);
            }
        }
    }
}