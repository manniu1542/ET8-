using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    [FriendOf(typeof(Room))]
    public static partial class RoomSystem
    {
        public static Room Room(this Entity entity)
        {
            return entity.IScene as Room;
        }

        public static void Init(this Room self, List<LockStepUnitInfo> unitInfos, long startTime, int frame = -1)
        {
            //初始 开始时间，初始帧数，
            self.StartTime = startTime;
            self.AuthorityFrame = frame;
            self.PredictionFrame = frame;
            self.Replay.UnitInfos = unitInfos;
            //帧缓存的（运行时缓存，数据快照，比对哈希）
            self.FrameBuffer = new FrameBuffer(frame);
            //让同步更加平缓的计时器
            self.FixedTimeCounter = new FixedTimeCounter(self.StartTime, 0, LSConstValue.UpdateInterval);
            //初始化同步世界
            LSWorld lsWorld = self.LSWorld;
            lsWorld.Frame = frame + 1;
            //同步世界添加同步Unit的组件
            lsWorld.AddComponent<LSUnitComponent>();
            //玩家网络信息，开始初始化玩家
            for (int i = 0; i < unitInfos.Count; ++i)
            {
                LockStepUnitInfo unitInfo = unitInfos[i];
                LSUnitFactory.Init(lsWorld, unitInfo);
                self.PlayerIds.Add(unitInfo.PlayerId);
            }
        }

        /// <summary>
        /// 被帧同步的同步器所调用，获取网络输入的消息来驱动玩家表现。获取帧同步世界。帧同步世界下面的Unit管理组件。获取每个玩家的输入组件。输入网络消息驱动
        /// </summary>
        /// <param name="self"></param>
        /// <param name="oneFrameInputs"></param>
        public static void Update(this Room self, OneFrameInputs oneFrameInputs)
        {
            LSWorld lsWorld = self.LSWorld;
            // 设置输入到每个LSUnit身上
            LSUnitComponent unitComponent = lsWorld.GetComponent<LSUnitComponent>();
            foreach (var kv in oneFrameInputs.Inputs)
            {
                LSUnit lsUnit = unitComponent.GetChild<LSUnit>(kv.Key);
                LSInputComponent lsInputComponent = lsUnit.GetComponent<LSInputComponent>();
                lsInputComponent.LSInput = kv.Value;
            }

            if (!self.IsReplay)
            {
                // 保存当前帧场景数据
                self.SaveLSWorld();
                self.Record(self.LSWorld.Frame);
            }

            lsWorld.Update();

            foreach (var kv in oneFrameInputs.Inputs)
            {
                LSUnit lsUnit = unitComponent.GetChild<LSUnit>(kv.Key);
                Log.LockStepWarning($"帧:{lsWorld.Frame},玩家{lsUnit.Id},位置{lsUnit.Position},旋转{lsUnit.Rotation}");
            }
        }

        /// <summary>
        /// 获取某一帧的 场景数据
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sceneType"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static LSWorld GetLSWorld(this Room self, SceneType sceneType, int frame)
        {
            MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            LSWorld lsWorld = MemoryPackHelper.Deserialize(typeof(LSWorld), memoryBuffer) as LSWorld;
            lsWorld.SceneType = sceneType;
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            return lsWorld;
        }

        /// <summary>
        /// 保存当前帧的所有输入汇总的哈希值到 帧缓存中
        /// </summary>
        /// <param name="self"></param>
        private static void SaveLSWorld(this Room self)
        {
            int frame = self.LSWorld.Frame;
            //取得对应帧的内存缓存区
            MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
            //充值缓存区的位置以及长度
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);

            MemoryPackHelper.Serialize(self.LSWorld, memoryBuffer);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            //计算这个缓存区的哈希值
            long hash = memoryBuffer.GetBuffer().Hash(0, (int)memoryBuffer.Length);

            self.FrameBuffer.SetHash(frame, hash);
        }

        // 记录需要存档的数据 ， 记录某一帧的所有操作数据
        public static void Record(this Room self, int frame)
        {
            if (frame > self.AuthorityFrame)
            {
                return;
            }

            //每一帧的输入都被记录
            OneFrameInputs oneFrameInputs = self.FrameBuffer.FrameInputs(frame);
            OneFrameInputs saveInput = OneFrameInputs.Create();
            oneFrameInputs.CopyTo(saveInput);
            self.Replay.FrameInputs.Add(saveInput);
            // 间隔一定的帧。记录帧快照的 数据再回放时使用
            if (frame % LSConstValue.SaveLSWorldFrameCount == 0)
            {
                MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
                byte[] bytes = memoryBuffer.ToArray();
                self.Replay.Snapshots.Add(bytes);
            }
        }
    }
}