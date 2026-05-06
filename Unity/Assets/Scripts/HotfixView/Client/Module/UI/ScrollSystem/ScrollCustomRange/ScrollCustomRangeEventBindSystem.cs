using ScrollViewUI;
using UnityEngine;

namespace ET
{

    /// <summary>
    /// 滚动列表 所绑定的事件  （滑动区域绑定的，滑动块绑定的，滑动区域惯性移动的）
    /// </summary>
    [FriendOf(typeof(ScrollCustomRange))]
    [FriendOf(typeof(ScrollCustomRangeItemComponent))]
    [FriendOf(typeof(ScrollMoveComponent))]
    public static class ScrollCustomRangeEventBindSystem
    {


        /// <summary>
        /// 更新 拖拽的滑动区域 的 最新位置 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="dv2">滑动区域绑定的 OnDrag 在 每一帧 移动的距离 间隔向量</param>
        public static void _OnUpdatePosition(this ScrollCustomRange self, Vector2 dv2)
        {

            if (self.dataCount == 0) return;
            Vector2 pos = self.rtfGrid.anchoredPosition;
            //修正 参数 ，在竖滑动条 里  横向移动参数 置为0 . 横向滑动条同理
            dv2.x = 0;
            pos += dv2;

            self._SetPosition(pos);

            bool isOutView = self.CheckOutViewSpring();


            self.scrScrollMove.SetDragRebound(isOutView);

        }



        /// <summary>
        /// 检测超过屏幕的回弹
        /// </summary>
        public static bool CheckOutViewSpring(this ScrollCustomRange self, bool isPlayAnim = false)
        {

            Vector2 pos = self.rtfGrid.anchoredPosition;



            //表示第一个元素 他的位置已经重置过了
            if (self.IsItemRebuildPos(self.startItemIdx))
            {
                if (pos.y < self.v2GridPosLimit.x)
                {
                    if (isPlayAnim)
                        self._SetBackTween(DOTweenAnimCurve.Instance.DOAnchorPosY(self.rtfGrid, self.v2GridPosLimit.x, 0.3f, v =>
                        {
                            self.isCheckContinueItem = true;
                        }));

                    return true;
                }

            }



            //表示最后一个元素 他的位置已经重置过了
            if (self.IsItemRebuildPos(self.endItemIdx))
            {
                if (pos.y > self.v2GridPosLimit.y)
                {

                    if (isPlayAnim)
                        self._SetBackTween(DOTweenAnimCurve.Instance.DOAnchorPosY(self.rtfGrid, self.v2GridPosLimit.y, 0.3f, v =>
                        {
                            self.isCheckContinueItem = true;

                        }));
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// 惯性移动结束的 回调
        /// </summary>
        /// <param name="self"></param>
        public static void _OnAnimDragMoveEnd(this ScrollCustomRange self)
        {
            self.isAnimMoving = false;

            self.CheckOutViewSpring(true);





        }
        /// <summary>
        /// 惯性移动过程中
        /// </summary>
        /// <param name="self"></param>
        public static void _OnAnimDragMoving(this ScrollCustomRange self, Vector2 dv2)
        {


            self.isAnimMoving = true;
            self._OnUpdatePosition(dv2);





        }


        public static void OnDragStart(this ScrollCustomRange self)
        {
            self.isDrag = true;

            self.targetAnimIdx = -1;
            self._SetBackTween();
        }


        public static void OnDragMove(this ScrollCustomRange self, Vector2 dv2)
        {
            self._OnUpdatePosition(dv2);



        }


        public static void OnDragEnd(this ScrollCustomRange self)
        {
            self.isDrag = false;



        }












    }

}
