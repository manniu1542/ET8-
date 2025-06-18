using System;
using System.Collections.Generic;

namespace ET.Server
{

    [FriendOf(typeof(MatchComponent))]
    public static partial class MatchComponentSystem
    {
        public static async ETTask Match(this MatchComponent self, long playerId)
        {
            //避免客户端重复点击匹配
            if (self.waitMatchPlayers.Contains(playerId))
            {
                return;
            }
            
            self.waitMatchPlayers.Add(playerId);
            //匹配人数多少人够开一个房间
            if (self.waitMatchPlayers.Count < LSConstValue.MatchCount)
            {
                return;
            }
            
            // 申请一个房间
            StartSceneConfig startSceneConfig = RandomGenerator.RandomArray(StartSceneConfigCategory.Instance.Maps);
            Match2Map_GetRoom match2MapGetRoom = Match2Map_GetRoom.Create();
            foreach (long id in self.waitMatchPlayers)
            {
                match2MapGetRoom.PlayerIds.Add(id);
            }
            
            self.waitMatchPlayers.Clear();

            Scene root = self.Root();
            // 发送消息给地图纤程，获取创建的房间服务器信息
            Map2Match_GetRoom map2MatchGetRoom = await root.GetComponent<MessageSender>().Call(
                startSceneConfig.ActorId, match2MapGetRoom) as Map2Match_GetRoom;

            Match2G_NotifyMatchSuccess match2GNotifyMatchSuccess = Match2G_NotifyMatchSuccess.Create();
            match2GNotifyMatchSuccess.ActorId = map2MatchGetRoom.ActorId;
            //根据定位中心发送者组件，给该房间的所有玩家发送匹配成功消息
            MessageLocationSenderComponent messageLocationSenderComponent = root.GetComponent<MessageLocationSenderComponent>();
            
            foreach (long id in match2MapGetRoom.PlayerIds) // 这里发送消息线程不会修改PlayerInfo，所以可以直接使用
            {
                messageLocationSenderComponent.Get(LocationType.Player).Send(id, match2GNotifyMatchSuccess);
                // 等待进入房间的确认消息，如果超时要通知所有玩家退出房间，重新匹配
            }
        }
    }

}