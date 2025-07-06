namespace ET.Server
{
    /// <summary>
    /// unit的aoi中A看到B
    /// </summary>
    public struct UnitAOIAVisibleBEvent
    {
        //大小是16个字节。引用类型8个字节大小。
        public AreaOfInterestEntity A;
        public AreaOfInterestEntity B;
    }
    
    /// <summary>
    /// unit的aoi中A看不到B
    /// </summary>
    public struct UnitAOIAInVisibleBEvent
    {
        public AreaOfInterestEntity A;
        public AreaOfInterestEntity B;
    }
    
}