using ScrollViewUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
   
    public enum ScrollCustomUpdateType
    {
        /// <summary>
        /// 初始化Item（1.先判断是从上到下，还是从下往上生成，2.生成从上往下，中间的从上往下，从下往上 ）
        /// </summary>
        Init,

        /// <summary>
        /// 移动（1.触摸滑动，2.动画 回弹，3.动画惯性移动，4.动画 制定目标的移动）产生的 item更新
        /// </summary>
        Move,

        /// <summary>
        /// 静止 不更新
        /// </summary>
        None,

    }
    /// <summary>
    /// 自定义范围的无限滚动列表
    /// </summary>
    public class ScrollCustomRange : Entity, IAwake<Transform, Scroll.DragType>, IUpdate, IDestroy, IScrollView
    {


        /// <summary>
        /// 存储 item GO管理数据的
        /// </summary>
        public class ItemData
        {

            //下标
            public int index;
            //是否需要 重新绑定布局
            public bool isRebuildSize;
            //自身大小
            public float size;
            //是否需要 重新绑定布局
            public bool isRebuildPos;
            //初始位置
            public float posOrign;

        }

        /// <summary>
        /// 朝向目标动画移动速度
        /// </summary>
        public float animMoveTargetSpeed;

        /// <summary>
        /// 目标动画移动的值
        /// </summary>
        public int targetAnimIdx = -1;
        /// <summary>
        /// 动画的移动（惯性，移动至制定目标）
        /// </summary>
        public bool isAnimMoving;
        /// <summary>
        /// 是否拖拽
        /// </summary>
        public bool isDrag;
        /// <summary>
        /// 判断是否在Update更新item
        /// </summary>
        public bool IsUpdateItem
        {
            get
            {
                return targetAnimIdx != -1 || isAnimMoving || isDrag || isUpdateAwait || isCheckContinueItem;
            }

        }


        /// <summary>
        /// 更新等待 计算item的高度度
        /// </summary>
        public bool isUpdateAwait;

        /// <summary>
        /// 判断是否继续 刷新Item
        /// </summary>
        public bool isCheckContinueItem;



        //动态增加的 item物体
        public Stack<ScrollCustomRangeItemComponent> listGamePoolItemGO;

        //使用中的item 物体
        public LinkedList<ScrollCustomRangeItemComponent> listItemGO;//改成  LinkList非常合适，频繁的增删，


        //所有数据
        public Dictionary<int, ItemData> dicAllItemData;


        /// <summary>
        ///  当前存储的数据链 头尾
        /// </summary>
        public Vector2Int v2DicStartEndItemIdx;


        /// <summary>
        /// 合理范围的grid值  可滑动区域
        /// </summary>
        public Vector2 v2GridPosLimit;

        /// <summary>
        ///  item最小尺寸
        /// </summary>
        public Vector2 itemMinSizeDefault;

        /// <summary>
        /// 一页最多可能拥有的item数量
        /// </summary>
        public int onePageMaxItemCount;
        /// <summary>
        /// 移除的容错，放置item 在临界值。反复添加删除 ,是ItemMinSizeDefault的 20%+spacing
        /// </summary>
        public float itemRemoveSizeTolerant;

        /// <summary>
        /// 窗口大小
        /// </summary>
        public Vector2 v2ViewSize;

        /// <summary>
        /// 初始位置 
        /// </summary>
        public Vector2 initOrignPos;

        /// <summary>
        /// 所有data的数量
        /// </summary>
        public int dataCount;


        /// <summary>
        /// 起始点的idx   默认是0
        /// </summary>
        public int startItemIdx;

        public int endItemIdx
        {
            get
            {
                return dataCount - 1 + startItemIdx;
            }
        }

        public Action<int, GameObject> funUpdateItem;



        /// <summary>
        /// 滚动列表的根
        /// </summary>
        public RectTransform rtfRoot;
        /// <summary>
        /// ScrollCycleView滑动区域的 rtf 控件
        /// </summary>
        public RectTransform rtfScrollView;
        /// <summary>
        /// 实际滚动的tf;
        /// </summary>
        public RectTransform rtfGrid;





        public GameObject item;

        /// <summary>
        /// 支持超框拖动
        /// </summary>
        public bool isUseElastic = true;


        /// <summary>
        /// 滑动类型
        /// </summary>
        public Scroll.DragType dragType;



        /// <summary>
        /// 左右，上下间距
        /// </summary>
        public Vector2 spacing;


        /// <summary>
        /// 移动动画
        /// </summary>
        public DGTween tmpTween;


        public ScrollMoveComponent scrScrollMove;


        //正在准备初始化的item
        public bool isInitPageIteming;

        /// <summary>
        /// 更新类型
        /// </summary>
        public ScrollCustomUpdateType enumUpdateType;

        /// <summary>
        /// 计算Item初始化时使用
        /// </summary>
        public ScrollCustomRangeItemComponent scrItemCalculation;

    }
}