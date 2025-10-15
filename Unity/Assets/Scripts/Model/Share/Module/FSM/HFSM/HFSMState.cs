using System.Collections.Generic;

namespace ET
{
    [EnableClass]
    public abstract class HFSMState
    {
        public abstract int StateId { get; }
        public HFSM _FSM;

        // ✅ 新增层级信息
        public HFSMState ParentState { get; private set; }
        private List<HFSMState> _children = new List<HFSMState>();
        private HFSMState _activeChild;
        private int _defaultChildId = -1;
        /// <summary>
        /// 状态初始化方法
        /// </summary>
        public virtual void Awake()
        {
            // 默认空实现，子类可重写
        }

        // ✅ 状态树构建（添加子状态）
        public void AddChildState(HFSMState child, bool isDefault = false)
        {
            if (child == null || _children.Contains(child))
                return;

            child.ParentState = this;
            _children.Add(child);

            if (isDefault)
                _defaultChildId = child.StateId;
        }

        // ✅ 获取当前激活子状态
        public HFSMState GetActiveChild() => _activeChild;

        // ✅ 状态生命周期
        public virtual void EnterState()
        {
            OnEnter();

            // 进入默认子状态
            if (_defaultChildId != -1)
            {
                var def = _children.Find(s => s.StateId == _defaultChildId);
                if (def != null)
                {
                    _activeChild = def;
                    _activeChild.EnterState();
                }
            }
        }

        public virtual void ExitState()
        {
            // 子状态先退出
            _activeChild?.ExitState();
            _activeChild = null;
            OnExit();
        }

        public virtual void Update()
        {
            OnUpdate();
            _activeChild?.Update();
        }

        public virtual void Reason()
        {
            OnReason();
            _activeChild?.Reason();
        }

        // ✅ 子类实现的核心逻辑
        protected abstract void OnEnter();
        protected abstract void OnExit();
        protected abstract void OnUpdate();
        protected virtual void OnReason() { }

        // ✅ 层级切换
        public void SwitchSubState(int childId)
        {
            HFSMState target = _children.Find(c => c.StateId == childId);
            if (target == null)
            {
                Log.Error($"子状态 {childId} 不存在于 {StateId}");
                return;
            }

            // 退出当前子状态
            _activeChild?.ExitState();
            _activeChild = target;
            _activeChild.EnterState();
        }

        // ✅ 清理
        public virtual void Dispose()
        {
            foreach (var c in _children)
                c.Dispose();
            _children.Clear();
            _activeChild = null;
            ParentState = null;
        }
        
    }
}