using System;
using Lockstep.Math;
using UnityEngine;
using ZHFSM;

namespace ET.Client
{
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
            var unitView = self.Parent as LSUnitView;
            var unit = unitView.GetUnit();
            ZHFSM.HFSMDataSnapshot tt = new ZHFSM.HFSMDataSnapshot();
            tt.curStateName=    unit.dataSnapshot.curStateName ;
            tt.arrStateMachineName= unit.dataSnapshot.arrStateMachineName;
            tt.arrParameters= unit.dataSnapshot.arrParameters;
            tt.elapsed= unit.dataSnapshot.elapsed;
            HFSMDataSnapshotHelper.SetSnapshot(self.playerHFSM,tt);
        }

        [LSEntitySystem]
        private static void LSUpdate(this ET.LSUnitState self)
        {
            return;
            self.playerHFSM.OnUpdate();
            self.Reset();
        }

        public static void Reset(this LSUnitState self)
        {
            
            var unitView = self.Parent as LSUnitView;
            var unit = unitView.GetUnit();
            var tt = HFSMDataSnapshotHelper.GetSnapshot(self.playerHFSM);
            unit.dataSnapshot = new HFSMDataSnapshot();
            unit.dataSnapshot.curStateName = tt.curStateName;
            unit.dataSnapshot.arrStateMachineName = tt.arrStateMachineName;
            unit.dataSnapshot.arrParameters = tt.arrParameters;
            unit.dataSnapshot.elapsed = tt.elapsed;
            
        }
    }
}