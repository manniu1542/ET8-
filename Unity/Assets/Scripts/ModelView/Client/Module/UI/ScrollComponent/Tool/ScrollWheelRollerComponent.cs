using ScrollViewUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ET
{
    /// <summary>
    ///  滚轮输入
    /// </summary>
    [ComponentOf]
    public class ScrollWheelRollerComponent : Entity, IAwake<RectTransform,InputAction>, IDestroy ,IUpdate
    {

        /// <summary>
        /// 滚动监听触发
        /// </summary>
        public EventTrigger trigger;

        /// <summary>
        /// 滚轮输入
        /// </summary>
        public InputAction inputActMouseScroll;

        public bool isInViewStay;

        /// <summary>
        /// 鼠标输入滚动值 输入
        /// </summary>
        public Action<float> funWheelRollerInput; 
    }
}
