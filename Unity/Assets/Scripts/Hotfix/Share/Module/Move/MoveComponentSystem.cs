// using System;
// using System.Collections.Generic;
// using Unity.Mathematics;
//
// namespace ET
// {
//
//     /// <summary>
//     /// MoveComponent 的扩展系统类，包含所有移动相关的逻辑
//     /// </summary>
//     [EntitySystemOf(typeof(MoveComponent))] // 关联到 MoveComponent
//     [FriendOf(typeof(MoveComponent))]      // 声明为 MoveComponent 的友元
//     public static partial class MoveComponentSystem
//     {
//         /// <summary>
//         /// 移动定时器，驱动每帧移动更新
//         /// </summary>
//         [Invoke(TimerInvokeType.MoveTimer)]
//         public class MoveTimer: ATimer<MoveComponent>
//         {
//             protected override void Run(MoveComponent self)
//             {
//                 try
//                 {
//                     // 每帧调用 MoveForward 更新位置
//                     self.MoveForward(true);
//                 }
//                 catch (Exception e)
//                 {
//                     Log.Error($"move timer error: {self.Id}\n{e}");
//                 }
//             }
//         }
//     
//         /// <summary>
//         /// 销毁时清理移动状态
//         /// </summary>
//         [EntitySystem]
//         private static void Destroy(this MoveComponent self)
//         {
//             self.MoveFinish(false);
//         }
//         
//         /// <summary>
//         /// 初始化移动组件
//         /// </summary>
//         [EntitySystem]
//         private static void Awake(this MoveComponent self)
//         {
//             self.StartTime = 0;
//             self.StartPos = float3.zero;
//             self.NeedTime = 0;
//             self.MoveTimer = 0;
//             self.tcs = null;
//             self.Targets.Clear();
//             self.Speed = 0;
//             self.N = 0;
//             self.TurnTime = 0;
//         }
//         
//         /// <summary>
//         /// 检查是否已到达所有目标点
//         /// </summary>
//         public static bool IsArrived(this MoveComponent self)
//         {
//             return self.Targets.Count == 0;
//         }
//
//         /// <summary>
//         /// 动态改变移动速度
//         /// </summary>
//         /// <returns>是否成功改变速度</returns>
//         public static bool ChangeSpeed(this MoveComponent self, float speed)
//         {
//             if (self.IsArrived())
//             {
//                 return false;
//             }
//
//             if (speed < 0.0001)
//             {
//                 return false;
//             }
//             
//             Unit unit = self.GetParent<Unit>();
//
//             // 使用对象池创建临时路径
//             using ListComponent<float3> path = ListComponent<float3>.Create();
//             
//             // 先完成当前移动段
//             self.MoveForward(false);
//                 
//             // 重新构建路径：当前位置 + 剩余目标点
//             path.Add(unit.Position);
//             for (int i = self.N; i < self.Targets.Count; ++i)
//             {
//                 path.Add(self.Targets[i]);
//             }
//             
//             // 以新速度重新开始移动
//             self.MoveToAsync(path, speed).Coroutine();
//             return true;
//         }
//
//         /// <summary>
//         /// 异步移动到目标路径
//         /// </summary>
//         /// <param name="target">目标路径点列表</param>
//         /// <param name="speed">移动速度</param>
//         /// <param name="turnTime">转向时间(ms)</param>
//         /// <returns>是否成功完成移动</returns>
//         public static async ETTask<bool> MoveToAsync(this MoveComponent self, List<float3> target, float speed, int turnTime = 100)
//         {
//             // 停止当前移动
//             self.Stop(false);
//
//             // 设置新路径
//             foreach (float3 v in target)
//             {
//                 self.Targets.Add(v);
//             }
//
//             // 配置移动参数
//             self.IsTurnHorizontal = true;
//             self.TurnTime = turnTime;
//             self.Speed = speed;
//             self.tcs = ETTask<bool>.Create(true);
//        
//             // 发布移动开始事件 
//             EventSystem.Instance.PublishAsync(self.Scene(), new MoveStart() {Unit = self.GetParent<Unit>()}).Coroutine();
//             
//             // 开始移动
//             self.StartMove();
//             
//             // 等待移动完成  如果moveRet 是true 表示正常移动完成
//             bool moveRet = await self.tcs;
//
//             // 如果正常完成，发布移动停止事件
//             if (moveRet)
//             {
//                 EventSystem.Instance.Publish(self.Scene(), new MoveStop() {Unit = self.GetParent<Unit>()});
//             }
//             return moveRet;
//         }
//
//         /// <summary>
//         /// 移动核心逻辑， 当前的时间玩家应该移动到的位置以及状态  计算插值位置和旋转 ,
//         /// </summary>
//         /// <param name="ret">是否正常完成移动</param>
//         private static void MoveForward(this MoveComponent self, bool ret)
//         {
//             Unit unit = self.GetParent<Unit>();
//             
//             // 计算从开始移动到现在的时间
//             long timeNow = TimeInfo.Instance.ClientNow();
//             //到下个点位的所需要的移动时间
//             long moveTime = timeNow - self.StartTime;
//    
//             while (true)
//             {
//                 //还没开始移动
//                 if (moveTime <= 0)
//                 {
//                     return;
//                 }
//                 
//                 // 如果已经超过本段移动时间，直接到达目标点
//                 if (moveTime >= self.NeedTime)
//                 {
//                     unit.Position = self.NextTarget;
//                     if (self.TurnTime > 0)
//                     {
//                         unit.Rotation = self.To;
//                     }
//                 }
//                 else // 否则计算插值  （位置更新。服务端来更新新的aoi，客户端来驱动模型移动， 旋转，客户端驱动模型旋转）
//                 {
//                     // 位置插值
//                     float amount = moveTime * 1f / self.NeedTime;
//                     Log.Error($"self.StartPos：{self.StartPos}，self.NextTarget：{self.NextTarget}，amount:{amount},moveIntervalTime:{moveTime},moveNextTargetNeedIntervalTime:{self.NeedTime}");
//                     if (amount > 0)
//                     {
//                         float3 newPos = math.lerp(self.StartPos, self.NextTarget, amount);
//                         unit.Position = newPos;
//                         Log.Error($"newPos:{newPos}");
//                     }
//                     
//                     // 旋转插值
//                     if (self.TurnTime > 0)
//                     {
//                         amount = moveTime * 1f / self.TurnTime;
//                         if (amount > 1)
//                         {
//                             amount = 1f;
//                         }
//                         quaternion q = math.slerp(self.From, self.To, amount);
//                         unit.Rotation = q;
//                     }
//                 }
//
//                 // 扣除已计算的时间
//                 moveTime -= self.NeedTime;
//
//                 // 如果还有剩余时间，还没有移动这一段 移动完。继续移动
//                 if (moveTime < 0)
//                 {
//                     return;
//                 }
//                 
//                 // 检查是否是最后一个点
//                 if (self.N >= self.Targets.Count - 1)
//                 {
//                     unit.Position = self.NextTarget;
//                     unit.Rotation = self.To;
//
//                     self.MoveFinish(ret);
//                     return;
//                 }
//   
//                 // 切换到下一个目标点
//                 self.SetNextTarget();
//             }
//         }
//
//         /// <summary>
//         /// 开始移动，初始化计时器和参数
//         /// </summary>
//         private static void StartMove(this MoveComponent self)
//         {
//             self.BeginTime = TimeInfo.Instance.ClientNow();
//             self.StartTime = self.BeginTime;
//             self.SetNextTarget();
//
//             // 注册帧定时器
//             self.MoveTimer = self.Root().GetComponent<TimerComponent>().NewFrameTimer(TimerInvokeType.MoveTimer, self);
//         }
//
//         /// <summary>
//         /// 设置下一个目标点并计算相关参数  ,设置 当前点位，下个点位，到下个点位需要的旋转，朝向。移动到下个点位所需要的时间
//         /// </summary>
//         private static void SetNextTarget(this MoveComponent self)
//         {
//             Unit unit = self.GetParent<Unit>();
//
//             ++self.N; // 增加路径点索引
//
//             // 计算方向向量和距离
//             float3 v = self.GetFaceV();
//             float distance = math.length(v);
//             
//             // 更新起始位置为当前位置
//             self.StartPos = unit.Position;
//
//             // 累加开始时间
//             self.StartTime += self.NeedTime;
//             
//             // 计算到下一个点需要的时间
//             self.NeedTime = (long)(distance / self.Speed * 1000);
//             
//             // 处理转向逻辑
//             if (self.TurnTime > 0)
//             {
//                 float3 faceV = self.GetFaceV();
//                 if (math.lengthsq(faceV) < 0.0001f)
//                 {
//                     return;
//                 }
//                 self.From = unit.Rotation;
//                 
//                 if (self.IsTurnHorizontal)
//                 {
//                     faceV.y = 0;
//                 }
//
//                 if (Math.Abs(faceV.x) > 0.01 || Math.Abs(faceV.z) > 0.01)
//                 {
//                     self.To = quaternion.LookRotation(faceV, math.up());
//                 }
//             }
//             else if (self.TurnTime == 0) // 立即转向
//             {
//                 float3 faceV = self.GetFaceV();
//                 if (self.IsTurnHorizontal)
//                 {
//                     faceV.y = 0;
//                 }
//
//                 if (Math.Abs(faceV.x) > 0.01 || Math.Abs(faceV.z) > 0.01)
//                 {
//                     self.To = quaternion.LookRotation(faceV, math.up());
//                     unit.Rotation = self.To;
//                 }
//             }
//         }
//
//         /// <summary>
//         /// 获取当前面向方向向量
//         /// </summary>
//         private static float3 GetFaceV(this MoveComponent self)
//         {
//             return self.NextTarget - self.PreTarget;
//         }
//
//         /// <summary>
//         /// 瞬间移动到目标点
//         /// </summary>
//         public static bool FlashTo(this MoveComponent self, float3 target)
//         {
//             Unit unit = self.GetParent<Unit>();
//             unit.Position = target;
//             return true;
//         }
//
//         /// <summary>
//         /// 停止移动
//         /// </summary>
//         /// <param name="ret">是否正常完成</param>
//         public static void Stop(this MoveComponent self, bool ret)
//         {
//             if (self.Targets.Count > 0)
//             {
//                 self.MoveForward(ret);
//             }
//
//             self.MoveFinish(ret);
//         }
//
//         /// <summary>
//         /// 移动结束，清理状态
//         /// </summary>
//         private static void MoveFinish(this MoveComponent self, bool ret)
//         {
//             if (self.StartTime == 0)
//             {
//                 return;
//             }
//             
//             // 重置所有状态
//             self.StartTime = 0;
//             self.StartPos = float3.zero;
//             self.BeginTime = 0;
//             self.NeedTime = 0;
//             self.Targets.Clear();
//             self.Speed = 0;
//             self.N = 0;
//             self.TurnTime = 0;
//             self.IsTurnHorizontal = false;
//             
//             // 移除定时器
//             self.Root().GetComponent<TimerComponent>()?.Remove(ref self.MoveTimer);
//
//             // 完成异步任务
//             if (self.tcs != null)
//             {
//                 var tcs = self.tcs;
//                 self.tcs = null;
//                 tcs.SetResult(ret);
//             }
//         }
//         
//         public static void UnitRelinkReset(this MoveComponent self, ref UnitInfo unitInfo)
//         {
//             if (!self.IsArrived())
//             {
//                 unitInfo.MoveInfo = MoveInfo.Create();
//                 unitInfo.MoveInfo.Points.Add(self.GetParent<Unit>().Position);
//                 for (int i = self.N; i < self.Targets.Count; ++i)
//                 {
//                     float3 pos = self.Targets[i];
//                     unitInfo.MoveInfo.Points.Add(pos);
//                 }
//             }
//          
//         }
//     }
// }