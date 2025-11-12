using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 只回滚非LS的组件，LS的组件数据是被LSWorld序列化的不需要回滚
    /// </summary>
    public interface ILSRollback
    {
    }
    
    public interface ILSRollbackSystem: ISystemType
    {
        void Run(Entity o);
    }
    
    [LSEntitySystem]
    public abstract class LSRollbackSystem<T>: SystemObject, ILSRollbackSystem where T: Entity, ILSRollback
    {
        void ILSRollbackSystem.Run(Entity o)
        {
            this.LSRollback((T)o);
        }

        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(ILSRollbackSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        protected abstract void LSRollback(T self);
    }
}