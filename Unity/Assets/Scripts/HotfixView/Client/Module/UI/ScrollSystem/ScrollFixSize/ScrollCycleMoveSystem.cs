using ScrollViewUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 滚动列表移动的逻辑
    /// </summary>

    [FriendOf(typeof(ScrollCycleView))]
    [FriendOf(typeof(ScrollMoveComponent))]
    public static partial class ScrollCycleMoveSystem
    {
        /// <summary>
        /// 设置 滑动的动画
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tween"></param>
        public static void _SetBackTween(this ScrollCycleView self, DGTween tween = null)
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
        public static void _SetPosition(this ScrollCycleView self, Vector2 v2)
        {
            self.rtfGrid.anchoredPosition = v2;
            self._ItemAnimUpdateAllItem();
        }
        /// <summary>
        ///   设置所有item动画位置变化
        /// </summary>
        /// <param name="self"></param>
        public static void _ItemAnimUpdateAllItem(this ScrollCycleView self)
        {
        
            if (self.scrScrollMove.scrollUITweenAnim != null)
                foreach (var item in self.dicItemData.Keys)
                {
                    self.scrScrollMove.scrollUITweenAnim.OnUpdateItemAnim(item.rtf, self);
                }
        }

        /// <summary>
        /// 设置 某个item的激活信息以及回调
        /// </summary>
        /// <param name="self"></param>
        /// <param name="index">滑动条的某个item的 idex编号</param>
        /// <param name="item"></param>
        public static void _SetUpdateItem(this ScrollCycleView self, int index, ScrollCycleView.ItemData item,
            bool isRefreshDicData = true)
        {
            if (index < self.dataCount)
            {
                //设置激活 
                if (!item.isActive)
                {
                    item.isActive = true;
                    item.go.SetActive(true);
                }

                self.scrScrollMove.scrollUITweenAnim?.OnUpdateItemAnim(item.rtf, self);
                //回调方法
                self?.funUpdateItem(index, item.go);

                if (self.dicItemData.ContainsKey(item))
                    self.dicItemData[item] = index;
                else
                {
                    self.dicItemData.Add(item, index);
                }
            }
            else
            {
                if (item.isActive)
                {
                    item.isActive = false;
                    item.go.SetActive(false);
                }
            }
        }


        /// <summary>
        ///  更新item 的位置 以及 信息 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="curIndex">当前 item的index   竖： 从上往下 第一排的idex</param>
        public static void _SetItemIndex(this ScrollCycleView self, int curIndex)
        {
            curIndex = Math.Clamp(curIndex, 0, self.maxOffset);
            if (curIndex != self.curDataIndex)
            {
                self.ItemListOffset(curIndex - self.curDataIndex);

                self.curDataIndex = curIndex;
            }
        }

        //在 lua中 负数 求余的  -1%10 = 9  .C# 中 -1%10 = -1；  C#来模拟 lua求余 -1%10 = -1%10+10 
        private static int Modulo(int dividend, int divisor)
        {
            int remainder = dividend % divisor;
            return remainder < 0 ? remainder + divisor : remainder;
        }

        /// <summary>
        /// 根据 item的移动， cellItemList 所在的列 ，数据 的顺序。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="list"></param>
        /// <param name="k"></param>
        private static void Move(this ScrollCycleView self, List<ScrollCycleView.ItemData> list, int k)
        {
            int len = list.Count;
            k = Modulo(k, len);

            if (k == 0)
                return;
            int endIdx = len - 1;

            //反转 偏移的 那几个物体
            self.Reverse(list, 0, endIdx - k);
            //反转 后续的 那几个物体
            self.Reverse(list, endIdx - k + 1, endIdx);
            //整体反转
            self.Reverse(list, 0, endIdx);
        }

        /// <summary>
        /// 反转数组里面的 元素       （交换左右两边数据，直到中间）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="list"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private static void Reverse(this ScrollCycleView self, List<ScrollCycleView.ItemData> list, int start, int end)
        {
            while (start < end)
            {
                ScrollCycleView.ItemData temp = list[start];
                list[start] = list[end];
                list[end] = temp;
                start++;
                end--;
            }
        }

        /// <summary>
        /// 元素列表偏移 ： 元素自身位置= 当前位置+偏移的页数*一页的 （高/宽）  ★★★  核心代码  
        /// </summary>
        /// <param name="self"></param>
        /// <param name="offsetNum"></param>
        private static void ItemListOffset(this ScrollCycleView self, int offsetNum)
        {
            int cutItemRowOrCol, offsetPage, offsetRemainder, curChangeOffsetPage, showIdx, itemNextPage;
            ScrollCycleView.ItemData item;

            //找出那几个 item需要 更新

            //竖滑动条时： 偏移了多少页（一页需要多少个item填满，总数/多少个Item，得出第几页）
            offsetPage = Mathf.FloorToInt(Mathf.Abs(offsetNum) / self.OnePageOneFixeditemCount);
            //竖滑动条时：偏移了几个item   
            offsetRemainder = Mathf.Abs(offsetNum) % self.OnePageOneFixeditemCount;

            if (offsetNum > 0)
            {
                //ui的移动
                for (int j = 0; j < self.fixedCount; j++)
                {
                    for (int i = 0; i < self.OnePageOneFixeditemCount; i++)
                    {
                        //当前item是否 翻到下一页。      
                        itemNextPage = (i + 1) <= offsetRemainder ? 1 : 0;

                        curChangeOffsetPage = itemNextPage + offsetPage;
                        if (curChangeOffsetPage > 0) //那几个 item需要移动 （某个item他下次移动的位置，+x页的当前位置）
                        {
                            //当前item所处的行或列
                            cutItemRowOrCol = self.OnePageOneFixeditemCount * curChangeOffsetPage + self.curDataIndex +
                                              i;
                            //显示的具体idx  j表示第几列， + hang* 一行多少个   =  当前所处于的列数的上一列* 行数+当前所处的列数排第几个 
                            showIdx = cutItemRowOrCol * self.fixedCount + j;

                            //
                            item = self.listItemData[j][i];
                            //这个item当前的位置 = 当前位置+ 移动了多少页的 插值
                            item.rtf.anchoredPosition += self.v2OnePageOneFixedTotalItemSize * curChangeOffsetPage;


                            self._SetUpdateItem(showIdx, item);
                        }
                    }

                    //data数组 跟随 ui的 变动， 每次都是 从上倒下 依次排列
                    self.Move(self.listItemData[j], -offsetNum);
                }
            } //竖滑动时， 代表 向下 滑动
            else //竖滑动时， 代表 向上 滑动//竖滑动时， 代表 向上 滑动
            {
                for (int j = self.fixedCount - 1; j >= 0; --j)
                {
                    for (int i = self.OnePageOneFixeditemCount - 1; i >= 0; --i)
                    {
                        itemNextPage = (i + 1) > (self.OnePageOneFixeditemCount - offsetRemainder) ? 1 : 0;

                        curChangeOffsetPage = itemNextPage + offsetPage;
                        if (curChangeOffsetPage > 0)
                        {
                            cutItemRowOrCol = self.curDataIndex + i -
                                              self.OnePageOneFixeditemCount * curChangeOffsetPage;

                            showIdx = cutItemRowOrCol * self.fixedCount + j;


                            item = self.listItemData[j][i];
                            item.rtf.anchoredPosition -= self.v2OnePageOneFixedTotalItemSize * curChangeOffsetPage;

                            self._SetUpdateItem(showIdx, item);
                        }
                    }

                    //data数组 跟随 ui的 变动
                    self.Move(self.listItemData[j], -offsetNum);
                }
            }
        }
    }
}