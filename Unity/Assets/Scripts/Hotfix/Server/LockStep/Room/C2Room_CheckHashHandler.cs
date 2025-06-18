namespace ET.Server
{
    [MessageHandler(SceneType.RoomRoot)]
    public class C2Room_CheckHashHandler: MessageHandler<Scene, C2Room_CheckHash>
    {
        protected override async ETTask Run(Scene root, C2Room_CheckHash message)
        {
            Room room = root.GetComponent<Room>();
            long hash = room.FrameBuffer.GetHash(message.Frame);
            //操作对比不通过
            if (message.Hash != hash)
            {
                byte[] bytes = room.FrameBuffer.Snapshot(message.Frame).ToArray();
                Room2C_CheckHashFail room2CCheckHashFail = Room2C_CheckHashFail.Create();
                room2CCheckHashFail.Frame = message.Frame;
                //服务器确定的哈希值 返回给客户端
                room2CCheckHashFail.LSWorldBytes = bytes;
                room.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(message.PlayerId, room2CCheckHashFail);
            }
            await ETTask.CompletedTask;
        }
    }
}