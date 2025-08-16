using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    /// <summary>
    /// MoveComponentSelf 的扩展系统类，包含所有移动相关的逻辑
    /// </summary>
    [EntitySystemOf(typeof(MoveComponentSelf))]
    [FriendOf(typeof(MoveComponentSelf))]
    public static partial class MoveComponentSelfSystem
    {
        /// <summary>
        /// 移动定时器，驱动每帧移动更新
        /// </summary>
        [Invoke(TimerInvokeType.MoveSelfTimer)]
        public class MoveTimer : ATimer<MoveComponentSelf>
        {
            protected override void Run(MoveComponentSelf self)
            {
                try
                {
                    // 每帧调用 MoveForward 更新位置
                    self.UpdateMoveState(true);
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }

        [EntitySystem]
        private static void Awake(this ET.MoveComponentSelf self)
        {
            self.ResetMoveState(false);
        }

        [EntitySystem]
        private static void Destroy(this ET.MoveComponentSelf self)
        {
            
            self.MoveStop();
        }

        // 维护  重置  移动 属性（目标点位，当前目标点位的idx，移动速度，旋转，）

        // 移动方法 使用 开启一个 update的 协程，用etask来 维护这个携程。 

        // update执行携程内容：当前时间玩家所初一的位置，旋转 设置，

        //方法： 立即开启移动，  立即停止移动，  更新当前玩家所处的位置，旋转。 

        /// <summary>
        /// 立即移动到指定点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <param name="speed"></param>
        /// <param name="turnTime"></param>
        /// <returns></returns>
        public static async ETTask<bool> MoveToAsync(this MoveComponentSelf self, List<float3> target, float speed, int turnTime = 100)
        {
            // 停止上一个移动
            self.MoveStop();

            #region 设置移动属性

            //人物移动相关
            foreach (var item in target)
            {
                self.listPath.Add(item);
            }

            self.moveSpeed = speed;
            self.rotationNeedAnimTime = turnTime;

            self.SetToNextTarget();

            #endregion

            //发送移动开始的事件
            EventSystem.Instance.PublishAsync(self.Scene(), new MoveStart() { Unit = self.GetParent<Unit>() }).Coroutine();

            //开启移动的携程 回调
            self.taskMoveState = ETTask<bool>.Create(true);

            self.Root().GetComponent<TimerComponent>().NewFrameTimer(TimerInvokeType.MoveSelfTimer, self);

            bool isNormalMoveFinish = await self.taskMoveState;
            if (isNormalMoveFinish)
            {
                //正常移动结束的事件发送
                EventSystem.Instance.PublishAsync(self.Scene(), new MoveStop() { Unit = self.GetParent<Unit>() }).Coroutine();
            }

            return isNormalMoveFinish;
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        /// <param name="self"></param>
        public static void MoveStop(this MoveComponentSelf self)
        {
            //在移动中,当前帧移动到的位置计算出来，在停止
            if (self.listPath.Count > 0)
            {
                self.UpdateMoveState(false);
            }

            self.ResetMoveState(false);
        }

        /// <summary>
        /// 重置移动属性状态
        /// </summary>
        /// <param name="self"></param>
        public static void ResetMoveState(this MoveComponentSelf self, bool isNormalMove)
        {
            self.listPath.Clear();
            self.dtMoveLastTargetTime = 0;
            self.nextMoveIdx = 0;
            self.moveSpeed = 0;
            self.rotationNeedAnimTime = 0;

   

            self.Root().GetComponent<TimerComponent>()?.Remove(ref self.curMoveTimer);

            
            if (self.taskMoveState != null)
            {
                //为什么先缓存 tcs再置空？  答：避免   tcs.SetResult(isNormalMove);调用Etask封装的闭包函数，再次调用ResetMoveState，形成递归调用了。
                //self.taskMoveState还没修改的情况，非常危险，需要先给他置空。形成闭包也没问题，不会再递归调用下去了
                
                var tcs = self.taskMoveState;
                self.taskMoveState = null;
                tcs.SetResult(isNormalMove);
            }
     
            
        }

        public static void SetToNextTarget(this MoveComponentSelf self)
        {
            //记录移动开始的时间戳
            if (self.nextMoveIdx == 1)
                self.dtMoveLastTargetTime = TimeInfo.Instance.ClientNow();
            
            //下个移动点位索引自增
            self.nextMoveIdx++;

            //到达上个目标点位的时间戳
            self.dtMoveLastTargetTime += self.moveNextTargetNeedIntervalTime;

            //记录移动到下个位置所需的时间。
            float3 dirDistance = self.listPath[self.nextMoveIdx] - self.CurMoveTarget;
            float distance = math.length(dirDistance);

            self.moveNextTargetNeedIntervalTime = (long)(distance / self.moveSpeed * 1000);
            
            
          
            
        }

        /// <summary>
        ///   更新当前玩家所处的位置，旋转。 
        /// </summary>
        /// <param name="self"></param>
        public static void UpdateMoveState(this MoveComponentSelf self, bool isNormalMove)
        {
            long nowTime = TimeInfo.Instance.ClientNow();
            
            //算出移动的间隔时间内 moveIntervalTime
            long moveIntervalTime = nowTime - self.dtMoveLastTargetTime;
            //玩家 当前所处于的点位，旋转
            while (true)
            {
                if (moveIntervalTime <= 0)
                    return;

                //玩家移动的代码。
                //可以移动到下个点位了
                var unit = self.GetParent<Unit>();
                if (moveIntervalTime >= self.moveNextTargetNeedIntervalTime)
                {
                    unit.Position = self.CurMoveTarget;
                    //TODO:旋转赋值
                    // unit.Rotation =

                }
                else //插值
                {
                    unit.Position = math.lerp( unit.Position ,self.CurMoveTarget, moveIntervalTime * 1f / self.moveNextTargetNeedIntervalTime);
                    //TODO:旋转赋值   unit.Rotation =
                    if (self.isRotationLockY)
                    {
                    
                    
                    }
                    else
                    {
                    
                    }
                }
                
                
                moveIntervalTime -= self.moveNextTargetNeedIntervalTime;
                //不能移动了没有移动时间了
                if(moveIntervalTime<0)
                    return;
                
                
                //已经到终点了
                if (self.nextMoveIdx >= self.listPath.Count)
                {
                    //移动结束
                    self.ResetMoveState(true);
                    return;
                }
                
                //到达下个点位
                self.SetToNextTarget();
            }

        }
    }
}