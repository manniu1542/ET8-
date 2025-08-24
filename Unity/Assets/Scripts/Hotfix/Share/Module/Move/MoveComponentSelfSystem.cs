using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    /// <summary>
    /// MoveComponent 的扩展系统类，包含所有移动相关的逻辑
    /// </summary>
    [EntitySystemOf(typeof(MoveComponent))]
    [FriendOf(typeof(MoveComponent))]
    public static partial class MoveComponentSystem
    {
        /// <summary>
        /// 移动定时器，驱动每帧移动更新
        /// </summary>
        [Invoke(TimerInvokeType.MoveSelfTimer)]
        public class MoveTimer : ATimer<MoveComponent>
        {
            protected override void Run(MoveComponent self)
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
        private static void Awake(this ET.MoveComponent self)
        {
            self.ResetMoveState(false);
        }

        [EntitySystem]
        private static void Destroy(this ET.MoveComponent self)
        {
            self.MoveStop(false);
        }

        /// <summary>
        /// 立即移动到指定点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <param name="speed"></param>
        /// <param name="turnTime"></param>
        /// <returns></returns>
        public static async ETTask<bool> MoveToAsync(this MoveComponent self, List<float3> target, float speed, int turnTime = 100)
        {
            // 停止上一个移动
            self.MoveStop(false);

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

            self.curMoveTimer = self.Root().GetComponent<TimerComponent>().NewFrameTimer(TimerInvokeType.MoveSelfTimer, self);

            bool isNormalMoveFinish = await self.taskMoveState;
            if (isNormalMoveFinish)
            {
                //正常移动结束的事件发送
                EventSystem.Instance.PublishAsync(self.Scene(), new MoveStop() { Unit = self.GetParent<Unit>() }).Coroutine();
            }

            return isNormalMoveFinish;
        }

        public static void Stop(this MoveComponent self, bool isNormalMove)
        {
            self.MoveStop(isNormalMove);
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        /// <param name="self"></param>
        public static void MoveStop(this MoveComponent self, bool isNormalMove)
        {
            //在移动中,当前帧移动到的位置计算出来，在停止
            if (self.listPath.Count > 0)
            {
                self.UpdateMoveState(isNormalMove);
            }

            self.ResetMoveState(isNormalMove);
        }

        /// <summary>
        /// 重置移动属性状态
        /// </summary>
        /// <param name="self"></param>
        public static void ResetMoveState(this MoveComponent self, bool isNormalMove)
        {
            self.listPath.Clear();
            self.dtMoveLastTargetTime = 0;
            self.nextMoveIdx = 0;
            self.moveSpeed = 0;
            self.rotationNeedAnimTime = 0;
            self.moveNextTargetNeedIntervalTime = 0;
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

        public static void SetToNextTarget(this MoveComponent self)
        {
            //记录移动开始的时间戳
            if (self.nextMoveIdx == 0)
                self.dtMoveLastTargetTime = TimeInfo.Instance.ClientNow();

            //下个移动点位索引自增
            self.nextMoveIdx++;

            //到达上个目标点位的时间戳
            self.dtMoveLastTargetTime += self.moveNextTargetNeedIntervalTime;

            //位置
            var unit = self.GetParent<Unit>();
            self.MoveTargetStartPos = unit.Position;
            //记录移动到下个位置所需的时间。
            float3 dirDistance = self.CurMoveTargetPos - self.LastMoveTargetPos;
            float distance = math.length(dirDistance);
            self.moveNextTargetNeedIntervalTime = (long)(distance / self.moveSpeed * 1000);

            //是否需要旋转。如果点位距离过近不需要旋转。
            if (self.IsRotateInstantly)
            {
                float3 dir = math.normalize(dirDistance);
                //是否需要玩家倾斜。朝向目标方向
                if (self.isRotationLockY)
                    dir.y = 0;
                //获取到有旋转。避免0向量的朝向。
                if (math.abs(dir.x) > 0.01f || math.abs(dir.z) > 0.01f)
                {
                    self.MoveTargetDirRotation = quaternion.LookRotation(dir, new float3(0, 1, 0));
                    //立即旋转
                    unit.Rotation = self.MoveTargetDirRotation;
                }
            }
            else
            {
                //距离过近的话旋转屏蔽
                if (math.lengthsq(dirDistance) < 0.001f)
                {
                    return;
                }
                self.MoveTargetStartRotation = unit.Rotation;
                //是否需要玩家倾斜。朝向目标方向
                if (self.isRotationLockY)
                    dirDistance.y = 0;
                //获取到有旋转。避免0向量的朝向。
                if (math.abs(dirDistance.x) > 0.01f || math.abs(dirDistance.z) > 0.01f)
                {
                    self.MoveTargetDirRotation = quaternion.LookRotation(dirDistance, new float3(0, 1, 0));
                }
                
            }

        
        }

        /// <summary>
        ///   更新当前玩家所处的位置，旋转。 
        /// </summary>
        /// <param name="self"></param>
        public static void UpdateMoveState(this MoveComponent self, bool isNormalMove)
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
                    unit.Position = self.CurMoveTargetPos;
                    if (self.IsRotateInstantly)
                    {
                        unit.Rotation = self.MoveTargetDirRotation;
                    }
                }
                else //插值
                {
                    float amount = moveIntervalTime * 1f / self.moveNextTargetNeedIntervalTime;

                    if (amount > 0)
                    {
                        unit.Position = math.lerp(self.MoveTargetStartPos, self.CurMoveTargetPos, amount);
                    }

                    if (!self.IsRotateInstantly)
                    {
                        amount = moveIntervalTime * 1f / self.rotationNeedAnimTime;
                        //避免 math.slerp 持续的插值运算。math.slerp跟Quaternion.SlerpUnclamped 运算相同。
                        amount = amount > 1 ? 1 : amount;
                        unit.Rotation = math.slerp(self.MoveTargetStartRotation, self.MoveTargetDirRotation, amount);
                    }
                }

                moveIntervalTime -= self.moveNextTargetNeedIntervalTime;
                //不能移动了没有移动时间了
                if (moveIntervalTime < 0)
                    return;

                //已经到终点了
                if (self.nextMoveIdx >= self.listPath.Count - 1)
                {
                    //移动结束
                    self.ResetMoveState(true);
                    return;
                }

                //到达下个点位
                self.SetToNextTarget();
            }
        }

        public static void UnitRelinkReset(this MoveComponent self, ref UnitInfo uinfo)
        {
            if (self.listPath.Count > 0)
            {
                uinfo.MoveInfo = MoveInfo.Create();
                uinfo.MoveInfo.Points.Add(self.GetParent<Unit>().Position);

                for (int i = self.nextMoveIdx; i < self.listPath.Count; ++i)
                {
                    float3 pos = self.listPath[i];
                    uinfo.MoveInfo.Points.Add(pos);
                }
            }
        }
    }
}