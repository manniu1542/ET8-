namespace ET.Server
{
    /// <summary>
    /// AOI的管理器,管理他的子级Cell单位，Cell单位有容器管理AOIEntity组件。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class AOIManagerComponent: Entity, IAwake
    {
        
        public const int CellSize = 10 * 1000;
    }
}