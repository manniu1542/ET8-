
using ScrollViewUI;
using System;
using UnityEngine;
using UnityEngine.UI;
using ItemData = ET.ScrollCustomRange.ItemData;
using Vector2 = UnityEngine.Vector2;

namespace ET
{

    /// <summary>
    /// 提供的功能 （放置的列数量，刷新当前item，设置列表位置）
    /// </summary>
    [FriendOf(typeof(ScrollCustomRange))]
    [FriendOf(typeof(ScrollCustomRangeItemComponent))]
    [FriendOf(typeof(ScrollMoveComponent))]
    public static class ScrollCustomRangeProvidFunSystem
    {
        /// <summary>
        /// 移除动画移动相关的 组件及数据
        /// </summary>
        /// <param name="self"></param>
        public static void RemoveMoveAnim(this ScrollCustomRange self)
        {

            self.scrScrollMove?.ForceRemoveDragAnim();
            self.isAnimMoving = false;
            self._SetBackTween();
        }



        /// <summary>
        ///  对应 数据 显示 ，刷新 当前滑动item    TODO:热重载之前的数据，刷新列表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="initDataCount"></param>
        /// <param name="updateFunc"></param>
        public static void InitScroll(this ScrollCustomRange self, int initDataCount, Action<int, GameObject> updateFunc, int initUIIdx = 0)
        {
            self.isInitPageIteming = true;
            //设置初始位置的idx
            self.startItemIdx = 0;
            //触摸关闭。 shu'ru

            //动画组件移除

            self.funUpdateItem = updateFunc;
            //初始化对象池， 
            self.dataCount = initDataCount;


            self.v2GridPosLimit = Vector2.zero;
            //调用 更新方法
            self.rtfGrid.anchoredPosition = Vector2.zero;

            self.v2DicStartEndItemIdx = Vector2Int.zero;
            //清理数据
            self.dicAllItemData.Clear();
            int dirItemIdx = self.SwitchUIIdxToItemIdx(initUIIdx);
            

            self.ForceStopUpdateItem();
            self.scrScrollMove.SetActiveTriggerUI(false);

            dirItemIdx = Mathf.Clamp(dirItemIdx, self.startItemIdx, self.endItemIdx);

            //会打断之前正在计算的 InitScroll
            self.scrItemCalculation.SetIsUsing(false);
            //回收 页面上已经存在的 item
            if (self.listItemGO.Count > 0)
            {
                foreach (var item in self.listItemGO)
                {
                    self.PushGOPool(item);
                }

                self.listItemGO.Clear();
            }

            if (self.dataCount > 0)
            {

                self.scrItemCalculation.SetIsUsing(true);
                //重置页面的idx
                bool isOnePage = self.dataCount <= self.onePageMaxItemCount && (!( self.IsTopToBottonEnoughOnePage(self.startItemIdx)));
                if (!self.scrItemCalculation.IsUsing) return;
                //没有再次初始化滚动列表了
                bool isNotAgainInitScroll;
                //检查所有item不满足一页的时候，
                if (isOnePage)
                {
                    isNotAgainInitScroll =  self.InitItemTB(self.startItemIdx);
                    if (!isNotAgainInitScroll) return;
                }
                else
                {

                    bool IsInitT2B =  self.IsTopToBottonEnoughOnePage(dirItemIdx);
                    if (!self.scrItemCalculation.IsUsing) return;

                    if (IsInitT2B)
                        isNotAgainInitScroll =  self.InitItemTB(dirItemIdx);
                    else
                        isNotAgainInitScroll =  self.InitItemBTEnd();

                    if (!isNotAgainInitScroll) return;

                }


            }



            self.scrScrollMove.SetActiveTriggerUI(true);

            self.scrItemCalculation.SetIsUsing(false);
            self.isInitPageIteming = false;
        }
        /// <summary>
        /// 强制停止更新Item 以及 更新里面item的生成等待逻辑
        /// </summary>
        public static void ForceStopUpdateItem(this ScrollCustomRange self)
        {
            //停止Update可能继续生成Item的逻辑,


            self.RemoveMoveAnim();
            self.isAnimMoving = false;
            self.isDrag = false;
            self.isUpdateAwait = false;
            self.isCheckContinueItem = false;

            self.targetAnimIdx = -1;

        }


