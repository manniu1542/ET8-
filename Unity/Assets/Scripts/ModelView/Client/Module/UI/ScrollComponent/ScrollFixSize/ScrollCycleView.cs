using ScrollViewUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    namespace Scroll
    {
        public enum DragType
        {
            Vertical,
            Horizontal

        }
    }

    public class ScrollCycleView : Entity, IAwake<Transform, Scroll.DragType>, IDestroy, IScrollView
    {
    

        public class ItemData
        {
            public GameObject go;
            public RectTransform rtf;
            public bool isActive;
        }
        #region UI 组件

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
        #endregion



        /// <summary>
        /// 支持超框拖动
        /// </summary>
        public bool isUseElastic = true;



        #region OnOpen参数

   
        /// <summary>
        /// item的大小
        /// </summary>
        public Vector2 cellSize;
        /// <summary>
        /// 中心点 如是 竖的滚动条 中心点设为 （0.5f,1）,横的（0，0.5f）
        /// </summary>
        public Vector2 itemPivot;

        /// <summary>
        /// item的宽度加上 宽度间隔
        /// </summary>
        public float itemWidth;
        /// <summary>
        /// item的高度加上 高度度间隔
        /// </summary>
        public float itemHeight;


        /// <summary>
        /// 滑动类型
        /// </summary>
        public Scroll.DragType dragType;

        /// <summary>
        /// 是否是竖的滑动条
        /// </summary>
        public bool IsVertical
        {
            get
            {
                return dragType == Scroll.DragType.Vertical;   
            }
        }
        /// <summary>
        /// 自动排布的行数或者列数 （竖的滑动列表 就表示  横着放多少个 换行 ，横的滑动列表 同理）
        /// </summary>
        public int fixedCount;
        /// <summary>
        ///  一页一（列/行）item的数量（如果 此时 是 竖 滑动条。 他就是 竖 这一列能放 几个 item数量【默认这个数量+1，表示总能显示全，且还能滚动时不漏空】。  横的滑动条同理）
        /// </summary>
        public int OnePageOneFixeditemCount;
        /// <summary>
        ///  一页一（列/行）里所有item总和所占的 （高/宽）的尺寸大小
        /// </summary>
        public Vector2 v2OnePageOneFixedTotalItemSize;


        /// <summary>
        /// 左右，上下间距
        /// </summary>
        public Vector2 spacing;

      
        /// <summary>
        /// item物体的对象池容器
        /// </summary>
        public List<GameObject> listItemGOPool;
        /// <summary>
        ///  字典 物体item 
        /// </summary>
        public Dictionary<ItemData, int> dicItemData;
        /// <summary>
        /// 当前所有 循环的item       （ 竖 的滑动 条 时，  存储为：行 列）
        /// </summary>
        public List<List<ItemData>> listItemData;

        #endregion

        #region SetData参数

        /// <summary>
        /// 显示所有的data 数量
        /// </summary>
        public int dataCount;

        public Action<int, GameObject> funUpdateItem;

        /// <summary>
        ///  总的可移动距离     = 竖 滑动条时，所有的行数* 一个item的高度   -   滑动区域控件的高度。
        /// </summary>
        public float totalMovableDistance;

        /// <summary>
        /// 滑动最大的 index 值 （最多滑动到那个 index）  = 竖 滑动条时：  值  （所有的行数 - 第一页的行数）
        /// </summary>
        public int maxOffset;

        /// <summary>
        /// 移动动画
        /// </summary>
        public DGTween tmpTween;

        /// <summary>
        /// 当前 item的index （区间在  0 ———— （实际所有行数- 第一页的行数））
        /// （因为可以 通过当前ui  rtfGrid 滑动块移动的位置 ，算出 当前所在的item 的index）
        /// </summary>
        public int curDataIndex;

        #endregion

        public ScrollWheelRollerComponent scrScrollWheelRoller;
        public ScrollMoveComponent scrScrollMove;
        public ScrollCycleBarComponent scrScrollBar;


    }
}