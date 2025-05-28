namespace ET.Client
{
    /// <summary>
    /// 路由连接器 ，负责选择路径（如选择服务器IP）和建立连接
    /// </summary>
    [ChildOf(typeof(NetComponent))]
    public class RouterConnector: Entity, IAwake, IDestroy
    {
        public byte Flag { get; set; }
    }
}