using System;
using System.Net;

namespace ET.Server
{
    [Event(SceneType.Main)]
    public class EntryEvent2_InitServer: AEvent<Scene, EntryEvent2>
    {
        protected override async ETTask Run(Scene root, EntryEvent2 args)
        {
            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                {
                    int process = root.Fiber.Process;
                    StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(process);
                    if (startProcessConfig.Port != 0)
                    {
                        //处理服务器同一个进程的消息，非同一个进程则 被NetOuter 消息转发。同一个进程则直接 进程内部消息互传 ，提升网络效率
                        await FiberManager.Instance.Create(SchedulerType.ThreadPool, ConstFiberId.NetInner, 0, SceneType.NetInner, "NetInner");
                    }

                    // 根据配置创建纤程
                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(process);
                    foreach (StartSceneConfig startConfig in processScenes)
                    {
                        await FiberManager.Instance.Create(SchedulerType.ThreadPool, startConfig.Id, startConfig.Zone, startConfig.Type, startConfig.Name);
                    }

                    break;
                }
                case AppType.Watcher:
                {
                    root.AddComponent<WatcherComponent>();
                    break;
                }
                case AppType.GameTool:
                {
                    break;
                }
            }

            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
        }
    }
}