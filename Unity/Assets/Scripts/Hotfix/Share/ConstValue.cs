
namespace ET
{
    
 
    public static partial class ConstValue
    {
        public const string RouterHttpHost = "127.0.0.1";
        public const int RouterHttpPort = 30300;
        public const int SessionTimeoutTime = 30 * 1000;

        /// <summary>
        /// 使用自己写的AOI 组件，来管理玩家的u 可视范围。在状态同步的时候
        /// </summary>
        public const bool IsUseSelfAOI = true;

        /// <summary>
        /// 使用自己写的移动组件，来管理移动
        /// </summary>
        public const bool IsUseSelfMove = true;
    }
}