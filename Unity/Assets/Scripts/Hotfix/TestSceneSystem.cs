namespace ET
{
    public struct TestSceneEvent
    {
        public string msg;
    }

    [Event(SceneType.TestScene)]
    public class TestSceneEvent_InitClient : AEvent<Scene, TestSceneEvent>
    {
        protected override async ETTask Run(Scene root, TestSceneEvent args)
        {
            root.GetComponent<TestScene>().ALog(args.msg);
            Log.Error("MESSAGE" + args.msg);
            await ETTask.CompletedTask;
        }
    }

    [Invoke((long)SceneType.TestScene)]
    public class FiberInit_TestScene : AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            
            ActorId ActorId = new( fiberInit.Fiber.Process,  fiberInit.Fiber.Id);
            Log.Error("ActorID TestScene:" +  ActorId);
            //共享代码
            
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ObjectWait>();
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<TestScene>();
            await ETTask.CompletedTask;
            
        }
    }

    [MessageHandler(SceneType.TestScene)]
    public class Main2TestScene_TestHandler : MessageHandler<Scene, Main2TestScene_Test, TestScene2Main_Test>
    {
        protected override async ETTask Run(Scene root, Main2TestScene_Test request, TestScene2Main_Test response)
        {
            await EventSystem.Instance.PublishAsync(root, new TestSceneEvent { msg = "TestScene2Main_Test" });
            response.ResMsg = "TestScene2Main_Test222";
            await ETTask.CompletedTask;
        }
    }

    [EntitySystemOf(typeof(TestScene))]
    [FriendOf(typeof(TestScene))]
    public static partial class TestSceneSystem
    {
        [EntitySystem]
        private static void Awake(this TestScene self)
        {
        }

        public static void ALog(this TestScene self, string msg)
        {
            Log.Error(msg);
        }
    }
}