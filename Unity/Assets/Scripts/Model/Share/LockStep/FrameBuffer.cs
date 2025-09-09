using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    /// <summary>
    /// 帧缓冲区（用于存储帧数据、快照、哈希值）
    /// 典型应用场景：帧同步游戏中，每一帧需要保存玩家输入、游戏状态快照以及校验用的哈希值
    /// </summary>
    public class FrameBuffer: Object
    {
        // 当前能保存的最大帧号（动态向前推进）
        public int MaxFrame { get; private set; }

        // 每一帧的输入集合（例如：玩家操作指令）网络帧消息 (一帧消息存储的当前所有玩家的操作输入)
        private readonly List<OneFrameInputs> frameInputs;

        // 每一帧的游戏状态快照（序列化的内存数据）
        private readonly List<MemoryBuffer> snapshots;

        // 每一帧对应的哈希值（用于帧校验、防止不同步） idex表示帧号，存储的是真快照的 字节流数据的缓存。byte[]得到该帧的哈希值。该帧输入的唯一值。
        private readonly List<long> hashs;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="frame">起始帧号</param>
        /// <param name="capacity">缓存容量（默认60秒 * 每秒帧数）  1分钟所有帧数</param>
        public FrameBuffer(int frame = 0, int capacity = LSConstValue.FrameCountPerSecond * 60)
        {
            // 初始最大帧号（默认给30秒的范围，最多给 当前帧的后半分钟的帧数）
            this.MaxFrame = frame + LSConstValue.FrameCountPerSecond * 30;

            this.frameInputs = new List<OneFrameInputs>(capacity);
            this.snapshots = new List<MemoryBuffer>(capacity);
            this.hashs = new List<long>(capacity);
            
            // 预先分配好所有缓冲区，避免运行时GC压力
            for (int i = 0; i < this.snapshots.Capacity; ++i)
            {
                this.hashs.Add(0); // 初始化哈希值
                this.frameInputs.Add(OneFrameInputs.Create()); // 创建输入对象

                // 初始化每帧的快照内存缓冲区（默认 10KB）
                MemoryBuffer memoryBuffer = new(10240);
                // 把长度设置为0 ，如果是对象池中哪的可能lenth没有被重置
                memoryBuffer.SetLength(0);
                //把游标重置到开头  防止MemoryBuffer从对象池中那
                memoryBuffer.Seek(0, SeekOrigin.Begin);
                this.snapshots.Add(memoryBuffer);
            }
        }

        /// <summary>
        /// 设置某一帧的哈希值（用于校验一致性）
        /// </summary>
        public void SetHash(int frame, long hash)
        {
            EnsureFrame(frame);
            this.hashs[frame % this.frameInputs.Capacity] = hash;
        }
        
        /// <summary>
        /// 获取某一帧的哈希值
        /// </summary>
        public long GetHash(int frame)
        {
            EnsureFrame(frame);
            return this.hashs[frame % this.frameInputs.Capacity];
        }

        /// <summary>
        /// 检查某一帧是否合法（不能小于0，不能超过最大帧）
        /// </summary>
        public bool CheckFrame(int frame)
        {
            if (frame < 0)
            {
                return false;
            }

            if (frame > this.MaxFrame)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 确保帧合法，否则抛异常
        /// </summary>
        private void EnsureFrame(int frame)
        {
            if (!CheckFrame(frame))
            {
                throw new Exception($"frame out: {frame}, maxframe: {this.MaxFrame}");
            }
        }
        
        /// <summary>
        /// 获取某一帧的输入（玩家操作）
        /// </summary>
        public OneFrameInputs FrameInputs(int frame)
        {
            EnsureFrame(frame);
            OneFrameInputs oneFrameInputs = this.frameInputs[frame % this.frameInputs.Capacity];
            return oneFrameInputs;
        }

        /// <summary>
        /// 推进最大帧（如果快接近上限了，就扩展1帧）
        /// </summary>
        public void MoveForward(int frame)
        {
            // 如果最大帧号距离当前帧还大于1秒，则不需要扩展
            if (this.MaxFrame - frame > LSConstValue.FrameCountPerSecond) 
            {
                return;
            }
            
            // 推进一帧
            ++this.MaxFrame;
            
            // 清理该帧对应的输入数据（为新帧复用）
            OneFrameInputs oneFrameInputs = this.FrameInputs(this.MaxFrame);
            oneFrameInputs.Inputs.Clear();
        }

        /// <summary>
        /// 获取某一帧的状态快照
        /// </summary>
        public MemoryBuffer Snapshot(int frame)
        {
            EnsureFrame(frame);
            MemoryBuffer memoryBuffer = this.snapshots[frame % this.snapshots.Capacity];
            return memoryBuffer;
        }
    }
}
