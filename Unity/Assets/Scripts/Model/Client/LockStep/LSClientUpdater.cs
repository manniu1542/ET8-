using TrueSync;

namespace ET.Client
{
    /// <summary>
    /// 帧同步的 
    /// </summary>
    [ComponentOf(typeof(Room))]
    public class LSClientUpdater: Entity, IAwake, IUpdate
    {
        public LSInput Input = new();
        
        public long MyId { get; set; }
    }
}