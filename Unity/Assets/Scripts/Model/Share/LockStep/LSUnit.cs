using System;
using Lockstep.Math;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;


namespace ET
{
    [ChildOf(typeof(LSUnitComponent))]
    [MemoryPackable]
    public partial class LSUnit: LSEntity, IAwake, ISerializeToEntity
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
            set => this.Rotation = LQuaternion.LookRotation(value, LVector3.up);
        }
        
        public LQuaternion Rotation
        {
            get;
            set;
        }
    }
}