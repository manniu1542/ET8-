using ScrollViewUI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ET
{
    
    /// <summary>
    /// 滚动动画 
    /// </summary>
    [ComponentOf]
    public class ScrollMoveComponent : Entity, IAwake<RectTransform>, IDestroy
    {

        /// <summary>
        /// 滚动监听触发
        /// </summary>
        public EventTrigger trigger;

        /// <summary>
        /// 移动动画
        /// </summary>
        public DGTween animMove;


        /// <summary>
        /// 移动动画组件
        /// </summary>
        public UITweenSpring scrAnimMove;

        /// <summary>
        /// 滚动中item动画
        /// </summary>
        public IScrollUITweenAnim scrollUITweenAnim;

        /// <summary>
        /// 拖动灵敏度（越大越灵敏）
        /// </summary>
        public float speed = 1;

        /// <summary>
        /// 滑动阻力（越大阻力越大）
        /// </summary>
        public float resistance = 1;

        /// <summary>
        /// 拖拽回弹值
        /// </summary>
        public float dragRebound = 999;


        public Action OnTouchEnter;
        public Action<Vector2> OnTouchMove;
        public Action OnTouchExit;
        
        public bool canDrag = true;
    }
}
