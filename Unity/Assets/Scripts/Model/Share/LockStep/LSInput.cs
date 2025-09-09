using System;
using MemoryPack;

namespace ET
{
    /// <summary>
    ///  一帧的输入      标记为 MemoryPack 可序列化结构体（方便网络传输/存档）
    /// </summary>
    [MemoryPackable]
    public partial struct LSInput
    {
        // 玩家输入的方向（二维向量，例如摇杆方向）
        [MemoryPackOrder(0)]
        public TrueSync.TSVector2 V;

        // 玩家输入的按键（整数表示，例如 1=跳跃，2=攻击）
        [MemoryPackOrder(1)]
        public int Button;

        // 判断是否等于另一个 LSInput（同时比较向量和按键）
        public bool Equals(LSInput other)
        {
            return this.V == other.V && this.Button == other.Button;
        }

        // 重写 Equals，支持 object 比较
        public override bool Equals(object obj)
        {
            return obj is LSInput other && Equals(other);
        }

        // 取得按键的哈希值，如果按键一致则哈希值也一致
        public override int GetHashCode()
        {
            return HashCode.Combine(this.V, this.Button);
        }

        // 重载 == 运算符（用于直接比较 LSInput）
        public static bool operator ==(LSInput a, LSInput b)
        {
            if (a.V != b.V) // 向量不相等
            {
                return false;
            }

            if (a.Button != b.Button) // 按键不相等
            {
                return false;
            }

            return true; // 向量和按键都相等
        }

        // 重载 != 运算符
        public static bool operator !=(LSInput a, LSInput b)
        {
            return !(a == b);
        }
    }
}