using System;
using UnityEngine;
namespace ScrollViewUI
{



    /// <summary>
    /// 控制缓动的UI组件
    /// </summary>
    public class UITweenSpring : MonoBehaviour
    {
        
        /// <summary>
        /// 动量数量，物体运动的状态
        /// </summary>
        public float MomentumAmount = 1;
        //滑动 拖拽的 力度 系数 （越大，滑动播放的时间越短）
        public float Strength = 1;
        /// <summary>
        /// 开启 拖拽的回调 
        /// </summary>
        public bool IsUseCallBack;
        //动量向量   （质量与速度的乘积p=mv, 描述物体运动状态）
        public Vector3 Momentum;

        /// <summary>
        /// 移动中 回调
        /// </summary>
        public Action<Vector2> OnUpdate;
        /// <summary>
        /// 移动结束的 回调
        /// </summary>
        public Action OnMoveEnd;
        /// <summary>
        /// 开始的动量
        /// </summary>
        private Vector3 _startMomentum;
        /// <summary>
        /// 动量变化。间隔时间
        /// </summary>
        private float _duration;
        /// <summary>
        /// 开始的时间
        /// </summary>
        private float _startTime;

        /// <summary>
        /// 根据 传入的偏移量 计算动量，并设置开始的动量 和开始时间 
        /// </summary>
        /// <param name="offset"></param>
        public void LerpMomentum(Vector3 offset)
        {
            _startMomentum = offset * MomentumAmount;
            _startTime = 0;
        }
        //0.1s
        public void Rebound(int type, float strength) // x 0x1 y 0x10 z 0x100
        {
            switch (type)
            {
                case 1:
                    _startMomentum.x = -strength * _startMomentum.x;
                    break;
                case 2:
                    _startMomentum.y = -strength * _startMomentum.y;
                    break;
                case 3:
                    _startMomentum.x = -strength * _startMomentum.x;
                    _startMomentum.y = -strength * _startMomentum.y;
                    break;
                case 4:
                    _startMomentum.z = -strength * _startMomentum.z;
                    break;
                case 5:
                    _startMomentum.x = -strength * _startMomentum.x;
                    _startMomentum.z = -strength * _startMomentum.z;
                    break;
                case 6:
                    _startMomentum.y = -strength * _startMomentum.y;
                    _startMomentum.z = -strength * _startMomentum.z;
                    break;
                case 7:
                    _startMomentum.x = -strength * _startMomentum.x;
                    _startMomentum.y = -strength * _startMomentum.y;
                    _startMomentum.z = -strength * _startMomentum.z;
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!Application.isPlaying) return;
            _startTime += Time.unscaledDeltaTime;

            if (IsUseCallBack)
            {

                _duration = 1 / Strength;
                if (_startTime < _duration)
                {
                    //移动的动量 插值趋近于 0
                    Momentum = Vector3.Lerp(Vector3.zero, _startMomentum, 1 - Easing.cubicOut(_startTime / _duration));
                    OnUpdate(Momentum);
                }
                else
                {
                    MovenEnd();
                }

            }
        }

        public void MovenEnd()
        {
            this.enabled = false;
            _startTime = 0;
            Momentum = Vector3.zero;
            if (OnMoveEnd != null)
                OnMoveEnd();
        }

    }



}
