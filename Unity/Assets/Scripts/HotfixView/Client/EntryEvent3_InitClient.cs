using System;
using System.Collections.Generic;
using System.IO;

namespace ET.Client
{
    [Event(SceneType.Main)]
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

            // 根据配置修改掉Main Fiber的SceneType
            SceneType sceneType = EnumHelper.FromString<SceneType>(globalComponent.GlobalConfig.AppType.ToString());
            root.SceneType = sceneType;

            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
            ShowViewSingleton();
        }

        /// <summary>
        /// 显示单例类
        /// </summary>
        public static void ShowViewSingleton()
        {
            var viewSingleton = new UnityEngine.GameObject("World.Singletons");
            viewSingleton.transform.SetParent(UnityEngine.GameObject.Find("Global/Wolrd").transform);
            foreach (var singleton in World.Instance.GetSingletons)
            {
                new UnityEngine.GameObject(singleton.ToString()).transform.SetParent(viewSingleton.transform);
            }
        }
    }
}