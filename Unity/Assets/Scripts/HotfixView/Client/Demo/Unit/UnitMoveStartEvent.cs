using System;
using ET.Client;
using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class UnitMoveStartEvent : AEvent<Scene, MoveStart>
    {
        protected override async ETTask Run(Scene root, MoveStart args)
        {
            try
            {
              
                args.Unit.GetComponent<AnimatorComponent>().Play(MotionType.Run,5f);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            await ETTask.CompletedTask;
        }
    }
}