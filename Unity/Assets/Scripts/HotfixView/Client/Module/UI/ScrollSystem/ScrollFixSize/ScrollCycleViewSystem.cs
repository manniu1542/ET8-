using ScrollViewUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ET.ScrollCycleView;


namespace ET
{

    [EntitySystemOf(typeof(ScrollCycleView))]
    [FriendOf(typeof(ScrollCycleView))]
    public static partial class ScrollCycleViewSystem
    {

        [EntitySystem]
        private static void Awake(this ScrollCycleView self, Transform rtRoot, Scroll.DragType type)
        {

            #region ui数据的初始化

            self.dragType = type;
            //item容器的初始化
            self.listItemData = new List<List<ItemData>>();
            self.dicItemData = new Dictionary<ItemData, int>();
            self.listItemGOPool = new List<GameObject>();

            //滚动动画相关

            self.isUseElastic = true;



            #endregion


            #region 获取ui组件


            ReferenceCollector rc = rtRoot.GetComponent<ReferenceCollector>();
            self.rtfRoot = rtRoot.GetComponent<RectTransform>();
            self.rtfScrollView = rc.Get<GameObject>("ScrollCycleView")?.GetComponent<RectTransform>();
            self.rtfGrid = rc.Get<GameObject>("grid")?.GetComponent<RectTransform>();
            LayoutGroup Layout = rc.Get<GameObject>("Layout")?.GetComponent<LayoutGroup>();
            RectTransform rtfLayout = Layout?.GetComponent<RectTransform>();
            rtfLayout?.gameObject.SetActive(false);
            self.item = Layout.transform.GetChild(0).gameObject;
            self.item.SetActive(false);



            #endregion


            #region ui组件的初始化（ui适配，滑动事件的绑定） 

            //适配ScrollView贴合父级的区域
            {
                //设置 scroll 的适配大小
                self.rtfScrollView.anchoredPosition = Vector2.zero;
                //设置默认滑动列表尺寸
                self.rtfScrollView.sizeDelta = new Vector2(Mathf.Abs(self.rtfRoot.rect.size.x), Mathf.Abs(self.rtfRoot.rect.size.y));

            }



            //根据layout 适配item的大小，以及相关参数
            {

                var rtfItem = self.item.GetComponent<RectTransform>();
                int fixedCountTmp;
                if (Layout is GridLayoutGroup)
                {
                    GridLayoutGroup glayout = (GridLayoutGroup)Layout;
                    //间距
                    self.spacing = glayout.spacing;
                    //item 的大小设置
                    rtfItem.sizeDelta = glayout.cellSize;
                    //指定排列数量 
                    if (glayout.constraint != GridLayoutGroup.Constraint.Flexible)
                        fixedCountTmp = glayout.constraintCount;
                    else //自动适配
                        fixedCountTmp = -1;
                }
                else if (Layout is VerticalLayoutGroup)
                {
                    VerticalLayoutGroup glayout = (VerticalLayoutGroup)Layout;
                    //间距
                    self.spacing = new Vector2(0, glayout.spacing);
                    //item 的大小设置
                    rtfItem.sizeDelta = new Vector2(rtfLayout.rect.width, rtfItem.sizeDelta.y);
                    fixedCountTmp = 1;
                }
                else
                {
                    HorizontalLayoutGroup glayout = (HorizontalLayoutGroup)Layout;
                    //间距
                    self.spacing = new Vector2(glayout.spacing, 0);
                    //item 的大小设置
                    rtfItem.sizeDelta = new Vector2(rtfItem.sizeDelta.x, rtfLayout.rect.height);

                    fixedCountTmp = 1;
                }


                self.cellSize = rtfItem.sizeDelta;
                self.itemWidth = self.spacing.x + self.cellSize.x;
                self.itemHeight = self.spacing.y + self.cellSize.y;
                self.itemPivot = type == Scroll.DragType.Vertical ? new Vector2(0.5f, 1) : new Vector2(0, 0.5f);


                self.SetFixedCountWithOnePageData(fixedCountTmp);
            }





    
            self.scrScrollWheelRoller = self.AddComponent<ScrollWheelRollerComponent, RectTransform,InputAction>(self.rtfScrollView,null);
            self.scrScrollMove = self.AddComponent<ScrollMoveComponent, RectTransform>(self.rtfScrollView);
            self.scrScrollMove.BindAnimMoveEvent( self._OnUpdatePosition, self._OnCheckOutView);
            self.scrScrollMove.BindTouchEvent(self._ResetBackTween, self._OnUpdatePosition, self._OnCheckOutView);
            self.scrScrollBar = self.AddComponent<ScrollCycleBarComponent>();
            #endregion





        }




