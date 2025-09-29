using System;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LoginGateHandler : MessageSessionHandler<C2G_LoginGate, G2C_LoginGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response)
        {
            Scene root = session.Root();
            string account = root.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            if (account == null)
            {
                response.Error = ErrorCore.ERR_ConnectGateKeyError;
                response.Message = "Gate key验证失败!";
                return;
            }

            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
            //这里应该有个数据库查找玩家数据，并根据数据来生成玩家
            Player player = playerComponent.GetByAccount(account);

            if (player == null)
            {
                player = playerComponent.AddChild<Player, string>(account);
                playerComponent.Add(player);
                PlayerSessionComponent playerSessionComponent = player.AddComponent<PlayerSessionComponent>();
                playerSessionComponent.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.GateSession);
                await playerSessionComponent.AddLocation(LocationType.GateSession);

                player.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
                await player.AddLocation(LocationType.Player);

                session.AddComponent<SessionPlayerComponent>().Player = player;
                playerSessionComponent.Session = session;
            }
            else
            {
                // 判断是否在战斗 ,通过获取玩家的房间组件。
                PlayerRoomComponent playerRoomComponent = player.GetComponent<PlayerRoomComponent>();
                if (playerRoomComponent.RoomActorId != default)
                {
                    CheckRoom(player, session).Coroutine();
                }
                else
                {
                    PlayerSessionComponent playerSessionComponent = player.GetComponent<PlayerSessionComponent>();
                    playerSessionComponent.Session = session;
                }
            }

            response.PlayerId = player.Id;
            await ETTask.CompletedTask;
        }

        private static async ETTask CheckRoom(Player player, Session session)
        {
            //等待房间的那一帧结束。不在房间逻辑还在执行中进入。怕出现有些消息 在还没有初始化完成就进入了房间。客户端收到该消息也乱掉。
            Fiber fiber = player.Fiber();
            await fiber.WaitFrameFinish();

            //网关=>玩家房间服务器。发送 重连消息
            G2Room_Reconnect g2RoomReconnect = G2Room_Reconnect.Create();
            g2RoomReconnect.PlayerId = player.Id;
            using Room2G_Reconnect room2GateReconnect = await fiber.Root.GetComponent<MessageSender>().Call(
                player.GetComponent<PlayerRoomComponent>().RoomActorId,
                g2RoomReconnect) as Room2G_Reconnect;
            //把房间服务器获取的消息转发给 客户端
            G2C_Reconnect g2CReconnect = G2C_Reconnect.Create();
            g2CReconnect.StartTime = room2GateReconnect.StartTime;
            g2CReconnect.Frame = room2GateReconnect.Frame;
            g2CReconnect.UnitInfos.AddRange(room2GateReconnect.UnitInfos);
            session.Send(g2CReconnect);

            session.AddComponent<SessionPlayerComponent>().Player = player;
            player.GetComponent<PlayerSessionComponent>().Session = session;
        }
    }
}