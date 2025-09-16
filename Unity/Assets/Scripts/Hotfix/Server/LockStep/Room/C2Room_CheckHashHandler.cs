namespace ET.Server
{
    /// <summary>
    /// 校验客户端与服务端之间的 这一帧的输入 哈希值比对
    /// </summary>
    [MessageHandler(SceneType.RoomRoot)]
    public class C2Room_CheckHashHandler: MessageHandler<Scene, C2Room_CheckHash>
    {
        protected override async ETTask Run(Scene root, C2Room_CheckHash message)
        {
            Room room = root.GetComponent<Room>();
            long hash = room.FrameBuffer.GetHash(message.Frame);
            //比对失败。服务端把这一帧的 服务端数据发送给客户端
            if (message.Hash != hash)
            {
                byte[] bytes = room.FrameBuffer.Snapshot(message.Frame).ToArray();
                Room2C_CheckHashFail room2CCheckHashFail = Room2C_CheckHashFail.Create();
                room2CCheckHashFail.Frame = message.Frame;
                room2CCheckHashFail.LSWorldBytes = bytes;
                room.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(message.PlayerId, room2CCheckHashFail);
            }
            await ETTask.CompletedTask;
        }
    }
}