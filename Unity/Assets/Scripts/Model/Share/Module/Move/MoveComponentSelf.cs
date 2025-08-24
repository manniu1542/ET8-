using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    /// <summary>
    /// Unit 的移动组件，用于处理路径移动、转向和异步控制
    /// 注意：这是一个前后端共用的组件
    /// </summary>
    [ComponentOf(typeof(Unit))] // 表示这个组件属于 Unit 实体
    public class MoveComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 正常移动完成返回 true，打断的移动返回false
        /// </summary>
        public ETTask<bool> taskMoveState;

        /// <summary>
        /// 记录当前移动的携程timerid
        /// </summary>
        public long curMoveTimer;

        /// <summary>
        /// 移动速度
        /// </summary>
        public float moveSpeed;

        /// <summary>
        /// 移动的路径点位存储
        /// </summary>
        public List<float3> listPath = new List<float3>();

        /// <summary>
        /// 当前路径点位索引
        /// </summary>
        public int nextMoveIdx;

        /// <summary>
        ///上一个移动的目标点位
        /// </summary>
        public float3 LastMoveTargetPos
        {
            get
            {
                return listPath[nextMoveIdx-1];
            }
        }

        /// <summary>
        /// 当前移动的目标点位
        /// </summary>
        public float3 CurMoveTargetPos
        {
            get
            {
                return listPath[nextMoveIdx];
            }
        }

        /// <summary>
        /// 移动到下个目标点 ，开始的点位。方便插值运算
        /// </summary>
        public float3 MoveTargetStartPos;

        /// <summary>
        /// 移动到下个目标点 ，开始的旋转。方便插值运算
        /// </summary>
        public quaternion MoveTargetStartRotation;

        /// <summary>
        /// 旋转到移动方向的朝向。
        /// </summary>
        public quaternion MoveTargetDirRotation;

        /// <summary>
        /// 是否旋转锁定y轴
        /// </summary>
        public bool isRotationLockY = true;

        /// <summary>
        /// 玩家旋转所需要的时间
        /// </summary>
        public int rotationNeedAnimTime;

        /// <summary>
        /// 是否立即旋转
        /// </summary>
        public bool IsRotateInstantly
        {
            get
            {
                return rotationNeedAnimTime <= 0;
            }
        }

        /// <summary>
        /// 记录移动到上一个目标点的时间戳
        /// </summary>
        public long dtMoveLastTargetTime;

        /// <summary>
        /// 移动到下个目标的所需时间 = 路程/移动速度
        /// </summary>
        public long moveNextTargetNeedIntervalTime;
    }
}