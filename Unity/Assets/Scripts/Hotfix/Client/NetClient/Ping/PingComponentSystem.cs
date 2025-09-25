using System;

namespace ET.Client
{
    [EntitySystemOf(typeof(PingComponent))]
    [FriendOf(typeof(ET.Client.PingComponent))]
    public static partial class PingComponentSystem
    {
        [EntitySystem]
        private static void Awake(this PingComponent self)
        {
            self.SetUsing(true);
            self.PingAsync().Coroutine();
        }

        [EntitySystem]
        private static void Destroy(this PingComponent self)
        {
            self.SetUsing(false);
            self.Ping = default;
        }

        public static void SetUsing(this PingComponent self, bool isUsing)
        {
            self.IsUsing = isUsing;
        }

        private static async ETTask PingAsync(this PingComponent self)
        {
            Session session = self.GetParent<Session>();
            long instanceId = self.InstanceId;
            Fiber fiber = self.Fiber();

            while (true)
            {
                try
                {
                    await fiber.Root.GetComponent<TimerComponent>().WaitAsync(self.HeartIntervalTime);
                    if (!self.IsUsing) return;

                    if (self.InstanceId != instanceId)
                    {
                        return;
                    }

                    long time1 = TimeInfo.Instance.ClientNow();

                    // C2G_Ping不需要调用dispose，Call中会判断，如果用了对象池会自动回收
                    C2G_Ping c2GPing = C2G_Ping.Create(true);
                    // 这里response要用using才能回收到池，默认不回收
                    using G2C_Ping response = await session.Call(c2GPing) as G2C_Ping;

                    if (self.InstanceId != instanceId)
                    {
                        return;
                    }

                    long time2 = TimeInfo.Instance.ClientNow();
                    //RTT =Round Trip Time往返时间0延时  ，RTT/2 = 单程往返延时， 
                    self.Ping = time2 - time1;
                    //response.Time +RTT/2 = 理想中服务端此刻的时间。   time2 客户端当前时间 。  
                    //理想中服务端此刻的时间 - 客户端当前时间 = 服务端当前时间 - 客户端当前时间 = 服务端与客户端的时间差
                    TimeInfo.Instance.ServerMinusClientTime = response.Time + (time2 - time1) / 2 - time2;
                }
                catch (RpcException e)
                {
                    // session断开导致ping rpc报错，记录一下即可，不需要打成error
                    Log.Debug($"session disconnect, ping error: {self.Id} {e.Error}");
                    return;
                }
                catch (Exception e)
                {
                    Log.Debug($"ping error: \n{e}");
                }
            }
        }
    }
}