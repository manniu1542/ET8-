using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class ChangAnimation_MoveStart : AEvent<Scene, MoveStart>
    {
        protected override async ETTask Run(Scene scene, MoveStart args)
        {
            Unit unit = args.Unit;

            AnimatorComponent aniCpt = unit.GetComponent<AnimatorComponent>();
            aniCpt?.SetFloatValue("Speed", 5);

            await ETTask.CompletedTask;
        }
    }
}