using System;
using UnityEngine;
using UnityEngine.UI;
using static ET.ScrollCustomRange;

namespace ET
{
    [EntitySystemOf(typeof(ScrollCustomRange))]
    [FriendOf(typeof(ScrollCustomRange))]
    [FriendOf(typeof(ScrollCustomRangeItemComponent))]

    public static partial class ScrollCustomRangeSystem
    {

        [EntitySystem]
        private static void Awake(this ScrollCustomRange self, Transform rtRoot, Scroll.DragType type)
        {

            #region ui数据的初始化
            self.dragType = type;
            //item容器的初始化
            self.listItemGO = new();
            self.dicAllItemData = new();
            self.listGamePoolItemGO = new();
            //滚动动画相关



            self.isUseElastic = true;



            #endregion


            #region 获取ui组件


            ReferenceCollector rc = rtRoot.GetComponent<ReferenceCollector>();
            self.rtfRoot = rtRoot.GetComponent<RectTransform>();
            self.rtfScrollView = rc.Get<GameObject>("ScrollView")?.GetComponent<RectTransform>();
            self.rtfGrid = rc.Get<GameObject>("grid")?.GetComponent<RectTransform>();
            LayoutGroup Layout = rc.Get<GameObject>("Layout")?.GetComponent<LayoutGroup>();
            RectTransform rtfLayout = Layout?.GetComponent<RectTransform>();
            rtfLayout.gameObject.SetActive(false);
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
               
              
                if (Layout is VerticalLayoutGroup)
                {
                    VerticalLayoutGroup glayout = (VerticalLayoutGroup)Layout;
                    //间距
                    self.spacing = new Vector2(0, glayout.spacing);
                    //item 的大小设置
                    rtfItem.sizeDelta = new Vector2(rtfLayout.rect.width, rtfItem.sizeDelta.y);
                    
                }

                self.itemMinSizeDefault = rtfItem.sizeDelta;


                self.v2ViewSize = self.rtfScrollView.sizeDelta;


                self.itemRemoveSizeTolerant = self.itemMinSizeDefault.y * 0.2f + self.spacing.y;

                self.onePageMaxItemCount = Mathf.CeilToInt(self.v2ViewSize.y / self.itemMinSizeDefault.y);

                self.InitViewAndItemUIProperty();

                


            }






            #endregion

            self.scrScrollMove = self.AddComponent<ScrollMoveComponent, RectTransform>(self.rtfScrollView);
            self.scrScrollMove.BindAnimMoveEvent(self._OnAnimDragMoving, self._OnAnimDragMoveEnd);

            self.scrScrollMove.BindTouchEvent(self.OnDragStart, self.OnDragMove, self.OnDragEnd);


            //生成对象池物体。
            self.ResetPoolGO();

            //生成 未初始化做准备的item
            GameObject childItem = GameObject.Instantiate<GameObject>(self.item, self.rtfGrid.transform);
            self.scrItemCalculation = self.AddChild<ScrollCustomRangeItemComponent, Transform>(childItem.transform);
            self.scrItemCalculation.SetOutViewPos();
            self.scrItemCalculation.SetIsUsing(false);
      


        }
    
        
        
        

        [EntitySystem]
        private static void Destroy(this ScrollCustomRange self)
        {
            self.RemoveMoveAnim();
            self.scrScrollMove = null;

            self.rtfScrollView = null;

            self.rtfGrid = null;

            self.item = null;
            self.tmpTween = null;
            self.rtfRoot = null;



            self.isAnimMoving = false;
            self.isDrag = false;
            self.isUpdateAwait = false;
            self.isCheckContinueItem = false;

        }

        [EntitySystem]
        private static void Update(this ScrollCustomRange self)
        {

            if (self.isInitPageIteming) return;





            self.UpdateMoveTargetItemAnim();

            if (self.IsUpdateItem)
                self.UpdateItem();



        }
        private static void Update2(this ScrollCustomRange self)
        {



            switch (self.enumUpdateType)
            {
                case ScrollCustomUpdateType.Init:


                    break;
                case ScrollCustomUpdateType.Move:


                    break;
                case ScrollCustomUpdateType.None:


                    break;
                default:
                    break;
            }

        }




        //滑动的时候检测。 第一个，以及最后一个 元素。或者 只检测 某一个
        public static void UpdateItem(this ScrollCustomRange self)
        {

            //等待过程 直接return出去
            if (self.isUpdateAwait) return;


            self.isUpdateAwait = true;

            self.isCheckContinueItem = false;

            //v2Tmp x表示这个item最top，  y表示item 最botton
            Vector2 v2Tmp;


            float curGridY = self.rtfGrid.anchoredPosition.y;
            if (self.listItemGO.First != null && self.listItemGO.First.Value != null)
            {
                var curFirstScr = self.listItemGO.First.Value;
                v2Tmp.x = curFirstScr.data.posOrign - curGridY;
                v2Tmp.y = v2Tmp.x + curFirstScr.data.size;



                //添加头部
                if (curFirstScr.data.index != self.startItemIdx && v2Tmp.x > 0)
                {

                     self.OnAddHead();
                    self.isCheckContinueItem = true;

                }

                //移除头部 （curFirstScr.data 会被移除掉）
                if (v2Tmp.y + self.itemRemoveSizeTolerant < 0)
                {


                    self.OnRemoveHead();
                }




            }

            if (self.listItemGO.Last != null && self.listItemGO.Last.Value != null)
            {
                var curLastScr = self.listItemGO.Last.Value;
                v2Tmp.x = curLastScr.data.posOrign - curGridY;
                v2Tmp.y = v2Tmp.x + curLastScr.data.size;
                //添加尾部

                if (curLastScr.data.index != self.endItemIdx && v2Tmp.y < self.v2ViewSize.y)
                {
                     self.OnAddEnd();
                    self.isCheckContinueItem = true;
                }

                //移除尾部
                if (v2Tmp.x > self.v2ViewSize.y + self.itemRemoveSizeTolerant)//
                {
                    self.OnRemoveEnd();
                }
            }
            self.isUpdateAwait = false;

        }



