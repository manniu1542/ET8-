namespace ET.Client
{
    [Event(SceneType.LockStep)]
    public class LSSceneInitFinish_Finish: AEvent<Scene, LSSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, LSSceneInitFinish args)
        {
            Room room = clientScene.GetComponent<Room>();
            // 初始化所有玩家角色
            await room.AddComponent<LSUnitViewComponent>().InitAsync();
            // 添加帧同步相机组件
            room.AddComponent<LSCameraComponent>();

            if (!room.IsReplay)
            {
                // 添加帧同步操作组件
                room.AddComponent<LSOperaComponent>();
            }
        

            await UIHelper.Remove(clientScene, UIType.UILSLobby);
            await ETTask.CompletedTask;
        }
    }
}