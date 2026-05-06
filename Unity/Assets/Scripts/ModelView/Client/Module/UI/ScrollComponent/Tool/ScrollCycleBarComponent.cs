using ScrollViewUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ET
{
    /// <summary>
    /// 滚动动画 
    /// </summary>
    [ComponentOf]
    public class ScrollCycleBarComponent : Entity, IAwake, IDestroy
    {
        public ScrollCycleView scrScrollView;
        public Scrollbar scrollBar;
        /// <summary>
        /// 滚动条的进度，在scrollBar没有的时候记录下当前的进度
        /// </summary>
        public float scrollProgress;
    }
}
