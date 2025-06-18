namespace ET.Client
{
    [Event(SceneType.LockStep)]
    public class LSSceneInitFinish_Finish: AEvent<Scene, LSSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, LSSceneInitFinish args)
        {
            Room room = clientScene.GetComponent<Room>();
            //创建玩家显示组件
            await room.AddComponent<LSUnitViewComponent>().InitAsync();
            //添加相机组件
            room.AddComponent<LSCameraComponent>();
            //如果是回放，就不添加操作的组建了
            if (!room.IsReplay)
            {
                room.AddComponent<LSOperaComponent>();
            }

            await UIHelper.Remove(clientScene, UIType.UILSLobby);
            await ETTask.CompletedTask;
        }
    }
}