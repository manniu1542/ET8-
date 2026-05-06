using ScrollViewUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ET
{
    /// <summary>
    ///  鼠标滚轮 逻辑
    /// </summary>
    [EntitySystemOf(typeof(ScrollWheelRollerComponent))]
    [FriendOf(typeof(ScrollWheelRollerComponent))]
    public static partial class ScrollRollerSystem
    {
        [EntitySystem]
        private static void Awake(this ScrollWheelRollerComponent self, RectTransform rtfScrollView,InputAction uiInputAction)
        {
            self.trigger = rtfScrollView.GetComponent<EventTrigger>();
            //TODO:UI挂载的滚动 ui事件
            self.inputActMouseScroll = uiInputAction;
            self.trigger.RegisterEvent(EventTriggerType.PointerEnter, self.OnEnter);
            self.trigger.RegisterEvent(EventTriggerType.PointerExit, self.OnExit);
        }

        [EntitySystem]
        private static void Update(this ScrollWheelRollerComponent self)
        {
            //在滚动条里面且，可以滚动。
            if (self.isInViewStay&&self.inputActMouseScroll!=null)
            {
                Vector2 v2 = self.inputActMouseScroll.ReadValue<Vector2>();
                //鼠标的值 +-120  +-240  
                if (v2.y != 0)
                {
                    self.funWheelRollerInput?.Invoke(v2.y * 0.001f);
                }
            }
        }
        [EntitySystem]
        private static void Destroy(this ScrollWheelRollerComponent self)
        {
            self.funWheelRollerInput = null;
            self.trigger = null;
            self.inputActMouseScroll = null;
        }
        private static void OnEnter(this ScrollWheelRollerComponent self, BaseEventData data)
        {
            self.isInViewStay = true;
        }
        private static void OnExit(this ScrollWheelRollerComponent self, BaseEventData data)
        {
            self.isInViewStay = false;

        }

    }
}
