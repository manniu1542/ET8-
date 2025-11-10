using Lockstep.Math;
using MemoryPack;
using UnityEngine;
using ZHFSM;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSUnitViewComponent))]
    public static partial class LSUnitViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSUnitViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LSUnitViewComponent self)
        {
        }

        public static async ETTask InitAsync(this LSUnitViewComponent self)
        {
            Room room = self.Room();
            LSUnitComponent lsUnitComponent = room.LSWorld.GetComponent<LSUnitComponent>();
            Scene root = self.Root();
            int idx = 0;
            foreach (long playerId in room.PlayerIds)
            {
                LSUnit lsUnit = lsUnitComponent.GetChild<LSUnit>(playerId);
                string assetsName = $"Assets/Bundles/Unit/Unit.prefab";
                GameObject bundleGameObject = await room.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
                bool isPlayer1 = ++idx % 2 == 0;
                string playerResName = $"Player{(isPlayer1 ? 1 : 2)}";
                GameObject prefab = bundleGameObject.Get<GameObject>(playerResName);

                GlobalComponent globalComponent = root.GetComponent<GlobalComponent>();
                GameObject unitGo = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
                unitGo.transform.position = lsUnit.Position.ToVector3();

                LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
                string racResName = isPlayer1 ? "Frank_Fighting_Part1" : "Frank_Fighting_Set2";
                RuntimeAnimatorController rac = await room.GetComponent<ResourcesLoaderComponent>()
                        .LoadAssetAsync<RuntimeAnimatorController>(racResName);
                lsUnitView.AddComponent<LSAnimatorComponent, RuntimeAnimatorController>(rac);
                //给unit添加 状态组件

                lsUnitView.AddComponent<LSUnitState, StateMachineExecutor>(unitGo.GetComponent<StateMachineExecutor>());
            }
        }
    }
}