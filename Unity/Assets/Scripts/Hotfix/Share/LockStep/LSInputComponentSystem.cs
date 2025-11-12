using System;
using ET.Client;
using Lockstep.Math;

namespace ET
{
    [EntitySystemOf(typeof(LSInputComponent))]
    [LSEntitySystemOf(typeof(LSInputComponent))]
    public static partial class LSInputComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSInputComponent self)
        {
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSInputComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();
#if !DOTNET
            EventSystem.Instance.Publish(unit.LSWorld().Parent.Room(), new LSUpdateEvent());
#endif

            //TODO:玩家有个buff状态的挂载，这个状态会改变unit的状态。例如被击飞下。输入被屏蔽。

            self.InputDriveTransform(unit);
        }

        /// <summary>
        /// 输入驱动 unit的坐标转换
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        public static void InputDriveTransform(this LSInputComponent self, LSUnit unit)
        {
            LVector2 v2 = self.LSInput.V * 6 * 50 / 1000;

            if (v2.sqrMagnitude.ToFloat() < 0.0001f)
            {
                return;
            }

            unit.Position += new LVector3(v2.x, 0, v2.y);
            if (v2 != LVector2.zero)
            {
                unit.Forward = new LVector3(v2.x, 0, v2.y).normalized;
            }
        }

        /// <summary>
        /// 输入驱动 unit的状态（目前处于什么状态下）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        public static void InputDriveState(this LSInputComponent self, LSUnit unit)
        {
            LSInputButton inputBtn = (LSInputButton)self.LSInput.Button;

            if (inputBtn.HasButton(LSInputButton.Jump))
            {
                Log.Info("Jump");
            }
        }
    }
}