        [EntitySystem]
        private static void Destroy(this ScrollCycleView self)
        {
            self.scrScrollBar = null;
            self.scrScrollWheelRoller = null;
            self.scrScrollMove = null;
            self.funUpdateItem = null;
            for (int i = self.listItemGOPool.Count - 1; i >= 0; i--)
            {
                GameObject.Destroy(self.listItemGOPool[i]);
                self.listItemGOPool.RemoveAt(i);
            }

            self.listItemGOPool = null;
            if (self.dicItemData != null)
            {
                self.dicItemData.Clear();
                self.dicItemData = null;
            }

            if (self.listItemData != null)
            {
                self.listItemData.Clear();
                self.listItemData = null;
            }




            self.rtfScrollView = null;

            self.rtfGrid = null;

            self.item = null;
            self.tmpTween = null;
            self.rtfRoot = null;
        }




        /// <summary>
        ///  根据滑动类型设置（列数 或 行数)，-1默认是 根据ui自动适配排列 （列数 或 行数)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="fixedCount"></param>
        private static void SetFixedCountWithOnePageData(this ScrollCycleView self, int fixedCount = -1)
        {
            //自动填满
            if (fixedCount == -1)
            {
                float len = self.dragType == Scroll.DragType.Vertical ? self.rtfScrollView.sizeDelta.x + self.spacing.x : self.rtfScrollView.sizeDelta.y + self.spacing.y;

                fixedCount = self.dragType == Scroll.DragType.Vertical ? ((int)(len / self.itemWidth)) : ((int)(len / self.itemHeight));
                fixedCount = Math.Max(fixedCount, 1);
            }
            self.fixedCount = fixedCount;
            switch (self.dragType)
            {
                case Scroll.DragType.Vertical:
                    // 设置 上测 为基准点
                    self.rtfGrid.anchorMin = new Vector2(0.5f, 1f);
                    self.rtfGrid.anchorMax = new Vector2(0.5f, 1f);
                    self.rtfGrid.pivot = new Vector2(0.5f, 1f);
                    // 设置 宽度为 
                    self.rtfGrid.sizeDelta = new Vector2(self.itemWidth * (self.fixedCount - 1), 0);
                    self.OnePageOneFixeditemCount = Mathf.CeilToInt(self.rtfScrollView.sizeDelta.y / self.itemHeight + 1);
                    self.v2OnePageOneFixedTotalItemSize = new Vector2(0f, -self.itemHeight * self.OnePageOneFixeditemCount);

                    break;
                case Scroll.DragType.Horizontal:


                    self.rtfGrid.anchorMin = new Vector2(0f, 0.5f);
                    self.rtfGrid.anchorMax = new Vector2(0f, 0.5f);
                    self.rtfGrid.pivot = new Vector2(0f, 0.5f);
                    self.rtfGrid.sizeDelta = new Vector2(0f, self.itemHeight * (self.fixedCount - 1));
                    self.OnePageOneFixeditemCount = Mathf.CeilToInt(self.rtfScrollView.sizeDelta.x / self.itemWidth + 1);
                    self.v2OnePageOneFixedTotalItemSize = new Vector2(self.itemWidth * self.OnePageOneFixeditemCount, 0f);



                    break;
                default:
                    break;
            }


            self.listItemData.Clear();

            //初始化 显示ui的 item 
            for (int j = 0; j < self.fixedCount; j++)
            {
                List<ScrollCycleView.ItemData> itemList = new List<ScrollCycleView.ItemData>();
                for (int i = 0; i < self.OnePageOneFixeditemCount; i++)
                {
                    itemList.Add(new ScrollCycleView.ItemData());
                }
                self.listItemData.Add(itemList);
            }
        }














    }

}
