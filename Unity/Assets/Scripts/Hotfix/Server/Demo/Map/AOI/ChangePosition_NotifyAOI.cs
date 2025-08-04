using Unity.Mathematics;

namespace ET.Server
{
    [Event(SceneType.Map)]
    public class ChangePosition_NotifyAOI : AEvent<Scene, ChangePosition>
    {
        protected override async ETTask Run(Scene scene, ChangePosition args)
        {
            Unit unit = args.Unit;
            float3 oldPos = args.OldPos;
            int oldCellX, oldCellY, newCellX, newCellY;

            if (ConstValue.IsUseSelfAOI)
            {
                oldCellX = AreaCellHelper.GridSizeCalculation(oldPos.x);
                oldCellY = AreaCellHelper.GridSizeCalculation(oldPos.z);
                newCellX = AreaCellHelper.GridSizeCalculation(unit.Position.x);
                newCellY = AreaCellHelper.GridSizeCalculation(unit.Position.z);
                if (oldCellX == newCellX && oldCellY == newCellY)
                {
                    return;
                }

                AreaOfInterestEntity aoiEntity = unit.GetComponent<AreaOfInterestEntity>();
                if (aoiEntity == null)
                {
                    return;
                }

                unit.Scene().GetComponent<AreaCellMgrComponent>().Move(aoiEntity, newCellX, newCellY);
            }
            else
            {
                oldCellX = (int)(oldPos.x * 1000) / AOIManagerComponent.CellSize;
                oldCellY = (int)(oldPos.z * 1000) / AOIManagerComponent.CellSize;
                newCellX = (int)(unit.Position.x * 1000) / AOIManagerComponent.CellSize;
                newCellY = (int)(unit.Position.z * 1000) / AOIManagerComponent.CellSize;
                if (oldCellX == newCellX && oldCellY == newCellY)
                {
                    return;
                }

                AOIEntity aoiEntity = unit.GetComponent<AOIEntity>();
                if (aoiEntity == null)
                {
                    return;
                }

                unit.Scene().GetComponent<AOIManagerComponent>().Move(aoiEntity, newCellX, newCellY);
            }

            await ETTask.CompletedTask;
        }
    }
}