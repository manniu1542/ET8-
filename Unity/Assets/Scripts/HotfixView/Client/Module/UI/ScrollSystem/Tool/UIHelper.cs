using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ET
{
    #region UI接口。(临时的不让报错的)

    public interface IUIBase { }
    public interface IScrollView : IUIBase
    {
    
        //public virtual Transform uiTransform { get => null; set => value = null; }
    }
    public interface IScrollItem : IUIBase
    {

        //public virtual Transform uiTransform { get => null; set => value = null; }
    }
    public interface IUILogic : IUIBase
    {
        
    }
    #endregion
    
    
    
    public static partial class UIHelper
    {
        public static void RegisterEvent(this EventTrigger trigger, EventTriggerType eventType,
        UnityAction<BaseEventData> callback, bool isRemoveAllEvent = true)
        {
            EventTrigger.Entry entry = null;
            bool isHas = false;
            // 查找是否已经存在要注册的事件
            foreach (EventTrigger.Entry existingEntry in trigger.triggers)
            {
                if (existingEntry.eventID == eventType)
                {
                    entry = existingEntry;
                    isHas = true;
                    break;
                }
            }

            // 如果这个事件不存在，就创建新的实例
            if (entry == null)
            {
                entry = new EventTrigger.Entry();
                entry.eventID = eventType;
            }

            if (isRemoveAllEvent)
                entry.callback.RemoveAllListeners();
            // 添加触发回调并注册事件
            entry.callback.RemoveListener(callback);
            entry.callback.AddListener(callback);
            if (!isHas)
                trigger.triggers.Add(entry);
        }
        
        
                #region Scroll

        public static T GetScrollItems<K, T>(this K self, ref Dictionary<GameObject, T> dictionary, GameObject go)
            where K : Entity, IUIBase where T : Entity, IAwake<Transform>, IScrollItem
        {
            if (dictionary.TryGetValue(go, out T value))
            {
                return value;
            }
            else
            {
                T t = self.AddChild<T, Transform>(go.transform);
                dictionary.Add(go, t);
                return t;
            }
        }


        /// <summary>
        ///  更新Item ，
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="scroll"></param>
        /// <param name="dicBindGOScr"></param>
        /// <param name="count"></param>
        /// <param name="updateItem"></param>
        public static void ScrollUpdateItem<K, T>(this K self, ScrollCycleView scroll,
            Dictionary<GameObject, T> dicBindGOScr, int count, Action<int, T> updateItem) where K : Entity, IUIBase
            where T : Entity, IAwake<Transform>, IScrollItem
        {
            scroll.SetData(count, (idx, go) =>
            {
                go.gameObject.SetActive(true);
                T item = self.GetScrollItems(ref dicBindGOScr, go);
                updateItem.Invoke(idx, item);
            }).Coroutine();
        }

        /// <summary>
        /// 释放scroll 数据
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="scroll"></param>
        /// <param name="dicBindGOScr"></param>
        public static void DestoryScroll<K, T>(this K self, ScrollCycleView scroll,
            ref Dictionary<GameObject, T> dicBindGOScr) where K : Entity, IUILogic
            where T : Entity, IAwake<Transform>, IScrollItem
        {
            scroll?.Dispose();
            if (dicBindGOScr != null)
            {
                foreach (var item in dicBindGOScr)
                {
                    self.RemoveChild(item.Value.Id);
                }

                dicBindGOScr.Clear();
                // 勿置 null：ScrollUpdateItem 会复用同一 Dictionary，置 null 会导致 GetScrollItems NRE
            }
        }


        /// <summary>
        ///  更新Item ，
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="scroll"></param>
        /// <param name="dicBindGOScr"></param>
        /// <param name="count"></param>
        /// <param name="updateItem"></param>
        public static void ScrollUpdateItem<K, T>(this K self, ScrollCustomRange scroll,
            Dictionary<GameObject, T> dicBindGOScr, int count, Action<int, T> updateItem, int initUIIdx = 0)
            where K : Entity, IUIBase where T : Entity, IAwake<Transform>, IScrollItem
        {
            scroll.InitScroll(count, (idx, go) =>
            {
                T item = self.GetScrollItems(ref dicBindGOScr, go);
                updateItem.Invoke(idx, item);
            }, initUIIdx);
        }

        /// <summary>
        /// 释放scroll 数据
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="scroll"></param>
        /// <param name="dicBindGOScr"></param>
        public static void DestoryScroll<K, T>(this K self, ScrollCustomRange scroll,
            ref Dictionary<GameObject, T> dicBindGOScr) where K : Entity, IUILogic
            where T : Entity, IAwake<Transform>, IScrollItem
        {
            scroll?.Dispose();
            if (dicBindGOScr != null)
            {
                foreach (var item in dicBindGOScr)
                {
                    self.RemoveChild(item.Value.Id);
                }

                dicBindGOScr.Clear();
                dicBindGOScr = null;
            }
        }

        #endregion

        
        
        
    }
    
}
