using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class ChangAnimation_MoveStop : AEvent<Scene, MoveStop>
    {
        protected override async ETTask Run(Scene scene, MoveStop args)
        {
            Unit unit = args.Unit;

            AnimatorComponent aniCpt = unit.GetComponent<AnimatorComponent>();
            aniCpt?.SetFloatValue("Speed", 0);

            await ETTask.CompletedTask;
        }
    }
}