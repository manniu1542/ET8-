using ScrollViewUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{

    /// <summary>
    /// 滚动列表移动的逻辑
    /// </summary>
    [FriendOf(typeof(ScrollCustomRange))]
    [FriendOf(typeof(ScrollCustomRangeItemComponent))]
    [FriendOf(typeof(ScrollMoveComponent))]
    public static class ScrollCustomRangeMoveSystem
    {

        /// <summary>
        /// 设置 滑动的动画
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tween"></param>
        public static void _SetBackTween(this ScrollCustomRange self, DGTween tween = null)
        {
            if (self.tmpTween != null)
            {

                DOTweenAnimCurve.Instance.KillAnimCurve(self.tmpTween);
                self.tmpTween = null;
            }

            self.tmpTween = tween;

        }

        /// <summary>
        /// 设置 滑动的 位置    移动可视区域
        /// </summary>
        /// <param name="self"></param>
        /// <param name="v2"></param>
        public static void _SetPosition(this ScrollCustomRange self, Vector2 v2)
        {


            self.rtfGrid.anchoredPosition = v2;


        }

        /// <summary>
        /// 重置视窗 滑动的 位置    移动可视区域
        /// </summary>
        /// <param name="self"></param>
        /// <param name="v2"></param>
        public static  void ResetViewSize(this ScrollCustomRange self)
        {

            //设置 scroll 的适配大小
            self.rtfScrollView.anchoredPosition = Vector2.zero;
            //设置默认滑动列表尺寸
            self.rtfScrollView.sizeDelta = new Vector2(Mathf.Abs(self.rtfRoot.rect.size.x), Mathf.Abs(self.rtfRoot.rect.size.y));
            self.v2ViewSize = self.rtfScrollView.sizeDelta;
            self.onePageMaxItemCount = Mathf.CeilToInt(self.v2ViewSize.y / self.itemMinSizeDefault.y);

            self.ResetPoolGO();

            int idx = 0;
            if (self.listItemGO.First != null && self.listItemGO.First.Value != null && self.listItemGO.First.Value.data != null)
            {

                idx = self.SwitchItemIdxToUIIdx(self.listItemGO.First.Value.data.index);
            }
           
             self.InitScroll(self.dataCount, self.funUpdateItem, idx);

        }
















    }

}
