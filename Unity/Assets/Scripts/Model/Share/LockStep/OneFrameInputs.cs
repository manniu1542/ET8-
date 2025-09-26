using System;
using System.Collections.Generic;

namespace ET
{
    
    
    
    /// <summary>
    /// 是网络某一帧的输入消息扩展类  OneFrameInputs 表示某一帧的输入集合（例如玩家操作、指令等）
    /// </summary>
    public partial class OneFrameInputs
    {
        // 比较两个 OneFrameInputs 的Inputs引用是否 相等
        protected bool Equals(OneFrameInputs other)
        {
            return Equals(this.Inputs, other.Inputs);
        }

        // 把当前 Inputs 的内容拷贝到另一个 OneFrameInputs 中
        public void CopyTo(OneFrameInputs to)
        {
            to.Inputs.Clear(); // 先清空目标的输入
            foreach (var kv in this.Inputs) // 遍历当前输入
            {
                to.Inputs.Add(kv.Key, kv.Value); // 逐个复制到目标
            }
        }

        // 重写 Equals，用于对象比较
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) // 如果对象为空
            {
                return false;
            }

            if (ReferenceEquals(this, obj)) // 如果是同一个引用对象
            {
                return true;
            }

            if (obj.GetType() != this.GetType()) // 如果类型不同
            {
                return false;
            }

            return Equals((OneFrameInputs)obj); // 调用上面的 Equals 方法
        }

        /// <summary>
        /// 给这一帧所有玩家的输入打个哈希值。如果输入一致则hash值也一致
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Inputs);
        }

        // 重载 == 运算符，两个 OneFrameInputs 的比较规则
        public static bool operator ==(OneFrameInputs a, OneFrameInputs b)
        {
            // 如果有一个是 null
            if (a is null || b is null)
            {
                if (a is null && b is null) // 两个都为 null 则相等
                {
                    return true;
                }

                return false; // 只有一个是 null，则不相等
            }

            // 如果字典数量不同，则直接不相等
            if (a.Inputs.Count != b.Inputs.Count)
            {
                return false;
            }

            // 遍历 a 的输入，逐个与 b 对比 ，key表示玩家的输入
            foreach (var kv in a.Inputs)
            {
                // 如果 b 中，有没有该玩家，
                if (!b.Inputs.TryGetValue(kv.Key, out LSInput inputInfo))
                {
                    return false;
                }

                //该玩家输入一致不一致。使用了LSInput 的对比。他们使用 内部具体的对比。
                if (kv.Value != inputInfo)
                {
                    return false;
                }
            }

            return true; // 所有 key-value 都相同，则相等
        }

        // 重载 != 运算符（与 == 相反）
        public static bool operator !=(OneFrameInputs a, OneFrameInputs b)
        {
            return !(a == b);
        }
    }
}