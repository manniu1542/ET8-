using System;
using System.Buffers;
using System.IO;

namespace ET
{
    /// <summary>
    /// 自定义的内存缓冲区
    /// - 继承 MemoryStream（方便读写、定位） 可以动态拿/取 缓存的字节流
    /// - 实现 IBufferWriter<byte>（支持高性能写入接口，常用于序列化/管道传输）  实现定义的 接口。使内部存储的始终是一个数组，节省内存。 管理指向的游标位置
    /// </summary>
    public class MemoryBuffer : MemoryStream, IBufferWriter<byte>
    {
        // 缓冲区的起始位置（主要用于支持偏移量构造）
        private int origin;
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public MemoryBuffer()
        {
        }
        
        /// <summary>
        /// 指定容量的构造函数
        /// </summary>
        public MemoryBuffer(int capacity): base(capacity)
        {
        }
        
        /// <summary>
        /// 使用现有字节数组作为缓冲区
        /// </summary>
        public MemoryBuffer(byte[] buffer): base(buffer)
        {
        } 
        
        /// <summary>
        /// 使用现有字节数组的一个片段作为缓冲区
        /// </summary>
        public MemoryBuffer(byte[] buffer, int index, int length): base(buffer, index, length)
        {
            this.origin = index;
        }
        
        /// <summary>
        /// 已写入的数据（只读内存）
        /// 从 origin 开始，到当前 Position 结束
        /// </summary>
        public ReadOnlyMemory<byte> WrittenMemory => this.GetBuffer().AsMemory(this.origin, (int)this.Position);

        /// <summary>
        /// 已写入的数据（只读 Span）
        /// 从 origin 开始，到当前 Position 结束
        /// </summary>
        public ReadOnlySpan<byte> WrittenSpan => this.GetBuffer().AsSpan(this.origin, (int)this.Position);

        /// <summary>
        /// 通知写入器，已经写入了 count 个字节
        /// 更新 Position，如果超出当前长度，则扩展 Length
        /// </summary>
        public void Advance(int count)
        {
            long newLength = this.Position + count;
            if (newLength > this.Length)
            {
                // 如果写入超出已有长度，扩展缓冲区
                this.SetLength(newLength);
            }
            // 移动写入位置
            this.Position = newLength;
        }

        /// <summary>
        /// 获取可写入的 Memory（用于 IBufferWriter）    堆上的数据。
        /// sizeHint = 建议的最小可用空间  
        /// </summary>
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            // 如果剩余空间不足，扩展长度
            if (this.Length - this.Position < sizeHint)
            {
                this.SetLength(this.Position + sizeHint);
            }
            // 返回当前 Position 之后的可写入内存
            var memory = this.GetBuffer().AsMemory((int)this.Position + this.origin, (int)(this.Length - this.Position));
            return memory;
        }

        /// <summary>
        /// 获取可写入的 Span（用于 IBufferWriter）     栈空间操作的数据，速度快。
        /// sizeHint = 建议的最小可用空间
        /// </summary>
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            // 如果剩余空间不足，扩展长度
            if (this.Length - this.Position < sizeHint)
            {
                this.SetLength(this.Position + sizeHint);
            }
            // 返回当前 Position 之后的可写入 Span
            var span = this.GetBuffer().AsSpan((int)this.Position + this.origin, (int)(this.Length - this.Position));
            return span;
        }
    }
}
