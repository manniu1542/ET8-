using System;
using ET.Server;

namespace ET
{
    [MessageHandler(SceneType.RoomRoot)]
    public class C2Room_PingHandler : MessageHandler<Scene, C2Room_Ping, Room2C_Ping>
    {
        protected override async ETTask Run(Scene root, C2Room_Ping request, Room2C_Ping response)
        {
           
            Room room = root.GetComponent<Room>();
            response.Time = TimeInfo.Instance.ClientNow();
            response.Frame =  room.AuthorityFrame;
            
            await ETTask.CompletedTask;
        }
    }
}