using System;
using UnityEngine;

namespace ET
{
    [FriendOf(typeof(ScrollCustomRange))]
    [FriendOf(typeof(ScrollCustomRangeItemComponent))]
    [FriendOf(typeof(ScrollMoveComponent))]
    public static class ScrollCustomRangeGOPoolSystem
    {

        public static void ResetPoolGO(this ScrollCustomRange self)
        {
            int maxRefreshCount = self.onePageMaxItemCount + 2;//+2必须的预留两个上面一个，下面一个，防止不足对象池中不够
            int count = maxRefreshCount - self.listGamePoolItemGO.Count;
      
            for (int i = 0; i < count; i++)
            {
                self.AddNewItem();
            }


        }
        public static void AddNewItem(this ScrollCustomRange self)
        {
            GameObject childItem = GameObject.Instantiate(self.item, self.rtfGrid.transform);
            var scrollViewItemComponent = self.AddChild<ScrollCustomRangeItemComponent, Transform>(childItem.transform);
            self.PushGOPool(scrollViewItemComponent);
        }
        public static void PushGOPool(this ScrollCustomRange self, ScrollCustomRangeItemComponent scritem)
        {
            scritem.SetIsUsing(false);

            self.listGamePoolItemGO.Push(scritem);
        }
        public static ScrollCustomRangeItemComponent PopGOPool(this ScrollCustomRange self)
        {
            if (self.listGamePoolItemGO.Count == 0)
            {
                self.AddNewItem();
                Log.Error($"检查UI上最小尺寸的Item,是否不是最小的尺寸导致的Bug，对象池内不可能不足！临时加入新的item");
            }
            ScrollCustomRangeItemComponent scrItem = self.listGamePoolItemGO.Pop();
            scrItem.SetIsUsing(true);
            return scrItem;
        }

    }
}
