namespace ET.Client
{
    /// <summary>
    /// 客户端收到服务器下发的“调整刷新率”消息时的处理逻辑
    /// （服务端通过 diffTime 来校准客户端帧同步节奏，避免时间漂移）
    /// </summary>
    [MessageHandler(SceneType.LockStep)]
    public class Room2C_AdjustUpdateTimeHandler: MessageHandler<Scene, Room2C_AdjustUpdateTime>
    {
        protected override async ETTask Run(Scene root, Room2C_AdjustUpdateTime message)
        {
            // 获取当前房间
            Room room = root.GetComponent<Room>();

            // 根据服务器发来的时间差计算新的帧间隔
            // 公式： DiffTime - 原来的间隔  表示 服务器 跟客户端实际间隔时间。  理想状态下。服务端比客户端早跑1帧，客户端发消息到服务端。这个间隔时间内-那一帧。 真正的时间差 。再算当前的追帧的更新间隔
            // 作用：把服务器的时间偏差换算成客户端需要调整的 Update 间隔
            int newInterval = (1000 + (message.DiffTime - LSConstValue.UpdateInterval)) * LSConstValue.UpdateInterval / 1000;

            // 最小限制：40ms（即最大帧率约 25 FPS）
            if (newInterval < 40)
            {
                newInterval = 40;
            }

            // 最大限制：66ms（即最小帧率约 15 FPS）
            if (newInterval > 66)
            {
                newInterval = 66;
            }
            
            // 修改 FixedTimeCounter 的帧间隔
            // 参数：新的间隔 + 当前预测帧（保证调整后时间对齐）
            room.FixedTimeCounter.ChangeInterval(newInterval, room.PredictionFrame);

       
            await ETTask.CompletedTask;
        }
    }
}