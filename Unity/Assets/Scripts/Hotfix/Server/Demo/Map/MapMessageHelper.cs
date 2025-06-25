using System.Collections.Generic;
using System.IO;

namespace ET.Server
{
    public static partial class MapMessageHelper
    {
        /// <summary>
        /// 通知客户端新增单位
        /// </summary>
        /// <param name="unit">接收通知的单位</param>
        /// <param name="sendUnit">新增的单位</param>
        public static void NoticeUnitAdd(Unit unit, Unit sendUnit)
        {
            // 创建M2C_CreateUnits消息实例
            M2C_CreateUnits createUnits = M2C_CreateUnits.Create();
            // 将新增单位的信息添加到消息中
            createUnits.Units.Add(UnitHelper.CreateUnitInfo(sendUnit));
            // 将消息发送给指定单位的客户端
            MapMessageHelper.SendToClient(unit, createUnits);
        }

        /// <summary>
        /// 通知客户端移除单位
        /// </summary>
        /// <param name="unit">接收通知的单位</param>
        /// <param name="sendUnit">被移除的单位</param>
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit)
        {
            // 创建M2C_RemoveUnits消息实例
            M2C_RemoveUnits removeUnits = M2C_RemoveUnits.Create();
            // 将被移除单位的ID添加到消息中
            removeUnits.Units.Add(sendUnit.Id);
            // 将消息发送给指定单位的客户端
            MapMessageHelper.SendToClient(unit, removeUnits);
        }

        /// <summary>
        /// 广播消息给所有能看到指定单位的玩家
        /// </summary>
        /// <param name="unit">消息来源单位</param>
        /// <param name="message">要广播的消息</param>
        public static void Broadcast(Unit unit, IMessage message)
        {
            // 设置消息的来源池标识为false
            (message as MessageObject).IsFromPool = false;
            // 获取所有能看到指定单位的玩家字典
            Dictionary<long, EntityRef<AOIEntity>> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化
            MessageLocationSenderOneType oneTypeMessageLocationType =
                    unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession);
            // 遍历字典，向所有能看到单位的玩家发送消息
            foreach (AOIEntity u in dict.Values)
            {
                oneTypeMessageLocationType.Send(u.Unit.Id, message);
            }
        }

        /// <summary>
        /// 向指定单位的客户端发送消息
        /// </summary>
        /// <param name="unit">接收消息的单位</param>
        /// <param name="message">要发送的消息</param>
        public static void SendToClient(Unit unit, IMessage message)
        {

            unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(unit.Id, message);
        }

        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        /// <param name="root">场景根实体</param>
        /// <param name="actorId">Actor的ID</param>
        /// <param name="message">要发送的消息</param>
        public static void Send(Scene root, ActorId actorId, IMessage message)
        {
            // 通过MessageSender组件向指定Actor发送消息
            root.GetComponent<MessageSender>().Send(actorId, message);
        }
    }
}