// using System.Collections.Generic;
//
// namespace ET
// {
//     ///<summary>状态</summary>
//     public abstract class FSMState
//     {
//         ///<summary>条件->输出状态的映射关系</summary>
//         private class Mapping
//         {
//             ///<summary>条件Id</summary>
//             public int[] TriggerIds;
//             ///<summary>状态Id</summary>
//             public int StateId;
//         }
//
//         /// <summary>状态Id</summary>
//         public abstract int StateId { get; }
//         ///<summary>状态机</summary>
//         public FSM _FSM;
//         /// <summary>触发条件的列表</summary>
//         private List<FSMTrigger> Triggers = new List<FSMTrigger>();
//         ///<summary>条件->输出状态的映射关系</summary>
//         private List<Mapping> AllMapping = new List<Mapping>();
//         ///<summary>条件检查</summary>
//         private Dictionary<int, bool> TriggerEvaluate = new Dictionary<int, bool>();
//
//         /// <summary>添加条件</summary>
//         /// <typeparam name="T">条件类型</typeparam>
//         public void AddTrigger<T>() where T : FSMTrigger, new()
//         {
//             FSMTrigger trigger = new T();
//             if (!Triggers.Exists(t => t.TriggerId == trigger.TriggerId))
//             {
//                 Triggers.Add(trigger);
//                 TriggerEvaluate.Add(trigger.TriggerId, false);
//             }
//             else
//             {
//                 trigger.Dispose();
//                 Log.Error($"条件 {typeof(T).Name} 已经添加");
//             }
//         }
//
//         ///<summary>添加条件映射</summary>
//         public void AddMapping(int stateId, params int[] triggers)
//         {
//             if (triggers.Length > 0)
//             {
//                 AllMapping.Add(new Mapping
//                 {
//                     TriggerIds = triggers,
//                     StateId = stateId,
//                 });
//             }
//         }
//
//         /// <summary>0.初始化</summary>
//         public abstract void Awake();
//
//         /// <summary>1.进入状态时</summary>
//         public abstract void EnterState();
//
//         /// <summary>2.离开状态时</summary>
//         public abstract void ExitState();
//
//         /// <summary>3.持续状态时，执行的主要行为</summary>
//         public abstract void Update();
//
//         /// <summary>4.检查转换条件</summary>
//         public virtual void Reason()
//         {
//             if (AllMapping.Count > 0)
//             {
//                 for (int i = 0; i < Triggers.Count; i++)
//                 {
//                     bool evaluate = Triggers[i].HandleEvaluate(_FSM);
//                     TriggerEvaluate[Triggers[i].TriggerId] = evaluate;
//                 }
//                 for (int i = 0; AllMapping.Count > 0; i++)
//                 {
//                     Mapping mapping = AllMapping[i];
//                     bool evaluate = false;
//                     for (int j = 0; j < mapping.TriggerIds.Length; j++)
//                     {
//                         evaluate = TriggerEvaluate[mapping.TriggerIds[j]];
//                         if (!evaluate)
//                             break;
//                     }
//                     if (evaluate)
//                     {
//                         _FSM.SwitchState(mapping.StateId);
//                         return;
//                     }
//                 }
//             }
//         }
//
//         public virtual void Dispose()
//         {
//             for (int i = 0; i < Triggers.Count; i++)
//             {
//                 Triggers[i].Dispose();
//             }
//             Triggers.Clear();
//             AllMapping.Clear();
//             TriggerEvaluate.Clear();
//         }
//         
//         
//         
//         
//     }
// }