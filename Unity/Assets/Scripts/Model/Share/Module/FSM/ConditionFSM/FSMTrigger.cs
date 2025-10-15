// using System.Collections.Generic;
//
// namespace ET
// {
//     [EnableClass]
//     /// <summary>触发条件</summary>
//     public abstract class FSMTrigger
//     {
//         /// <summary>子条件</summary>
//         public List<FSMTrigger> ChildTrigger = new List<FSMTrigger>();
//         ///<summary>条件Id</summary>
//         public abstract int TriggerId { get; set; }
//         /// <summary>检查条件是否满足</summary>
//         protected abstract bool Evaluate(FSM fsm);
//
//         /// <summary>检查子条件</summary>
//         protected bool CheckSubTriggers(FSM fsm)
//         {
//             if (ChildTrigger.Count == 0) return true;
//
//             for (int i = 0; i < ChildTrigger.Count; i++)
//             {
//                 if (!ChildTrigger[i].Evaluate(fsm))
//                 {
//                     return false;
//                 }
//             }
//             return true;
//         }
//
//         ///<summary>添加子条件</summary>
//         public void AddChildTrigger<T>() where T : FSMTrigger, new()
//         {
//             FSMTrigger trigger = new T();
//             if (trigger.TriggerId == TriggerId)
//             {
//                 Log.Error($"不能添加默认条件");
//                 trigger.Dispose();
//                 return;
//             }
//             if (ChildTrigger.Exists(t => t.TriggerId == trigger.TriggerId))
//             {
//                 Log.Error($"条件 {typeof(T).Name} 已经添加");
//                 trigger.Dispose();
//                 return;
//             }
//             ChildTrigger.Add(trigger);
//         }
//
//         /// <summary>处理条件检查，检查本条件及子条件</summary>
//         public bool HandleEvaluate(FSM fsm)
//         {
//             bool result = Evaluate(fsm) && CheckSubTriggers(fsm);
//             return result;
//         }
//
//         public virtual void Dispose()
//         {
//             if (ChildTrigger.Count > 0)
//             {
//                 for (int i = 0; i < ChildTrigger.Count; i++)
//                 {
//                     ChildTrigger[i].Dispose();
//                 }
//                 ChildTrigger.Clear();
//             }
//         }
//     }
// }