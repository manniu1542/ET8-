namespace ET.Client
{
    [MessageHandler(SceneType.LockStep)]
    public class Room2C_CheckHashFailHandler: MessageHandler<Scene, Room2C_CheckHashFail>
    {
        protected override async ETTask Run(Scene root, Room2C_CheckHashFail message)
        {
            Log.Info("输出：服务器比对场景的那一帧数据不对，客户端的那一帧。跟服务端的那一帧对比");
            //比对输入哈希值失败的处理
            LSWorld serverWorld = MemoryPackHelper.Deserialize(typeof(LSWorld), message.LSWorldBytes, 0, message.LSWorldBytes.Length) as LSWorld;
            using (root.AddChild(serverWorld))
            {
                Log.Debug($"check hash fail, server: {message.Frame} {serverWorld.ToJson()}");
            }

            Room room = root.GetComponent<Room>();
            //获取客户端的那一帧的 LSWorld
            LSWorld clientWorld = room.GetLSWorld(SceneType.LockStepClient, message.Frame);
            using (root.AddChild(clientWorld))
            {
                Log.Debug($"check hash fail, client: {message.Frame} {clientWorld.ToJson()}");
            }
            
            
            //TODO: 后续可以添加 回滚逻辑  以服务端的逻辑该帧为基准。
            
            
            await ETTask.CompletedTask;
        }
    }
}