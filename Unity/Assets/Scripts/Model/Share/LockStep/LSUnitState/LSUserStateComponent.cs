using MemoryPack;

namespace ET
{
    /// <summary>
    /// 维护玩家的状态 组件
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSUnitStateComponent: LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
    
    }
}