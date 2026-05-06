using ScrollViewUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ET
{
    /// <summary>
    /// 滚动列表 所绑定的事件  （滑动区域绑定的，滑动块绑定的，滑动区域惯性移动的）
    /// </summary>

    public static class ScrollCycleEventBindSystem
    {
        public static void _ResetBackTween(this ScrollCycleView self)
        {
            self._SetBackTween();
        }

        /// <summary>
        /// 更新 拖拽的滑动区域 的 最新位置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="v2DeltaMove">滑动区域绑定的 OnDrag 在 每一帧 移动的距离 间隔向量</param>
        public static void _OnUpdatePosition(this ScrollCycleView self, Vector2 v2DeltaMove)
        {
            Vector2 lastPos = self.rtfGrid.anchoredPosition;
            //修正 参数 ，在竖滑动条 里  横向移动参数 置为0 . 横向滑动条同理
            if (self.IsVertical)
                v2DeltaMove.x = 0;
            else
                v2DeltaMove.y = 0;

            Vector2 willPos = lastPos + v2DeltaMove;


            // 在允许超过拖拽的基础上， 当前是否超过滑动区域的边界。（上下边界）
            bool isCurOutViewRange = false;
            //允许超框的处理  （检测是否超框，若超框 则在滑动结束 ，把 延长滑动 动画 播放时间设为约等于0，直接调用 OnMoveEnd 播放回弹）
            if (self.isUseElastic)
            {
                if (self.IsVertical)
                    isCurOutViewRange = (willPos.y < 0 && self.curDataIndex == 0) ||
                                        (willPos.y > self.totalMovableDistance && self.curDataIndex == self.maxOffset);
                else
                    isCurOutViewRange = (willPos.x > 0 && self.curDataIndex == 0) ||
                                        (willPos.x < -self.totalMovableDistance && self.curDataIndex == self.maxOffset);


                self.scrScrollMove.SetDragRebound(isCurOutViewRange);
            }
            else // 不允许超框拖拽的 处理   （把滑动数值 限定在可滑动区域）
            {
                //item的所占的长/宽 超过一页
                if (self.totalMovableDistance > 0)
                {
                    if (self.IsVertical)
                    {
                        if (willPos.y < 0 || willPos.y > self.totalMovableDistance)
                        {
                            willPos.y = Mathf.Clamp(willPos.y, 0, self.totalMovableDistance);
                        }
                    }
                    else
                    {
                        if (willPos.x > 0 || willPos.x < -self.totalMovableDistance)
                        {
                            willPos.x = Mathf.Clamp(willPos.x, -self.totalMovableDistance, 0);
                        }
                    }
                }
                else
                {
                    willPos = Vector2.zero;
                }
            }

            self._SetPosition(willPos);

            //超过边界只移动位置，不刷新item
            if (isCurOutViewRange)
                return;

            //实际滑动几个 item 数量
            int curIndex = 0;

            if (self.IsVertical)
            {
                curIndex = Mathf.FloorToInt(willPos.y / self.itemHeight);
                self.scrScrollBar.SetScrollProgressValue(willPos.y / self.totalMovableDistance);
            }
            else
            {
                curIndex = Mathf.FloorToInt(-willPos.x / self.itemWidth);
                self.scrScrollBar.SetScrollProgressValue(-willPos.x / self.totalMovableDistance);
            }


            self._SetItemIndex(curIndex);
        }

        /// <summary>
        /// 检测是否超框
        /// </summary>
        /// <param name="self"></param>
        public static void _OnCheckOutView(this ScrollCycleView self)
        {
            Vector2 fv2 = self.rtfGrid.anchoredPosition;

            if (self.IsVertical)
            {
                if (self.totalMovableDistance < 0)
                {
                    self._SetBackTween(DOTweenAnimCurve.Instance.DOAnchorPosY(self.rtfGrid, 0, 0.3f,
                        v => self._ItemAnimUpdateAllItem()));
                }
                else
                {
                    if (fv2.y < 0 && self.curDataIndex == 0)
                    {
                        self._SetBackTween(DOTweenAnimCurve.Instance.DOAnchorPosY(self.rtfGrid, 0, 0.3f,v => self._ItemAnimUpdateAllItem()));
                    }
                    else if (fv2.y > self.totalMovableDistance && self.curDataIndex == self.maxOffset)
                    {
                        self._SetBackTween(
                            DOTweenAnimCurve.Instance.DOAnchorPosY(self.rtfGrid, self.totalMovableDistance, 0.3f,v => self._ItemAnimUpdateAllItem()));
                    }
                }
            }
            else
            {
                if (self.totalMovableDistance < 0)
                {
                    self._SetBackTween(DOTweenAnimCurve.Instance.DOAnchorPosX(self.rtfGrid, 0, 0.3f,v => self._ItemAnimUpdateAllItem()));
                }
                else
                {
                    if (fv2.x > 0 && self.curDataIndex == 0)
                    {
                        self._SetBackTween(DOTweenAnimCurve.Instance.DOAnchorPosX(self.rtfGrid, 0, 0.3f,v => self._ItemAnimUpdateAllItem()));
                    }
                    else if (fv2.x < -self.totalMovableDistance && self.curDataIndex == self.maxOffset)
                    {
                        self._SetBackTween(DOTweenAnimCurve.Instance.DOAnchorPosX(self.rtfGrid,
                            -self.totalMovableDistance, 0.3f,v => self._ItemAnimUpdateAllItem()));
                    }
                }
            }
        }
    }
}