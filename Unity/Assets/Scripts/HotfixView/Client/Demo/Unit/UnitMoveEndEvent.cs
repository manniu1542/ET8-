using System;
using ET.Client;
using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class UnitMoveStopEvent : AEvent<Scene, MoveStop>
    {
        protected override async ETTask Run(Scene root, MoveStop args)
        {
            try
            {
                args.Unit.GetComponent<AnimatorComponent>().Play(MotionType.Idle, 0);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            await ETTask.CompletedTask;
        }
    }
}