        public static ItemData GetCreateItemData(this ScrollCustomRange self, int ItemIdx)
        {
            if (!self.dicAllItemData.TryGetValue(ItemIdx, out ItemData data))
            {
                data = new ItemData() { index = ItemIdx, isRebuildPos = true, isRebuildSize = true };
                self.dicAllItemData.Add(data.index, data);
            }
            return data;
        }
        public static ItemData GetItemData(this ScrollCustomRange self, int itemIdx)
        {
            self.dicAllItemData.TryGetValue(itemIdx, out ItemData data);
            return data;
        }
        /// <summary>
        /// 可以使用该Item的位置了
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemIdx"></param>
        /// <returns></returns>
        public static bool IsItemRebuildPos(this ScrollCustomRange self, int itemIdx)
        {
            var data = self.GetItemData(itemIdx);
            return data != null && (!data.isRebuildPos);
        }

        public static int SwitchItemIdxToUIIdx(this ScrollCustomRange self, int ItemIdx)
        {
            return ItemIdx - self.startItemIdx;
        }
        public static int SwitchUIIdxToItemIdx(this ScrollCustomRange self, int uiIdx)
        {
            return uiIdx + self.startItemIdx;
        }

        /// <summary>
        /// 可不可以从上往下排列
        /// </summary>
        /// <param name="self"></param>
        /// <param name="dirItemIdx"></param>
        /// <returns></returns>
        private static bool IsTopToBottonEnoughOnePage(this ScrollCustomRange self, int itemIdx)
        {

            //临时 显示的item所有高度
            float allItemHeightTmp = 0;
            ItemData itemDaTmp;
            bool isCan = true;

            while (allItemHeightTmp <= self.v2ViewSize.y)
            {

                if (itemIdx >= self.endItemIdx + 1)
                {
                    isCan = false;
                    break;
                }
                itemDaTmp = self.GetCreateItemData(itemIdx);

                //添加 他的高度
                allItemHeightTmp +=  self.scrItemCalculation.GetItemSize(itemDaTmp);
                if (!self.scrItemCalculation.IsUsing) return false;//被提前回收了，重置item了

                //添加他的间距
                allItemHeightTmp += (itemIdx == self.endItemIdx) ? 0 : self.spacing.y;

                ++itemIdx;
            }


            return isCan;
        }


        /// <summary>
        /// 从上往下生成item
        /// </summary>
        /// <param name="self"></param>
        /// <param name="idxTmp"></param>
        /// <returns></returns>
        private static bool InitItemTB(this ScrollCustomRange self, int realyInitIdx)
        {


            ScrollCustomRangeItemComponent scrItem = null;
            int idx = realyInitIdx;
            //临时 显示的item所有高度
            float allItemHeightTmp = 0;
            while (idx < self.dataCount && allItemHeightTmp <= self.v2ViewSize.y)
            {
                scrItem = self.PopGOPool();

                ItemData data = self.GetCreateItemData(idx);

                if (idx == realyInitIdx)
                {
                    data.isRebuildPos = false;
                    data.posOrign = 0;
                    self.SetGridPosLimit(data);
                }



                self.listItemGO.AddLast(scrItem);

                 scrItem.SetUpdateItem(data);
                if (!scrItem.IsUsing)
                {

                    return false;//"提前回收了，报错！00"
                }


                allItemHeightTmp += data.size;
                allItemHeightTmp += (idx == self.endItemIdx ? 0 : self.spacing.y);
                ++idx;
            }

            return true;
        }


