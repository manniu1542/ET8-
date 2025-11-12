using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;
using ZHFSM;

namespace ET
{
    //TODO: MemoryPackIgnore 不应该在view层有， 他是数据层的。等状态机可用了就把他移除掉把
    [ComponentOf(typeof(LSUnitView))]
    public partial class LSUnitState : Entity, IAwake<StateMachineExecutor>, IUpdate, ILSRollback
    {
      
        public StateMachineExecutor playerHFSM;
    
    }
}