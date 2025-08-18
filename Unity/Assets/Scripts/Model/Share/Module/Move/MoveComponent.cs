// using System;
// using System.Collections.Generic;
// using Unity.Mathematics;
//
// namespace ET
// {
//
//     /// <summary>
//     /// Unit 的移动组件，用于处理路径移动、转向和异步控制
//     /// 注意：这是一个前后端共用的组件
//     /// </summary>
//     [ComponentOf(typeof(Unit))] // 表示这个组件属于 Unit 实体
//     public class MoveComponent : Entity, IAwake, IDestroy
//     {
//         /// <summary>
//         /// 获取当前移动路径中的上一个目标点
//         /// </summary>
//         public float3 PreTarget
//         {
//             get
//             {
//                 return this.Targets[this.N - 1];
//             }
//         }
//
//         /// <summary>
//         /// 获取当前移动路径中的下一个目标点
//         /// </summary>
//         public float3 NextTarget
//         {
//             get
//             {
//                 return this.Targets[this.N];
//             }
//         }
//
//         /// <summary>
//         /// 移动协程开始的时间戳（毫秒）
//         /// </summary>
//         public long BeginTime;
//
//         /// <summary>
//         /// 当前路径段的开始时间（毫秒）
//         /// </summary>
//         public long StartTime { get; set; }
//
//         /// <summary>
//         /// 移动协程开始时 Unit 的位置
//         /// </summary>
//         public float3 StartPos;
//
//         /// <summary>
//         /// 获取 Unit 的真实位置（总是返回路径的第一个点）
//         /// </summary>
//         public float3 RealPos
//         {
//             get
//             {
//                 return this.Targets[0];
//             }
//         }
//
//         private long needTime;
//
//         /// <summary>
//         /// 移动到下一个目标点所需的时间（毫秒）
//         /// </summary>
//         public long NeedTime
//         {
//             get => this.needTime;
//             set => this.needTime = value;
//         }
//
//         /// <summary>
//         /// 驱动移动的定时器ID
//         /// </summary>
//         public long MoveTimer;
//
//         /// <summary>
//         /// 移动速度（米/秒）
//         /// </summary>
//         public float Speed;
//
//         /// <summary>
//         /// 移动异步任务控制器
//         /// </summary>
//         public ETTask<bool> tcs;
//
//         /// <summary>
//         /// 移动路径的目标点列表
//         /// </summary>
//         public List<float3> Targets = new List<float3>();
//
//         /// <summary>
//         /// 获取最终目标点
//         /// </summary>
//         public float3 FinalTarget
//         {
//             get
//             {
//                 return this.Targets[this.Targets.Count - 1];
//             }
//         }
//
//         /// <summary>
//         /// 当前路径点的索引
//         /// </summary>
//         public int N;
//
//         /// <summary>
//         /// 转向过渡时间（毫秒）
//         /// </summary>
//         public int TurnTime;
//
//         /// <summary>
//         /// 是否只水平方向转向（忽略Y轴旋转）
//         /// </summary>
//         public bool IsTurnHorizontal;
//
//         /// <summary>
//         /// 转向的起始旋转
//         /// </summary>
//         public quaternion From;
//
//         /// <summary>
//         /// 转向的目标旋转
//         /// </summary>
//         public quaternion To;
//     }
//
// }