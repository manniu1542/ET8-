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

            //TODO:物理世界的执行输入逻辑
            LVector2 v2 = self.LSInput.V * 6 * 50 / 1000;

            if (v2.sqrMagnitude.ToFloat() < 0.0001f)
            {
                return;
            }

            // LVector2 oldPos = unit.Position;
            unit.Position += new LVector3(v2.x, 0, v2.y);
            if (v2 != LVector2.zero)
            {
                unit.Forward = new LVector3(v2.x, 0, v2.y).normalized;
            }
            // unit.Forward = unit.Position - new LVector3(oldPos.x, 0, oldPos.y);
       
        }
    }
}