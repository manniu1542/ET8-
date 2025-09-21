using System;
using System.Collections.Generic;
using System.IO;
using BEPUphysics;
using TrueSync;

namespace ET.Client
{
    [Event(SceneType.Main)]
    [FriendOfAttribute(typeof(ET.Client.UIComponent))]
    public class EntryEvent3_InitClient : AEvent<Scene, EntryEvent3>
    {
        protected override async ETTask Run(Scene root, EntryEvent3 args)
        {
            GlobalComponent globalComponent = root.AddComponent<GlobalComponent>();
            root.AddComponent<UIGlobalComponent>();
            root.AddComponent<UIComponent>();
            root.AddComponent<ResourcesLoaderComponent>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();


            root.SceneType = SceneType.LockStep;

            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
            ShowViewSingleton();

            Space space = new Space();
            Log.Error("MESSAGE"+space.ToString());
        }

        /// <summary>
        /// 显示单例类
        /// </summary>
        public static void ShowViewSingleton()
        {
            var viewSingleton = new UnityEngine.GameObject("World.Singletons");
            viewSingleton.transform.SetParent(UnityEngine.GameObject.Find("Global/World").transform);
            viewSingleton.transform.SetSiblingIndex(0);
            foreach (var singleton in World.Instance.GetSingletons)
            {
                var go = new UnityEngine.GameObject(singleton.Key.ToString());
                go.transform.SetParent(viewSingleton.transform);
                //TODO: 写一个展示单例类的脚本，便于看单例类的属性情况。 go.AddComponent<ComponentView>().Component = 
            }
        }
    }
}