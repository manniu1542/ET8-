using System;


namespace ET.Client
{
    public static partial class EnterMapHelper
    {
        public static async ETTask EnterMapAsync(Scene root)
        {
            try
            {
                //客户端发送进入地图的请求到网关 
                G2C_EnterMap g2CEnterMap = await root.GetComponent<ClientSenderComponent>().Call(C2G_EnterMap.Create()) as G2C_EnterMap;
                //请求完毕后，服务端会接着处理 =》网关服务端 从连接找到player根据player从数据库中找到unit=》网关服务端 会把当前的unit锁住，并给这个unit传送到一个新的Map
                //=》在Map服务器接收到传送的消息，会推送 客户端切换场景的的消息1，以及在这个场景创建unit的消息2=》客户端就在等待消息，在消息内处理切换场景创建客户端角色完成后
                //=》客户端调用Wait_SceneChangeFinish
                await root.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
                
                EventSystem.Instance.Publish(root, new EnterMapFinish());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
        
        public static async ETTask Match(Fiber fiber)
        {
            try
            {
                G2C_Match g2CEnterMap = await fiber.Root.GetComponent<ClientSenderComponent>().Call(C2G_Match.Create()) as G2C_Match;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
    }
}