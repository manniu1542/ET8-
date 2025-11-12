using System;
using Lockstep.Math;
using UnityEngine;
using ZHFSM;

namespace ET.Client
{
    [Event(SceneType.Room)]
    public class LSUpdateEventHandler : AEvent<Room, LSUpdateEvent>
    {
        protected override async ETTask Run(Room room, LSUpdateEvent args)
        {
          
            if(room.Id>100)return; //客户端跟服务端都在unity中跑。会出现这个情况。忽略服务端的
                
            LSUnitViewComponent lsUnitComponent = room.GetComponent<LSUnitViewComponent>();
         
            //view层的 同步数据
            foreach (long playerId in room.PlayerIds)
            {
                LSUnitView lsUnit = lsUnitComponent.GetChild<LSUnitView>(playerId);
                lsUnit?.GetComponent<LSUnitState>()?.LSUpdate();
            }

            await ETTask.CompletedTask;
        }
    }

    [FriendOf(typeof(LSUnit))]
    [EntitySystemOf(typeof(LSUnitState))]
    [LSEntitySystemOf(typeof(LSUnitState))]
    [FriendOf(typeof(LSUnitState))]
    public static partial class LSUnitStateSystem
    {
        [EntitySystem]
        private static void Awake(this ET.LSUnitState self, StateMachineExecutor executor)
        {
            self.playerHFSM = executor;
            self.playerHFSM.Init();
        }

        //
        [LSEntitySystem]
        private static void LSRollback(this LSUnitState self)
        {
            Log.Error("回滚状态！");
            var unitView = self.Parent as LSUnitView;
            var unit = unitView.GetUnit();
            // ZHFSM.HFSMDataSnapshot tt = new ZHFSM.HFSMDataSnapshot();
            // tt.curStateName = unit.dataSnapshot.curStateName;
            // tt.arrStateMachineName = unit.dataSnapshot.arrStateMachineName;
            // tt.arrParameters = unit.dataSnapshot.arrParameters;
            // tt.elapsed = unit.dataSnapshot.elapsed;
            // HFSMDataSnapshotHelper.SetSnapshot(self.playerHFSM, tt);
        }

        public static void LSUpdate(this ET.LSUnitState self)
        {
            self.playerHFSM.OnUpdate();
            self.Reset();
        }

        public static void Reset(this LSUnitState self)
        {
            var unitView = self.Parent as LSUnitView;
            var unit = unitView.GetUnit();
            // var tt = HFSMDataSnapshotHelper.GetSnapshot(self.playerHFSM);
            // unit.dataSnapshot = new HFSMDataSnapshot();
            // unit.dataSnapshot.curStateName = tt.curStateName;
            // unit.dataSnapshot.arrStateMachineName = tt.arrStateMachineName;
            // unit.dataSnapshot.arrParameters = tt.arrParameters;
            // unit.dataSnapshot.elapsed = tt.elapsed;
        }

        [EntitySystem]
        private static void Update(this ET.LSUnitState self)
        {
        }
    }
}