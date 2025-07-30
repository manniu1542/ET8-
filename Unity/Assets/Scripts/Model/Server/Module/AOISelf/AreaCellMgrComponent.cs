using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    //区域 的管理器 ,  管理区域的创建 销毁
    [ComponentOf(typeof(Scene))]
    public class AreaCellMgrComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 单个区域的格子大小 长宽 10 
        /// </summary>
        public const int AreaCellSize = 10 * FloatToIntConversionFactor;

        /// <summary>
        /// 浮点转整数的转换因子
        /// </summary>
        public const int FloatToIntConversionFactor = 1000;

        /// <summary>
        /// 这个初始值是配置在表里的。不同类型的玩家 初始可视范围不一样，暂时临时定死，9米，单个格子10米
        /// </summary>
        public const int TmpNoramlPlayerVisableDistance = 9 * FloatToIntConversionFactor;
    }
}