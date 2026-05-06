using ScrollViewUI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ET
{
    [EntitySystemOf(typeof(ScrollMoveComponent))]
    [FriendOf(typeof(ScrollMoveComponent))]
    public static partial class ScrollMoveComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ScrollMoveComponent self, RectTransform rtfScrollView)
        {

            self.trigger = rtfScrollView.GetComponent<EventTrigger>();
            var rtfGrid = rtfScrollView.GetChild(0);
            self.scrAnimMove = rtfGrid.GetComponent<UITweenSpring>();

            self.trigger.RegisterEvent(EventTriggerType.BeginDrag, self._OnBeginDrag);
            self.trigger.RegisterEvent(EventTriggerType.EndDrag, self._OnEndDrag);
            self.trigger.RegisterEvent(EventTriggerType.Drag, self._OnDrag);


            //移动的动画组件

            self.scrAnimMove.enabled = false;

            self.scrAnimMove.MomentumAmount = self.speed;
            self.scrAnimMove.Strength = self.resistance;

            self.canDrag =false;
        }

        [EntitySystem]
        private static void Destroy(this ScrollMoveComponent self)
        {


            self.scrollUITweenAnim = null;
            self.scrAnimMove = null;
            self.animMove = null;
            self.trigger = null;

        }
        public static void BindAnimMoveEvent(this ScrollMoveComponent self, Action<Vector2> OnUpdatePosition, Action OnMoveEnd)
        {

            self.scrAnimMove.OnUpdate = OnUpdatePosition;
            self.scrAnimMove.OnMoveEnd = OnMoveEnd;
        }
        public static void BindTouchEvent(this ScrollMoveComponent self, Action OnTouchEnter, Action<Vector2> OnTouchMove, Action OnTouchExit)
        {
            self.OnTouchEnter = OnTouchEnter;
            self.OnTouchMove = OnTouchMove;
            self.OnTouchExit = OnTouchExit;
        }

        public static void _OnBeginDrag(this ScrollMoveComponent self, BaseEventData data)
        {
            //表示不会 向下渗透传播事件信息了。
            data.Use(); 
            if(!self.canDrag) return;
            self.scrAnimMove.enabled = true;
            self.scrAnimMove.Momentum = Vector3.zero;
            self.scrAnimMove.IsUseCallBack = false;
            self.scrAnimMove.Strength = self.resistance;
            self.OnTouchEnter?.Invoke();
        }

        public static void _OnDrag(this ScrollMoveComponent self, BaseEventData data)
        {
            data.Use();
            if(!self.canDrag) return;
            PointerEventData ped = data as PointerEventData;
            self.scrAnimMove.LerpMomentum(ped.delta);
            self.OnTouchMove(ped.delta * self.speed);


        }

        public static void _OnEndDrag(this ScrollMoveComponent self, BaseEventData data)
        {
            data.Use();
            //开启了动画移动
            self.scrAnimMove.IsUseCallBack = true;
            self.OnTouchExit?.Invoke();
        }
        /// <summary>
        /// 强制移除拖拽后的动画移动
        /// </summary>
        public static void ForceRemoveDragAnim(this ScrollMoveComponent self)
        {
            if (self.scrAnimMove == null) return;
            self.scrAnimMove.IsUseCallBack = false;
            self.scrAnimMove.enabled = false;
            self.scrAnimMove.Momentum = Vector3.zero;

        }


        /// <summary>
        /// 设置 超界限 回弹（）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tween"></param>
        public static void SetDragRebound(this ScrollMoveComponent self, bool isCurOutViewRange)
        {
            self.scrAnimMove.Strength = isCurOutViewRange ? self.dragRebound : self.resistance;
        }
        /// <summary>
        /// 是否能 触发scroll 滚动动画
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tween"></param>
        public static void SetActiveTriggerUI(this ScrollMoveComponent self, bool isActive)
        {
            self.trigger.enabled = isActive;
        }

        /// <summary>
        /// 设置是否可以拖拽滚动
        /// </summary>
        /// <param name="self"></param>
        /// <param name="canDrag"></param>
        public static void SetCanDrag(this ScrollMoveComponent self, bool canDrag)
        {
            self.canDrag = canDrag;
        }



    }
}