        /// <summary>
        /// 从下往上生成item
        /// </summary>
        /// <param name="self"></param>
        /// <param name="realyInitIdx"></param>
        /// <returns></returns>
        private static bool InitItemBTEnd(this ScrollCustomRange self)
        {

            ScrollCustomRangeItemComponent scrItem = null;
            int idx = self.endItemIdx;
            //临时 显示的item所有高度
            float allItemHeightTmp = 0;
            while (allItemHeightTmp <= self.v2ViewSize.y)
            {
                scrItem = self.PopGOPool();

                ItemData data = self.GetCreateItemData(idx);

                if (idx == self.endItemIdx)
                {
                    data.isRebuildPos = false;
                    data.posOrign = self.v2ViewSize.y -  scrItem.GetItemSize(data);
                    if (!scrItem.IsUsing)
                    {

                        return false;//"提前回收了，报错！00"

                    }
                    self.SetGridPosLimit(data);
                }


                self.listItemGO.AddFirst(scrItem);
                 scrItem.SetUpdateItem(data);
                if (!scrItem.IsUsing)
                {

                    return false;//"提前回收了，报错！00"

                }



                allItemHeightTmp += data.size;
                allItemHeightTmp += (idx == self.endItemIdx ? 0 : self.spacing.y);
                --idx;
            }
            return true;
        }
        public static void SetGridPosLimitWithStartEnd(this ScrollCustomRange self, float startPos, float endPos)
        {
            self.v2GridPosLimit.x = startPos;
            //少于一页
            if (Mathf.Abs(endPos - startPos) < self.v2ViewSize.y)
            {
                self.v2GridPosLimit.y = self.v2GridPosLimit.x;
            }
            else //不少于一页 尾部就是，
            {
                self.v2GridPosLimit.y = endPos - self.v2ViewSize.y;
            }
        }

        /// <summary>
        /// 设置格子的位置限制  (只要修改格子的位置的时候都需要调用这个方法)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="data"></param>
        public static void SetGridPosLimit(this ScrollCustomRange self, ItemData data)
        {

            self.v2DicStartEndItemIdx.x = Mathf.Min(self.v2DicStartEndItemIdx.x, data.index);
            self.v2DicStartEndItemIdx.y = Mathf.Max(self.v2DicStartEndItemIdx.y, data.index);

            //设置开始末尾的限制了

            float startPos, endPos;
            //首部
            if (data.index == self.startItemIdx)
            {
                startPos = data.posOrign;

                //有的话就要计算 可滑动区域
                if (self.IsItemRebuildPos(self.endItemIdx))
                {
                    ItemData endData = self.GetItemData(self.v2DicStartEndItemIdx.y);
                    endPos = endData.posOrign + endData.size;

                    self.SetGridPosLimitWithStartEnd(startPos, endPos);

                }
                else
                {
                    self.v2GridPosLimit.x = startPos;
                }


            }//尾部
            else if (data.index == self.endItemIdx)
            {
                endPos = data.posOrign + data.size;
                //此时如果有 头部idx
                if (self.IsItemRebuildPos(self.startItemIdx))
                {
                    ItemData startData = self.GetItemData(self.startItemIdx);
                    startPos = startData.posOrign;
                    self.SetGridPosLimitWithStartEnd(startPos, endPos);

                }
                else //还没有头部（表示肯定超过一页了）
                {
                    self.v2GridPosLimit.y = endPos - self.v2ViewSize.y;

                }

            }

        }





