using System;
using System.Collections.Generic;

namespace ET.Server
{  
	//地图服务器里 创建基于该地图服务器的房间服务器
	[MessageHandler(SceneType.Map)]
	public class Match2Map_GetRoomHandler : MessageHandler<Scene, Match2Map_GetRoom, Map2Match_GetRoom>
	{
		protected override async ETTask Run(Scene root, Match2Map_GetRoom request, Map2Match_GetRoom response)
		{
			//RoomManagerComponent roomManagerComponent = root.GetComponent<RoomManagerComponent>();
			
			Fiber fiber = root.Fiber();
			//创建房间纤程，该房间纤程的名称RoomRoot 可以换一下 依据房间编号
			int fiberId = await FiberManager.Instance.Create(SchedulerType.ThreadPool, fiber.Zone, SceneType.RoomRoot, "RoomRoot");
			ActorId roomRootActorId = new(fiber.Process, fiberId);

			// 发送消息给房间纤程，初始化
			RoomManager2Room_Init roomManager2RoomInit = RoomManager2Room_Init.Create();
			roomManager2RoomInit.PlayerIds.AddRange(request.PlayerIds);
			await root.GetComponent<MessageSender>().Call(roomRootActorId, roomManager2RoomInit);
			
			response.ActorId = roomRootActorId;
			await ETTask.CompletedTask;
		}
	}
}