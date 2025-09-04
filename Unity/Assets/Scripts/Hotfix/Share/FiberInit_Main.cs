namespace ET
{
    [Invoke((long)SceneType.Main)]
    public class FiberInit_Main : AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;

            //共享代码
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ObjectWait>();
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ProcessInnerSender>();

            await EventSystem.Instance.PublishAsync(root, new EntryEvent1());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent2());
            await EventSystem.Instance.PublishAsync(root, new EntryEvent3());

            //请求到 这个前程绑定这个场景
            var fiber = fiberInit.Fiber;

            var fiberId = await FiberManager.Instance.Create(SchedulerType.ThreadPool, fiber.Zone, SceneType.TestScene, "TestScene");
            ActorId ActorId = new(fiber.Process, fiberId);

            // // 发送消息给房间纤程，初始化
            var request = Main2TestScene_Test.Create();
            request.Msg = "Hello World";
            var response = await root.GetComponent<ProcessInnerSender>().Call(ActorId, request) as TestScene2Main_Test;
            Log.Error("MESSAGE"+response.ResMsg);
        }
    }
}