        /// <summary>
        /// 刷新列表， TODO:热重载之前的数据，刷新列表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="dataCount"></param>
        /// <param name="updateFunc"></param>
        /// <param name="isTopToBottom"></param>
        /// <param name="SpawanItemPreCount"></param>
        public static void AddEndDataLen(this ScrollCustomRange self, int addCount = 1)
        {
            if (addCount <= 0) return;

            bool isEmpty = self.dataCount == 0;

            if (isEmpty)
            {

                self.InitScroll(addCount, self.funUpdateItem, addCount);

            }
            else
            {
                //初始化对象池， 
                self.dataCount += addCount;
                //避免不满足一页的item刷新
                self.isCheckContinueItem = true;

            }

            // Log.Info($"--添加尾部开始idx是：{self.startItemIdx},  总数是：{self.dataCount}");


        }

        /// <summary>
        /// 添加头部item的长度
        /// </summary>
        /// <param name="len"></param>
        public static void AddStartDataLen(this ScrollCustomRange self, int addCount = 1)
        {
            if (addCount <= 0) return;

            bool isEmpty = self.dataCount == 0;

            if (isEmpty)
            {
                self.InitScroll(addCount, self.funUpdateItem, self.startItemIdx);

            }
            else
            {
                self.startItemIdx -= addCount;
                self.dataCount += addCount;

                //避免不满足一页的item刷新
                self.isCheckContinueItem = true;


            }
            //   Log.Info($"--添加头部开始idx是：{self.startItemIdx},  总数是：{self.dataCount}");

        }

        /// <summary>
        /// 移除头部item的长度
        /// </summary>
        /// <param name="len"></param>
        public static void RemoveStartDataLen(this ScrollCustomRange self, int removeCount = 1)
        {

            if (self.dataCount <= removeCount)
            {
                throw new Exception("容器里面，不满足移除的条件！");
            }

            self.startItemIdx += removeCount;
            self.dataCount -= removeCount;


            //避免不满足一页的item刷新
            self.isCheckContinueItem = true;


            //有的话就要计算 可滑动区域
            if (self.IsItemRebuildPos(self.startItemIdx))
            {
                ItemData data = self.GetItemData(self.startItemIdx);
                self.SetGridPosLimit(data);
            }

            //检测是否超屏了移除
            self.CheckOutViewSpring(true);

            // Log.Info($"--移除头部开始idx是：{self.startItemIdx},  总数是：{self.dataCount}");
        }


        public static int GetCurDataLen(this ScrollCustomRange self)
        {
            //初始化对象池， 
            return self.dataCount;
        }


        #region 设置移动 


        /// <summary>
        /// 设置index
        /// </summary>
        /// <param name="self"></param>
        /// <param name="uiIdx"></param>
        /// <returns></returns>
        public static void SetMoveIndex(this ScrollCustomRange self, int uiIdx, float moveItemSpeedRatio = 1)
        {
            if (self.dataCount == 0 || self.isInitPageIteming) return;
            if (moveItemSpeedRatio <= 0)
            {
                throw new Exception("moveItemSpeedRatio 必须为正数");
            }


            int itemIdx = self.SwitchUIIdxToItemIdx(uiIdx);
            itemIdx = Mathf.Clamp(itemIdx, self.startItemIdx, self.endItemIdx);



            self.ForceStopUpdateItem();

            ItemData data;

            //速度 跟这个item最小尺寸成正相关
            self.animMoveTargetSpeed = moveItemSpeedRatio * self.itemMinSizeDefault.y;

            //有这个 item位置e
            if (self.IsItemRebuildPos(itemIdx))
            {

                data = self.GetItemData(itemIdx);

                self.SetMoveTargetItemAnim(data);
            }
            else
            {
                //找出 最接近的第一个数据
                int distanceToX = Math.Abs(self.v2DicStartEndItemIdx.x - itemIdx);
                int distanceToY = Math.Abs(self.v2DicStartEndItemIdx.y - itemIdx);
                int nealyIdx = distanceToX < distanceToY ? self.v2DicStartEndItemIdx.x : self.v2DicStartEndItemIdx.y;


                //调整速度方向
                self.animMoveTargetSpeed *= nealyIdx < itemIdx ? 1 : -1;
                self.targetAnimIdx = itemIdx;





            }





        }
        public static void UpdateMoveTargetItemAnim(this ScrollCustomRange self)
        {
            if (self.targetAnimIdx == -1) return;

            if (self.targetAnimIdx >= self.v2DicStartEndItemIdx.x && self.targetAnimIdx <= self.v2DicStartEndItemIdx.y)
            {


                var data = self.GetItemData(self.targetAnimIdx);
                self.SetMoveTargetItemAnim(data);
                self.targetAnimIdx = -1;
            }
            else
            {
                Vector2 pos = self.rtfGrid.anchoredPosition;
                //修正 参数 ，在竖滑动条 里  横向移动参数 置为0 . 横向滑动条同理

                pos.y += self.animMoveTargetSpeed * Time.unscaledDeltaTime;
                self._SetPosition(pos);
            }
        }


