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
        
        
    }

}