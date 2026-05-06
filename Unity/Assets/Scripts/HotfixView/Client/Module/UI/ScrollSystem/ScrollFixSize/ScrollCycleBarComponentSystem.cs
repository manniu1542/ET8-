using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [EntitySystemOf(typeof(ScrollCycleBarComponent))]
    [FriendOf(typeof(ScrollCycleBarComponent))]
    [FriendOf(typeof(ScrollCycleView))]
    
    public static partial class ScrollCycleBarComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ScrollCycleBarComponent self)
        {
            self.scrScrollView = self.Parent as ScrollCycleView;
            ReferenceCollector rc = self.scrScrollView.rtfRoot.GetComponent<ReferenceCollector>();

            self.scrollBar = rc.Get<GameObject>("Scrollbar")?.GetComponent<Scrollbar>();


        }

        [EntitySystem]
        private static void Destroy(this ScrollCycleBarComponent self)
        {

            self.scrollBar = null;

        }

        public static void SetDataBar(this ScrollCycleBarComponent self, int dataCount, int oldIndex,  Vector2 oldPos)
        {

            //设置 scrollBar
            if (self.scrollBar != null)
            {
                // 求出 初始页在总长的占比，得出 滑动块的尺寸（一页在总长的比例）

                if (self.scrScrollView.IsVertical)   //设置初始 一页占多少 ，总长度页（多少竖的item* itemHeight 去除最后一个item的间隔既为总长） 的 多少。
                    self.scrollBar.size = self.scrScrollView.rtfScrollView.sizeDelta.y / (self.scrScrollView.itemHeight * Mathf.CeilToInt((float)dataCount / self.scrScrollView.fixedCount) + self.scrScrollView.spacing.y);
                else
                    self.scrollBar.size = self.scrScrollView.rtfScrollView.sizeDelta.x / (self.scrScrollView.itemWidth * Mathf.CeilToInt((float)dataCount / self.scrScrollView.fixedCount) + self.scrScrollView.spacing.x);



                //求出 当前位置的 scrollbar 所在的 比例。
                if (oldIndex == 0)
                {
                    self.scrollBar.value = 0;
                }
                else
                {
                    if (self.scrScrollView.IsVertical)
                        self.SetScrollProgressValue(oldPos.y / self.scrScrollView.totalMovableDistance);
                    else
                        self.SetScrollProgressValue(-oldPos.x / self.scrScrollView.totalMovableDistance);

                }

                self.scrollBar.onValueChanged.RemoveAllListeners();
                self.scrollBar.onValueChanged.AddListener(self.OnScrollProgressChanged);
            }
        }

        /// <summary>
        /// 滑动进度改变造成的item 数据 预制改动
        /// </summary>
        /// <param name="self"></param>
        /// <param name="f"></param>
        public static void OnScrollProgressChanged(this ScrollCycleBarComponent self, float f)
        {

            self.scrollProgress = f;
            Vector2 v2 = Vector2.zero;
            int curIndex = 0;

            if (self.scrScrollView.IsVertical)
            {
                v2 = Vector2.Lerp(Vector2.zero, new Vector2(0, self.scrScrollView.totalMovableDistance), f);
                curIndex = Mathf.FloorToInt(v2.y / self.scrScrollView.itemHeight);
            }
            else
            {
                v2 = Vector2.Lerp(Vector2.zero, new Vector2(-self.scrScrollView.totalMovableDistance, 0), f);
                curIndex = Mathf.FloorToInt(-v2.x / self.scrScrollView.itemWidth);
            }


            self.scrScrollView._SetPosition(v2);
            self.scrScrollView._SetItemIndex(curIndex);
        }

        /// <summary>
        /// 设置 滑动进度的值 =  当前滑动距离/总可滑动距离，
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        public static void SetScrollProgressValue(this ScrollCycleBarComponent self, float value)
        {
            value = value < 0.009f ? 0f : (value > 0.992f ? 1f : value);
            self.scrollProgress = value;
            if (self.scrollBar != null)
                self.scrollBar.SetValueWithoutNotify(value);
        }


    }
}