        /// <summary>
        /// 是否激活
        /// </summary>
        /// <param name="self"></param>
        /// <param name="isActive"></param>
        public static void SetMoveTargetItemAnim(this ScrollCustomRange self, ItemData data, Action funComplate = null)
        {

            float posY = data.posOrign;

            if (self.IsItemRebuildPos(self.startItemIdx))
            {

                posY = Mathf.Max(self.v2GridPosLimit.x, posY);
            }

            if (self.IsItemRebuildPos(self.endItemIdx))
            {
                posY = Mathf.Min(self.v2GridPosLimit.y, posY);
            }


            float time = MathF.Abs(posY - self.rtfGrid.anchoredPosition.y) / MathF.Abs(self.animMoveTargetSpeed);
            self._SetBackTween(DOTweenAnimCurve.Instance.DOAnchorPosY(self.rtfGrid, posY, time, v =>
            {
                self.isCheckContinueItem = true;
            }).OnComplete(funComplate));
        }




        #endregion








        /// <summary>
        /// 是否激活
        /// </summary>
        /// <param name="self"></param>
        /// <param name="isActive"></param>
        public static void SetActive(this ScrollCustomRange self, bool isActive)
        {
            self.rtfRoot.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// 获取激活状态
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>

        public static bool ActiveSelf(this ScrollCustomRange self)
        {
            return self.rtfRoot.gameObject.activeSelf;
        }


        /// <summary>
        /// 是否支持超屏幕拖拽 回弹
        /// </summary>
        /// <param name="self"></param>
        /// <param name="useElastic"></param>
        public static void SetUseElastic(this ScrollCustomRange self, bool useElastic)
        {
            self.isUseElastic = useElastic;
        }



        /// <summary>
        /// 初始化完成（防止还在异步加载 初始页面的item时候。就被其他 动画移动，末尾添加新的聊天 打断）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="useElastic"></param>
        public static bool IsInitFinish(this ScrollCustomRange self)
        {
            return !self.isInitPageIteming;
        }






        public static void NeedResetImageSize(Image image)
        {
            // 暂时没有这个需求 。图片大小自定义的
         
            if (image.sprite != null)
            { 
                image.SetNativeSize(); // 设置基础尺寸
                var layoutElement = image.GetComponent<LayoutElement>();
                if (layoutElement == null) layoutElement = image.gameObject.AddComponent<LayoutElement>();
            
                layoutElement.preferredWidth =   image.sprite.texture.width;
                layoutElement.preferredHeight =  image.sprite.texture.height;
            }
        }
        public static void NeedResetImageSize(RawImage image)
        {
            // 暂时没有这个需求 。图片大小自定义的
         
            if (image.texture != null)
            { 
                image.SetNativeSize(); // 设置基础尺寸
                var layoutElement = image.GetComponent<LayoutElement>();
                if (layoutElement == null) layoutElement = image.gameObject.AddComponent<LayoutElement>();
            
                layoutElement.preferredWidth =   image.texture.width;
                layoutElement.preferredHeight =  image.texture.height;
            }
        }




    }

}
