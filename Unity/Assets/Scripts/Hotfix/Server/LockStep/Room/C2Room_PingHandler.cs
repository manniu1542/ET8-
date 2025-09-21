using System;

namespace ET
{
    [MessageHandler(SceneType.RoomRoot)]
    public class C2Room_PingHandler : MessageHandler<Scene, C2Room_Ping, Room2C_Ping>
    {
        protected override async ETTask Run(Scene scene, C2Room_Ping request, Room2C_Ping response)
        {
            using C2Room_Ping _ = request;

          
            Log.Error("Scene+" + scene.Name);
            response.Time = TimeInfo.Instance.ClientNow();
            response.Frame = 100;
            await ETTask.CompletedTask;
        }
    }
}