        /// <summary>
        /// 初始化 视窗ui 以及 item-ui的锚点等ui 属性
        /// </summary>
        /// <param name="self"></param>
        private static void InitViewAndItemUIProperty(this ScrollCustomRange self)
        {
            
            switch (self.dragType)
            {
                case Scroll.DragType.Vertical:

                    //从上往下
                    Vector2 v2 = new Vector2(0.5f, 1);

                    self.rtfGrid.anchorMin = Vector2.up;
                    self.rtfGrid.anchorMax = Vector2.one;
                    self.rtfGrid.pivot = v2;
                    self.rtfGrid.sizeDelta = Vector2.zero;
                    self.rtfGrid.anchoredPosition = Vector2.zero;


                    var rtfItem = self.item.GetComponent<RectTransform>();
                    rtfItem.anchorMin = v2;
                    rtfItem.anchorMax = v2;
                    rtfItem.pivot = v2;
                    rtfItem.anchoredPosition = Vector2.zero;
                    rtfItem.sizeDelta = new Vector2(rtfItem.sizeDelta.x, self.itemMinSizeDefault.y);

                    
                    var layout = self.item.GetComponent<VerticalLayoutGroup>();
                    layout.childControlHeight = true;
                    layout.childControlWidth = false;
                    layout.childForceExpandHeight = false;
                    layout.childForceExpandWidth = false;
                    layout.useGUILayout = false;
                    layout.useGUILayout = false;
                    
                    
                    
                    break;
                case Scroll.DragType.Horizontal:



                    break;
                default:
                    break;
            }


        }







        public static void SetRebuldItemData(this ScrollCustomRange self, int startIdx, int endIdx, bool isRebuldPos = true, bool isRebulidSize = true)
        {

            foreach (var item in self.dicAllItemData)
            {
                if (item.Key >= startIdx && item.Key <= endIdx)
                {
                    if (isRebuldPos)
                        item.Value.isRebuildPos = true;
                    if (isRebulidSize)
                        item.Value.isRebuildSize = true;
                }
            }


        }




        public static void Destory(this ScrollCustomRange self)
        {
            self.dicAllItemData = null;
            self.listItemGO = null;
            self.listGamePoolItemGO = null;
        }
        public static ScrollCustomRangeItemComponent GetChildItem(this ScrollCustomRange self, int ItemIdx)
        {

            ItemData data = self.GetCreateItemData(ItemIdx);
            ScrollCustomRangeItemComponent scrollViewItemComponent = self.PopGOPool();


             scrollViewItemComponent.SetUpdateItem(data);
            if (!scrollViewItemComponent.IsUsing)
            {
                Log.Error("提前回收了！！！");
                return null;
            }
            return scrollViewItemComponent;
        }


        public static void  OnAddHead(this ScrollCustomRange self)
        {
            var firstItem = self.listItemGO.First.Value;
            int newFirstItemIdx = firstItem.data.index - 1;
            if (newFirstItemIdx >= self.startItemIdx)
            {
                //----first 不为 数据头---在data中做了
                ScrollCustomRangeItemComponent obj =  self.GetChildItem(newFirstItemIdx);

                if (obj != null)
                    self.listItemGO.AddFirst(obj);


            }
        }


        public static void OnRemoveHead(this ScrollCustomRange self)
        {

            var firstItem = self.listItemGO.First.Value;
            int newFirstItemIdx = firstItem.data.index + 1;


            if (firstItem != null && newFirstItemIdx <= self.endItemIdx)
            {

                self.PushGOPool(firstItem);
                self.listItemGO.RemoveFirst();

            }
        }

        public static void  OnAddEnd(this ScrollCustomRange self)
        {


            var endItem = self.listItemGO.Last.Value;
            int newEndItemIdx = endItem.data.index + 1;
            if (newEndItemIdx <= self.endItemIdx)
            {
                //----end 不为 数据尾在data中做了
                ScrollCustomRangeItemComponent obj =  self.GetChildItem(newEndItemIdx);
                
                if (obj != null)
                    self.listItemGO.AddLast(obj);
            }
        }




        public static void OnRemoveEnd(this ScrollCustomRange self)
        {

            var endItem = self.listItemGO.Last.Value;
            int newEndIdx = endItem.data.index - 1;
            if (endItem != null && newEndIdx >= self.startItemIdx)
            {
                self.PushGOPool(endItem);
                self.listItemGO.RemoveLast();

            }

        }








    }

}
