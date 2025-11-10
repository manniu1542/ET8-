using System;
using Lockstep.Math;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ChildOf(typeof(LSUnitComponent))]
    [MemoryPackable]
    public partial class LSUnit : LSEntity, IAwake, ISerializeToEntity
    {
        public LVector3 Position
        {
            get;
            set;
        }

        [MemoryPackIgnore]
        [BsonIgnore]
        public LVector3 Forward
        {
            get => this.Rotation * LVector3.forward;
            set
            {
                //如果设置0向量。则不进行旋转
                if (value != LVector3.zero)
                {
                    this.Rotation = LQuaternion.LookRotation(value, LVector3.up);
                }
            }
        }

        public LQuaternion Rotation
        {
            get;
            set;
        }
        
        public HFSMDataSnapshot dataSnapshot;
        
    }
    /// <summary>
    /// 层次状态机的快照数据
    /// </summary>
    [EnableClass]
    [Serializable]
    [MemoryPackable]
    public partial class HFSMDataSnapshot
    {
        /// <summary>
        /// 这一帧状态机执行的时间
        /// </summary>
        public long elapsed;

        /// <summary>
        /// 状态机的名称id,从最终执行的状态=>Root  (TODO:可以从配置里面获取到)
        /// </summary>
        public string[] arrStateMachineName;

        /// <summary>
        /// 当前执行的状态名
        /// </summary>
        public string curStateName;

        /// <summary>
        /// 状态机的参数
        /// </summary>
        public float[] arrParameters;
    }
}