using System.Collections.Generic;

namespace ET
{
    [EnableClass]
    ///<summary>状态机基类</summary>
    public abstract class HFSM
    {
        /// <summary>状态对象库,保存AI所有状态</summary>
        private Dictionary<int, HFSMState> HFSMStates = new Dictionary<int, HFSMState>();
        /// <summary>当前状态</summary>
        protected HFSMState CurrentState;
        /// <summary>默认状态</summary>
        private HFSMState DefaultState;
        /// <summary>默认状态ID</summary>
        protected int DefaultStateId;
        ///<summary>是否暂停</summary>
        protected bool IsPause;

        public HFSM()
        {
            RegisterState();
        }

        ///<summary>开始</summary>
        public void Begin()
        {
            if (HFSMStates.Count == 0)
            {
                Log.Error("没有状态加入链表!");
                return;
            }

            DefaultState = HFSMStates[DefaultStateId];
            CurrentState = DefaultState;
            CurrentState.EnterState();
        }

        /// <summary>注册状态</summary>
        public abstract void RegisterState();

        /// <summary>Update 在状态判断之后执行</summary>
        public virtual void Update()
        {
            if (IsPause) return;

            if (CurrentState != null)
            {
                CurrentState.Update();
                CurrentState.Reason();
            }
        }

        /// <summary>创建状态</summary>
        public T CreateState<T>() where T : HFSMState, new()
        {
            HFSMState state = new T();
            if (!HFSMStates.ContainsKey(state.StateId))
            {
                HFSMStates.Add(state.StateId, state);
                state._FSM = this;
                state.Awake();
                return (T)state;
            }
            state.Dispose();
            return null;
        }

        /// <summary>删除状态</summary> 
        public void DeleteState(int stateid)
        {
            if (HFSMStates.ContainsKey(stateid))
            {
                HFSMStates.Remove(stateid);
            }
        }

        ///<summary>根据状态类型手动切换状态</summary>
        ///<param name="stateid">状态Id</param>
        public void SwitchState(int stateid)
        {
            Log.Info($"切换到 {stateid} 状态");
            if (IsPause) return;

            HFSMState state = HFSMStates[stateid];
            if (state == null)
            {
                Log.Info($"状态机{this.GetType().Name}没有{stateid}状态");
            }

            //1.None : 不处理
            if (stateid == -1)
                return;
            //退出当前状态
            CurrentState.ExitState();
            //2.默认状态: 将原默认状态设为当前状态
            if (stateid == DefaultStateId)
                CurrentState = DefaultState;
            else //3.具体状态: 将具体状态设为当前状态
                CurrentState = state;
            //进入新状态
            CurrentState.EnterState();
        }

        /// <summary>退出状态机</summary>
        public void QuitFSM()
        {
            CurrentState = null;
            DefaultState = null;
            DefaultStateId = -1;
        }

        ///<summary>重新进入当前状态</summary>
        public void RetryExitCurrentState()
        {
            if (CurrentState != null)
                SwitchState(CurrentState.StateId);
        }

        ///<summary>暂停</summary>
        public void Pause()
        {
            IsPause = true;
        }

        ///<summary>取消暂停</summary>
        public void Unpause()
        {
            IsPause = false;
        }
    
        public virtual void Dispose()
        {
            foreach (var item in HFSMStates)
            {
                item.Value.Dispose();
            }
            HFSMStates.Clear();
        }
    }
}