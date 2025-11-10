namespace ET.Client
{
    public static partial class LSSceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            root.RemoveComponent<Room>();

            Room room = root.AddComponentWithId<Room>(sceneInstanceId);
            room.Name = sceneName;
     
            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(root, new LSSceneChangeStart() { Room = room });
            //发送切换场景完成 （服务端收集到所有玩家都 加载完成发送 Room2C_EnterMap 给客户端）
            root.GetComponent<ClientSenderComponent>().Send(C2Room_ChangeSceneFinish.Create());
            //切换ping 
            SwitchPing(root, room);
            // 等待Room2C_EnterMap消息
            WaitType.Wait_Room2C_Start waitRoom2CStart = await root.GetComponent<ObjectWait>().Wait<WaitType.Wait_Room2C_Start>();

            //客户端构建帧同步世界  （初始化服务端 玩家数据 到客户端的 玩家数据。）
            room.LSWorld = new LSWorld(SceneType.LockStepClient);
            room.Init(waitRoom2CStart.Message.UnitInfo, waitRoom2CStart.Message.StartTime);
            //客户端的帧消息 处理
            room.AddComponent<LSClientUpdater>();
            // 这个事件中可以订阅取消loading  （把客户端的玩家数据 与 表现 绑定）
            EventSystem.Instance.Publish(root, new LSSceneInitFinish());
        }

        /// <summary>
        /// 切换ping,普通切换=》帧同步。或者 帧同步=》普通切换
        /// </summary>
        /// <param name="root"></param>
        /// <param name="replay"></param>
        public static void SwitchPing(Scene root, Room room)
        {
            LockStep2NetClient_SetPing setPing = LockStep2NetClient_SetPing.Create();
            setPing.IsNormalPing = false;
            root.GetComponent<ClientSenderComponent>().Send(setPing);

            room.AddComponent<LockStepPingComponent>();
        }

        // 场景切换协程
        public static async ETTask SceneChangeToReplay(Scene root, Replay replay)
        {
            root.RemoveComponent<Room>();

            Room room = root.AddComponent<Room>();
            room.Name = "Map1";
            room.IsReplay = true;
            room.Replay = replay;
            room.LSWorld = new LSWorld(SceneType.LockStepClient);
            room.Init(replay.UnitInfos, TimeInfo.Instance.ServerFrameTime());

            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(root, new LSSceneChangeStart() { Room = room });

            room.AddComponent<LSReplayUpdater>();
            // 这个事件中可以订阅取消loading
            EventSystem.Instance.Publish(root, new LSSceneInitFinish());
        }

        // 场景切换协程
        public static async ETTask SceneChangeToReconnect(Scene root, G2C_Reconnect message)
        {
            root.RemoveComponent<Room>();

            Room room = root.AddComponent<Room>();
            room.Name = "Map1";

            room.LSWorld = new LSWorld(SceneType.LockStepClient);
            room.Init(message.UnitInfos, message.StartTime, message.Frame);

            SwitchPing(root, room);
            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(root, new LSSceneChangeStart() { Room = room });

            room.AddComponent<LSClientUpdater>();
            // 这个事件中可以订阅取消loading
            EventSystem.Instance.Publish(root, new LSSceneInitFinish());
        }
    }
}