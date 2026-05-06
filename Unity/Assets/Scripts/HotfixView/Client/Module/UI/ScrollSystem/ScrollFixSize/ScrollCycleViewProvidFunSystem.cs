using ScrollViewUI;
using System;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 提供的功能 （放置的列数量，刷新当前item，设置列表位置）
    /// </summary>

    public static class ScrollCycleViewProvidFunSystem
    {
        /// <summary>
        /// 移除动画移动相关的 组件及数据
        /// </summary>
        /// <param name="self"></param>
        private static void RemoveMoveAnim(this ScrollCycleView self)
        {
            //TODO:补丁：如果超屏幕滑动的时候，超过屏幕，在此时又重新刷新列表，会使 grid的位置 超过边界

            //此时把动画禁用掉，会造成 不能重新刷正确数据

            //self._SetBackTween();
            //self.moveTween.IsUseCallBack = false;
            //self.moveTween.enabled = false;
        }
        /// <summary>
        /// 刷新下item新的回调方法
        /// </summary>
        /// <param name="self"></param>
        public static void RefreshCallBackItemData(this ScrollCycleView self)
        {
            //拿到当前页面上的idx，活itemdata，然后 调用
            int endIdx = self.curDataIndex + self.OnePageOneFixeditemCount * self.fixedCount - 1;
            endIdx = Mathf.Min(endIdx, self.dataCount - 1);
            foreach (var item in self.dicItemData)
            {
                if (item.Key.isActive && item.Value >= self.curDataIndex && item.Value <= endIdx)
                {
                    self?.funUpdateItem(item.Value, item.Key.go);
                }
            }



        }


        /// <summary>
        ///  对应 数据 显示 ，刷新 当前滑动item
        /// </summary>
        /// <param name="self"></param>
        /// <param name="dataCount"></param>
        /// <param name="updateFunc"></param>
        public static async ETTask SetData(this ScrollCycleView self, int dataCount, Action<int, GameObject> updateFunc, int SpawanItemPreCount = 15)
        {
            //动画组件移除
            self.RemoveMoveAnim();
            //触摸关闭。
            self.scrScrollMove.SetActiveTriggerUI(false);
            // reset data
            self.dataCount = dataCount;
            self.funUpdateItem = updateFunc;
            self.listItemGOPool.Clear();
            self.dicItemData.Clear();

            //竖 或 横 滑动  对应的 竖 总数量。 或行总数亮
            int VetiOrHoriTotalNum = Mathf.CeilToInt((float)self.dataCount / self.fixedCount);
            if (self.IsVertical)
            {
                self.totalMovableDistance = self.itemHeight * VetiOrHoriTotalNum + self.spacing.y - self.rtfScrollView.sizeDelta.y;
                self.maxOffset = Mathf.Max(VetiOrHoriTotalNum - self.OnePageOneFixeditemCount, 0);
            }
            else
            {
                self.totalMovableDistance = self.itemWidth * VetiOrHoriTotalNum + self.spacing.x - self.rtfScrollView.sizeDelta.x;
                self.maxOffset = Mathf.Max(VetiOrHoriTotalNum - self.OnePageOneFixeditemCount, 0);
            }

            self._SetBackTween();

            int oldIndex = self.curDataIndex;
            Vector2 oldPos = self.rtfGrid.anchoredPosition;
            self.curDataIndex = 0;
            self._SetPosition(Vector2.zero);

            #region 设置 初始Item的位置
            //一半的偏移间距
            //float halfItemSpacing = self.dragType == Scroll.DragType.Vertical ? self.spacing.y / 2 : self.spacing.x / 2;
            for (int j = 0; j < self.fixedCount; j++)
            {
                for (int i = 0; i < self.OnePageOneFixeditemCount; i++)
                {
                    ScrollCycleView.ItemData item = self.listItemData[j][i];
                    int index = j + i * self.fixedCount;
                    //
                    if (item.go == null && index < self.dataCount)
                    {
                        GameObject go = GameObject.Instantiate(self.item);
                        if (index != 0 && index % SpawanItemPreCount == 0)
                        {
                            await  self.Root().GetComponent<TimerComponent>().WaitFrameAsync();
                        }
                        go.name = "item" + index;
                        go.transform.SetParent(self.rtfGrid, false);
                        go.transform.localPosition = Vector3.zero;
                        go.SetActive(false);

                        RectTransform tran = go.GetComponent<RectTransform>();
                        tran.anchorMin = new Vector2(0, 1);
                        tran.anchorMax = new Vector2(0, 1);
                        tran.pivot = self.itemPivot;
                        item.go = go;
                        item.rtf = tran;
                    }

                    //设置 初始位置
                    if (item.go != null)
                    {
                        switch (self.dragType)
                        {
                            case Scroll.DragType.Vertical:

                                //item 位置 ，考虑 item 的中心点
                                item.rtf.anchoredPosition = new Vector2(
                                    self.itemWidth * j,
                                    -i * self.itemHeight
                                );
                                break;
                            case Scroll.DragType.Horizontal:

                                item.rtf.anchoredPosition = new Vector2(
                                    i * self.itemWidth,
                                    -self.itemHeight * j
                                );
                                break;
                            default:
                                break;
                        }

                        self.listItemGOPool.Add(item.go);
                    }

                    self._SetUpdateItem(index, item);
                }
            }
            #endregion


            self.scrScrollBar.SetDataBar(dataCount, oldIndex, oldPos);


            //设置 之前的位置 。    懒加载。。
            if (oldIndex != 0)
            {
                if (self.totalMovableDistance > 0)
                {

                    if (self.IsVertical)
                        self._SetPosition(new Vector2(0, Mathf.Clamp(oldPos.y, 0, self.totalMovableDistance)));
                    else
                        self._SetPosition(new Vector2(Mathf.Clamp(oldPos.x, -self.totalMovableDistance, 0), 0));


                }
                self._SetItemIndex(oldIndex);
            }
            self.scrScrollMove.SetActiveTriggerUI(true);
        }



        /// <summary>
        /// 设置当前索引位置在 那个滚动item的idex
        /// </summary>
        /// <param name="self"></param>
        /// <param name="index">移动当前显示到那个滚动item的idex</param>
        /// <param name="duration">移动的时间 若duration > 0, 则播放动画缓动到指定位置, 否则立刻移动到指定位置</param>
        /// <param name="easeType">动画曲线</param>
        public static void SetIndex(this ScrollCycleView self, int index = 0, float duration = 0, EaseType easeType = EaseType.Linear)
        {
            self.RemoveMoveAnim();

            //算出 具体偏移到那一排
            int itemOffset = Mathf.CeilToInt((float)index / self.fixedCount);
            itemOffset = (index % self.fixedCount) == 0 ? itemOffset : itemOffset - 1;

            if (self.totalMovableDistance > 0) //滑动区间是否 能够翻页 能够翻页才 需要计算 设置idex
            {
                Vector2 av2 = self.rtfGrid.anchoredPosition;
                Vector2 mv2 = Vector2.zero;
                if (self.IsVertical)
                    mv2 = new Vector2(0, Mathf.Clamp(self.itemHeight * itemOffset, 0, self.totalMovableDistance));
                else
                    mv2 = new Vector2(Mathf.Clamp(-self.itemWidth * itemOffset, -self.totalMovableDistance, 0), 0);

                if (duration > 0)
                {
                    DOTweenAnimCurve.Instance.To(0, 1, duration,
                            progress =>
                            {
                                self._SetPosition(Vector2.Lerp(av2, mv2, progress));

                                int curIndexTmp = self.IsVertical ? Mathf.FloorToInt(self.rtfGrid.anchoredPosition.y / self.itemHeight)
                                    : Mathf.FloorToInt(-self.rtfGrid.anchoredPosition.x / self.itemWidth);

                                self._SetItemIndex(curIndexTmp);
                                float curGridLen = self.IsVertical ? self.rtfGrid.anchoredPosition.y : -self.rtfGrid.anchoredPosition.x;
                                self.scrScrollBar.SetScrollProgressValue(curGridLen / self.totalMovableDistance);
                            }
                        )
                        .OnStart(() =>
                        {
                            self.scrScrollMove.SetActiveTriggerUI(false);
                        })
                        .OnComplete(() =>
                        {
                            self.scrScrollMove.SetActiveTriggerUI(true);
                        })
                        .SetEase(easeType);
                }
                else
                {
                    //Log.Error($"11  av2 {av2}, mv2 {mv2},itemOffset {itemOffset}");
                    //不进行移动的区域检测
                    if (self.curDataIndex > 0) //--初始化为0，不进行区域计算
                    {
                        foreach (var kvp in self.dicItemData) //--检测到在可视区间内切换索引，记录原索引位置，保持切换后可视区域相对位置固定
                        {
                            if (kvp.Value == index)
                            {
                                Vector2 kv2 = kvp.Key.rtf.anchoredPosition + av2;
                                bool b = false;

                                if (self.IsVertical)
                                {
                                    b = kv2.y > -self.rtfScrollView.sizeDelta.y - self.cellSize.y / 2 && kv2.y < self.cellSize.y / 2;
                                }
                                else
                                {
                                    b = kv2.x > -self.cellSize.x / 2 && kv2.x < self.rtfScrollView.sizeDelta.x + self.cellSize.x / 2;
                                }

                                if (b)
                                {
                                    mv2 = av2;
                                    itemOffset = self.curDataIndex;
                                }
                                break;
                            }
                        }
                    }
                    //Log.Error($"22  av2 {av2}, mv2 {mv2},itemOffset {itemOffset}");
                    //移动可视区域
                    self._SetPosition(mv2);
                    //设置当前的 可视区域 显示的item
                    self._SetItemIndex(itemOffset);

                    //滚动条
                    float curGridLen = self.IsVertical ? self.rtfGrid.anchoredPosition.y : -self.rtfGrid.anchoredPosition.x;

                    self.scrScrollBar.SetScrollProgressValue(curGridLen / self.totalMovableDistance);
                }
            }
            else
            {

                self._SetPosition(Vector2.zero);
                self._SetItemIndex(0);
                self.scrScrollBar.SetScrollProgressValue(0);

            }
        }
        
    /// <summary>
    /// 根据 index 设置选中项，保证该项尽量始终处于可视区域的中心
    /// </summary>
    /// <param name="self"></param>
    /// <param name="index">移动当前显示到那个滚动item的idex</param>
    /// <param name="duration">移动的时间 若duration > 0, 则播放动画缓动到指定位置, 否则立刻移动到指定位置</param>
    /// <param name="easeType">动画曲线</param>
    public static void SetIndexCentered(this ScrollCycleView self, int index = 0, float duration = 0, EaseType easeType = EaseType.Linear)
        {
            self.RemoveMoveAnim();
            
            int itemOffset = Mathf.RoundToInt((float)index / self.fixedCount);
            itemOffset = (index % self.fixedCount) == 0 ? itemOffset : itemOffset - 1;
            
            float centerOffset = (self.IsVertical ? self.itemHeight : self.itemWidth) * (self.fixedCount / 2);
            
            if (self.totalMovableDistance > 0) 
            {
                Vector2 av2 = self.rtfGrid.anchoredPosition;
                Vector2 mv2 = Vector2.zero;
                
                if (self.IsVertical)
                {
                    mv2 = new Vector2(0, Mathf.Clamp(self.itemHeight * itemOffset - centerOffset, 0, self.totalMovableDistance));
                }
                else
                {
                    mv2 = new Vector2(Mathf.Clamp(-self.itemWidth * itemOffset + centerOffset, -self.totalMovableDistance, 0), 0);
                }

                if (duration > 0)
                {
                    DOTweenAnimCurve.Instance.To(0, 1, duration,
                        progress =>
                        {
                            self._SetPosition(Vector2.Lerp(av2, mv2, progress));

                            int curIndexTmp = self.IsVertical ? Mathf.FloorToInt(self.rtfGrid.anchoredPosition.y / self.itemHeight)
                                : Mathf.FloorToInt(-self.rtfGrid.anchoredPosition.x / self.itemWidth);

                            self._SetItemIndex(curIndexTmp);
                            float curGridLen = self.IsVertical ? self.rtfGrid.anchoredPosition.y : -self.rtfGrid.anchoredPosition.x;
                            self.scrScrollBar.SetScrollProgressValue(curGridLen / self.totalMovableDistance);
                        })
                        .OnStart(() =>
                        {
                            self.scrScrollMove.SetActiveTriggerUI(false);
                        })
                        .OnComplete(() =>
                        {
                            self.scrScrollMove.SetActiveTriggerUI(true);
                        })
                        .SetEase(easeType);
                }
                else
                {
                    self._SetPosition(mv2);
                    self._SetItemIndex(itemOffset);
                    
                    float curGridLen = self.IsVertical ? self.rtfGrid.anchoredPosition.y : -self.rtfGrid.anchoredPosition.x;
                    self.scrScrollBar.SetScrollProgressValue(curGridLen / self.totalMovableDistance);
                }
            }
            else
            {
                self._SetPosition(Vector2.zero);
                self._SetItemIndex(0);
                self.scrScrollBar.SetScrollProgressValue(0);
            }
        }

        /// <summary>
        /// 得到这个物体当前列表中的idx
        /// </summary>
        /// <param name="self"></param>
        /// <param name="go"></param>
        /// <returns></returns>
        public static int GetItemDataIndex(this ScrollCycleView self, GameObject go)
        {
            foreach (var kvp in self.dicItemData)
            {
                if (kvp.Key.go == go)
                {
                    return kvp.Value;
                }
            }

            return -1; // 如果没有找到匹配的项，可以根据需求返回适当的默认值
        }

        /// <summary>
        /// 是否激活
        /// </summary>
        /// <param name="self"></param>
        /// <param name="isActive"></param>
        public static void SetActive(this ScrollCycleView self, bool isActive)
        {
            self.rtfRoot.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// 获取激活状态
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>

        public static bool ActiveSelf(this ScrollCycleView self)
        {
            return self.rtfRoot.gameObject.activeSelf;
        }

        /// <summary>
        /// 是否支持超屏幕拖拽 回弹
        /// </summary>
        /// <param name="self"></param>
        /// <param name="useElastic"></param>
        public static void SetUseElastic(this ScrollCycleView self, bool useElastic)
        {
            self.isUseElastic = useElastic;

        }

        /// <summary>
        /// 是否可以触摸滚动
        /// </summary>
        /// <param name="self"></param>
        /// <param name="isScroll"></param>
        public static void SetSupportTouchScroll(this ScrollCycleView self, bool isScroll)
        {

            self.scrScrollMove.SetActiveTriggerUI(isScroll);
        }

        /// <summary>
        /// 设置是否可以拖拽滚动
        /// </summary>
        /// <param name="self"></param>
        /// <param name="canDrag"></param>
        public static void SetCanDrag(this ScrollCycleView self, bool canDrag)
        {
            if (self.scrScrollMove != null)
            {
                self.scrScrollMove.SetCanDrag(canDrag);
            }
        }
    }
}
