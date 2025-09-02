using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.RoomRoot)]
    public class RoomManager2Room_InitHandler : MessageHandler<Scene, RoomManager2Room_Init, Room2RoomManager_Init>
    {
        protected override async ETTask Run(Scene root, RoomManager2Room_Init request, Room2RoomManager_Init response)
        {
            //随机房间。添加房间组件。初始化房间的人员
            Room room = root.AddComponent<Room>();
            room.Name = "Server";
            room.AddComponent<RoomServerComponent, List<long>>(request.PlayerIds);
            
            //创建一个帧同步世界
            room.LSWorld = new LSWorld(SceneType.LockStepServer);
            await ETTask.CompletedTask;
        }
    }
}