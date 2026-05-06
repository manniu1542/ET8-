using System.Collections.Generic;
using ET.Scroll;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ET.ScrollCustomRange;


namespace ET
{
    [EntitySystemOf(typeof(ScrollCustomRangeItemComponent))]
    [FriendOf(typeof(ScrollCustomRangeItemComponent))]
    [FriendOf(typeof(ScrollCustomRange))]
    
    public static partial class ScrollCustomRangeItemComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this ScrollCustomRangeItemComponent self)
        {
            self.data = null;
            self.transform = null;

            self.go = null;

            self.rtf = null;
            self.scrParent = null;


            self.contenSizeFit = null;
            self.layoutGroup = null;
        }


        #region 方法

        [EntitySystem]
        private static void Awake(this ScrollCustomRangeItemComponent self, Transform tf)
        {
            self.transform = tf;

            self.go = tf.gameObject;

            self.rtf = self.transform.GetComponent<RectTransform>();
            self.scrParent = self.Parent as ScrollCustomRange;


            self.contenSizeFit = self.transform.GetComponent<ContentSizeFitter>();
            self.layoutGroup = self.transform.GetComponent<LayoutGroup>();


            //收集所有会变动的 textmeshpro，  
            self.arrTxtNeedUpdateMesh = new List<TextMeshProUGUI>();
            ReferenceCollector rc = self.go.GetComponent<ReferenceCollector>();
            foreach (var itemTmp in rc.data)
            {
                var go = itemTmp.gameObject as GameObject;
                var txt = go.GetComponent<TextMeshProUGUI>();
                if (txt != null) self.arrTxtNeedUpdateMesh.Add(txt);
            }
        }


        public static void ForceRebuildLayout(this ScrollCustomRangeItemComponent self)
        {
            //TODO：（1）textmeshpro 强制刷新下。（2）图片的大小需不需要setnative呢。

            foreach (var textMeshProUGUI in self.arrTxtNeedUpdateMesh)
            {
                textMeshProUGUI.ForceMeshUpdate();
            }

            
            LayoutRebuilder.ForceRebuildLayoutImmediate(self.rtf);
        }


        public static void ReBuildSize(this ScrollCustomRangeItemComponent self)
        {
            if (self.data.isRebuildSize)
            {
                self.data.isRebuildSize = false;
                //大小
                self.ForceRebuildLayout();
                //等待过程中 被回收了
                if (!self.IsUsing)
                {
                    Log.Info("即便取消了上面的 等待回调！下面依然会执行！！！！");
                    return;
                }

                self.data.size = self.rtf.sizeDelta.y;
            }
        }

        public static void ReBuildPos(this ScrollCustomRangeItemComponent self)
        {
            //位置
            if (self.data.isRebuildPos)
            {
                self.data.isRebuildPos = false;


                var last = self.scrParent.GetItemData(self.data.index - 1);
                if (last != null && !last.isRebuildPos)
                {
                    self.data.posOrign = last.posOrign + last.size + self.scrParent.spacing.y;
                }
                else
                {
                    var next = self.scrParent.GetItemData(self.data.index + 1);
                    if (next == null || next.isRebuildPos)
                    {
                        throw new System.Exception("位置更新出错！");
                    }

                    self.data.posOrign = next.posOrign - self.scrParent.spacing.y - self.data.size;
                }


                self.scrParent.SetGridPosLimit(self.data);
            }
        }


        public static void _UpdateItemUI(this ScrollCustomRangeItemComponent self)
        {
            self.ReBuildSize();
            if (!self.IsUsing) return;
            self.ReBuildPos();


            self.rtf.anchoredPosition = new Vector2(0, -self.data.posOrign);

            //虽然item上面有layout ，但是依然需要重置下当前的 layout的
            self.rtf.sizeDelta = new Vector2(self.scrParent.v2ViewSize.x, self.data.size);


            // int uiIdx = self.scrParent.SwitchItemIdxToUIIdx(self.data.index);
            //Log.Info(uiIdx + "下标 --" + "大小" + self.data.size + "位置：" + self.rtf.anchoredPosition.y);
        }

        /// <summary>
        /// 设置 某个item的激活信息以及回调
        /// </summary>
        /// <param name="self"></param>
        /// <param name="index">滑动条的某个item的 idex编号</param>
        /// <param name="item"></param>
        public static void SetUpdateItem(this ScrollCustomRangeItemComponent self, ItemData data)
        {
            self.data = data;
            self.CallItemFun();
            self._UpdateItemUI();
        }

        #region 初始化计算使用的Item方法

        public static float GetItemSize(this ScrollCustomRangeItemComponent self, ItemData data)
        {
            self.data = data;

            self.CallItemFun();
            self.ReBuildSize();

            return self.data.size;
        }

        public static void SetOutViewPos(this ScrollCustomRangeItemComponent self)
        {
            self.rtf.anchoredPosition = new Vector2(-self.scrParent.v2ViewSize.x, 0);
        }

        #endregion

        public static void CallItemFun(this ScrollCustomRangeItemComponent self)
        {
            int uiIdx = self.scrParent.SwitchItemIdxToUIIdx(self.data.index);

            self.scrParent?.funUpdateItem(uiIdx, self.go);
        }

        /// <summary>
        /// 设置 某个item的激活信息以及回调
        /// </summary>
        /// <param name="self"></param>
        /// <param name="index">滑动条的某个item的 idex编号</param>
        /// <param name="item"></param>
        public static void SetIsUsing(this ScrollCustomRangeItemComponent self, bool isUsing)
        {
            //设置激活 
            self.IsUsing = isUsing;
            
            //不用了
            if (!self.IsUsing)
            {
                //表示正在 等待刷新Item大小过程中，被回收，清理

                if (self.data!= null)
                    self.data.isRebuildSize = true;

                self.data = null;
            }

            self.go.SetActive(isUsing);
            if (!self.IsUsing)
                self.rtf.sizeDelta = self.scrParent.itemMinSizeDefault;
        }

        #endregion
    }
}