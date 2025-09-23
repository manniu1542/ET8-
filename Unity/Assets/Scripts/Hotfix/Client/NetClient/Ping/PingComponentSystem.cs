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
            self.PingAsync().Coroutine();
        }

        [EntitySystem]
        private static void Destroy(this PingComponent self)
        {
            self.Ping = default;
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
                    if (LSConstValue.IsStartFrameSync)
                        await self.SendFrameSyncPing(session, fiber, instanceId);
                    else
                        await self.SendHeartPing(session, fiber, instanceId);
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

        ///<summary>发送普通心跳包</summary>
        public static async ETTask SendHeartPing(this PingComponent self, Session session, Fiber fiber, long instanceId)
        {
            await fiber.Root.GetComponent<TimerComponent>().WaitAsync(self.HeartIntervalTime);
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

        ///<summary>发送普通心跳包 并且 调节更新帧同步的更新频率</summary>
        public static async ETTask SendFrameSyncPing(this PingComponent self, Session session, Fiber fiber, long instanceId)
        {
            await fiber.Root.GetComponent<TimerComponent>().WaitAsync(self.FrameSyncIntervalTime);
            if (self.InstanceId != instanceId)
            {
                return;
            }

            //获取 客户端当前的时间戳
            long time1 = TimeInfo.Instance.ClientNow();
            C2Room_Ping c2GPing = C2Room_Ping.Create(true);
            //请求 获取 当前的 ping的帧 号。
            Room2C_Ping m2C_Ping = (Room2C_Ping)await session.Call(c2GPing);

            if (self.InstanceId != instanceId)
            {
                return;
            }

            //得到ping值
            self.Ping = TimeInfo.Instance.ClientNow() - time1;
            //模拟 的 服务器时间戳 ，（请求后到达服务器时 ，此时的时间戳是多少）  
            // 还是说 这个是 ping 消息，要保证 他的请求体足够小 不给他传参，让他的响应时间足够的快，
            m2C_Ping.Time = time1 + self.Ping / 2;
            //根据ping 值, 动态调整 更新 帧率（默认帧率 是 固定 50毫秒 更新一次 ） 
            self.ClientHandleExceptionNet(self.Ping, m2C_Ping);
        }

        ///<summary>根据延迟调整FixedUpdate更新频率   
        ///目的：让客户端跟服务端 帧数差距不大。如果客户端领先服务端 帧数过大：（ 1.客户端与服务器之间的差距会越来越大，游戏误差也会越来越大  2.则会造成数据回滚量大，游戏不流畅.） 
        ///解决方式就是：1.预期下次 的网络延迟跑的帧数(根据上次到达的延迟数/2,   接近预期下次的网络延迟数)，  2.得出 本次 执行 后 客户端与服务端之间的间隔数 ,3 调整执行时间， 让下次 客户端跑的帧数接近 服务端跑的帧数</summary>
        //比如： 上次 延迟了500毫秒 通过计算得出 延迟帧数  6次。 上次的 客户端 执行到 18帧，服务端执行到 5帧。   怎么样 让 客户端 延迟后 接近 （服务器 5帧 + 延迟 6帧 ）11帧 ， 让客户端刷新率降低即可
        public static void ClientHandleExceptionNet(this PingComponent self, long Ping, Room2C_Ping Data)
        {
            #region 预期（领先客户端领先服务器帧数）  ，     客户端发到服务器时，那个时候服务器跑了多少帧，也就是预期 延迟了多少帧 发到服务器。

            //延迟  （客户端发送到服务器 所需要的时间）                   
            long RTT = Ping % 2 == 0 ? Ping / 2 : Ping / 2 + 1;
            //延迟帧数   （延迟的时间/ 执行1帧所需要的间隔时间= 需要执行多少帧）
            int AheadOfFrame = (int)(RTT / LSConstValue.UpdateInterval) + 1;
            //重置 客户端超前 帧 数，  表示  服务器的确定帧到达 客户端之前，客户端又跑了多少帧  ，    预期下次 的网络延迟跑的帧数，
            self.TargetAheadOfFrame = AheadOfFrame > self.AheadOfFrameMax ? self.AheadOfFrameMax : AheadOfFrame;

            #endregion

            #region 实际 当前客户端 领先 服务端多少帧

            //模拟服务器发到客户端时，那个时候服务端所在的帧
            self.ServerCurrentFrame = Data.Frame + (int)((TimeInfo.Instance.ClientNow() - Data.Time) / LSConstValue.UpdateInterval);
            self.CurrentAheadOfFrame = self.CurrentFrame - self.ServerCurrentFrame;

            #endregion

            //客户端 总是领先于服务端 先跑,最理想的情况 是：  预期（领先客户端领先服务器帧数） = 实际 （当前客户端领先服务端的帧数）
            //预期间隔帧 > 实际间隔帧  ，更新 客户端帧 就需要 追帧。 客户端需要缩短 更新间隔，
            //预期间隔帧 < 实际间隔帧  ，更新 客户端帧 就需要 避开更新帧， 客户端需要 延长 更新间隔。

            //浮动fps = 默认1秒需要执行的帧数 + 预期 - 实际
            long floatingFps = LSConstValue.FrameCountPerSecond + self.TargetAheadOfFrame - self.CurrentAheadOfFrame;

            //最新的间隔时间刻度  =   1秒的时间刻度 * 之后调整花费的秒   ，         之后调整花费的秒  = 每帧多少秒 =  1 / 每秒多少帧
            long ticksSecond = TimeSpan.TicksPerSecond / floatingFps;
            Log.Error("--" + ticksSecond);
            //最大限制      （表示 35 毫秒 执行一次 fixedUpdate ，默认是  ConstantConfig.FrameLength  50毫秒）
            if (ticksSecond < 350000)
                ticksSecond = 350000;

            LSConstValue.DynamicUpdateInterval = (int)ticksSecond;
            //时间的更新频率
            // CodeLoader.Instance.FixedUpdateActuator.TargetElapsedTime = TimeSpan.FromTicks(ticksSecond);
        }
    }
}