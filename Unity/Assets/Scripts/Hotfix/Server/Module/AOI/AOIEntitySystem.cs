using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(AOIEntity))]
    [FriendOf(typeof(AOIEntity))]
    public static partial class AOIEntitySystem
    {
        [EntitySystem]
        private static void Awake(this AOIEntity self, int distance, float3 pos)
        {
            self.ViewDistance = distance;
            self.Scene().GetComponent<AOIManagerComponent>().Add(self, pos.x, pos.z);
        }

        [EntitySystem]
        private static void Destroy(this AOIEntity self)
        {
            self.Scene().GetComponent<AOIManagerComponent>()?.Remove(self);
            self.ViewDistance = 0;
            self.SeeUnits.Clear();
            self.SeePlayers.Clear();
            self.BeSeePlayers.Clear();
            self.BeSeeUnits.Clear();
            self.SubEnterCells.Clear();
            self.SubLeaveCells.Clear();
        }
    }

    [FriendOf(typeof(AOIEntity))]
    [FriendOf(typeof(Cell))]
    public static partial class AOIEntitySystem
    {
        // 获取在自己视野中的对象
        public static Dictionary<long, EntityRef<AOIEntity>> GetSeeUnits(this AOIEntity self)
        {
            return self.SeeUnits;
        }

        public static Dictionary<long, EntityRef<AOIEntity>> GetBeSeePlayers(this AOIEntity self)
        {
            Log.Error("MESSAGE:" + self.BeSeePlayers.Count);
            return self.BeSeePlayers;
        }

        public static Dictionary<long, EntityRef<AOIEntity>> GetSeePlayers(this AOIEntity self)
        {
            return self.SeePlayers;
        }
        
        // cell中的unit进入self的视野   150 - 130
        public static void SubEnter(this AOIEntity self, Cell cell)
        {
            cell.SubsEnterEntities.Add(self.Id, self);
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in cell.AOIUnits)
            {
                if (kv.Key == self.Id)
                {
                    continue;
                }

                self.EnterSight(kv.Value);
            }
        }

        public static void UnSubEnter(this AOIEntity self, Cell cell)
        {
            cell.SubsEnterEntities.Remove(self.Id);
        }

        public static void SubLeave(this AOIEntity self, Cell cell)
        {
            cell.SubsLeaveEntities.Add(self.Id, self);
        }

        // cell中的unit离开self的视野
        public static void UnSubLeave(this AOIEntity self, Cell cell)
        {
            //这些刚好离开的Cell检查下，他 下面的Unit的AOI是否有可看到自己的。如果有就通知自己走了。
            foreach (KeyValuePair<long, EntityRef<AOIEntity>> kv in cell.AOIUnits)
            {
                if (kv.Key == self.Id)
                {
                    continue;
                }

                self.LeaveSight(kv.Value);
            }

            cell.SubsLeaveEntities.Remove(self.Id);
        }

        // enter进入self视野
        public static void EnterSight(this AOIEntity self, AOIEntity enter)
        {
         
            // 检查self是否已经看到enter，避免重复处理
            if (self.SeeUnits.ContainsKey(enter.Id))
            {
                return;
            }

            // 检查self和enter是否满足可见性条件
            if (!AOISeeCheckHelper.IsCanSee(self, enter))
            {
                return;
            }

            // 根据self和enter的类型，更新它们的可见单位列表
            if (self.Unit.Type() == UnitType.Player)
            {
                if (enter.Unit.Type() == UnitType.Player)
                {
                    // 玩家之间互相可见，更新双方的SeeUnits和SeePlayers列表
                    self.SeeUnits.Add(enter.Id, enter);
                    enter.BeSeeUnits.Add(self.Id, self);
                    self.SeePlayers.Add(enter.Id, enter);
                    enter.BeSeePlayers.Add(self.Id, self);
                }
                else
                {
                    // 玩家可见非玩家单位，更新双方的SeeUnits列表，并更新非玩家单位的BeSeePlayers列表
                    self.SeeUnits.Add(enter.Id, enter);
                    enter.BeSeeUnits.Add(self.Id, self);
                    enter.BeSeePlayers.Add(self.Id, self);
                }
            }
            else // 非玩家单位 的视野看到
            {
                if (enter.Unit.Type() == UnitType.Player)
                {
                    // 非玩家单位可见玩家，更新双方的SeeUnits列表，并更新非玩家单位的SeePlayers列表
                    self.SeeUnits.Add(enter.Id, enter);
                    enter.BeSeeUnits.Add(self.Id, self);
                    self.SeePlayers.Add(enter.Id, enter);
                }
                else
                {
                    // 非玩家单位之间互相可见，仅更新双方的SeeUnits列表
                    self.SeeUnits.Add(enter.Id, enter);
                    enter.BeSeeUnits.Add(self.Id, self);
                }
            }

            // 发布事件通知，self看到了enter，广播消息给指定unit客户端
            EventSystem.Instance.Publish(self.Scene(), new UnitEnterSightRange() { A = self, B = enter });
        }

      
        /// <summary>
        ///  leave离开self视野,当一个实体离开另一个实体的视野时调用此方法处理相关的逻辑。
        /// </summary>
        /// <param name="self">视野主体实体。</param>
        /// <param name="leave">离开视野的实体。</param>
        public static void LeaveSight(this AOIEntity self, AOIEntity leave)
        {
            // 检查两个实体的ID是否相同，相同则不执行离开视野的逻辑
            if (self.Id == leave.Id)
            {
                return;
            }
        
            // 检查视野主体实体是否能看到即将离开视野的实体，看不到则直接返回
            if (!self.SeeUnits.ContainsKey(leave.Id))
            {
                return;
            }
        
            // 从视野主体实体的可见单位字典中移除离开视野的实体
            self.SeeUnits.Remove(leave.Id);
        
            // 如果离开视野的实体是玩家类型，则从视野主体实体的可见玩家字典中移除该玩家
            if (leave.Unit.Type() == UnitType.Player)
            {
                self.SeePlayers.Remove(leave.Id);
            }
        
            // 从离开视野的实体的被观察单位字典中移除视野主体实体
            leave.BeSeeUnits.Remove(self.Id);
        
            // 如果视野主体实体是玩家类型，则从离开视野的实体的被观察玩家字典中移除该玩家
            if (self.Unit.Type() == UnitType.Player)
            {
                leave.BeSeePlayers.Remove(self.Id);
            }
        
            // 发布事件通知场景中有一个实体离开了另一个实体的视野范围，广播消息给指定unit客户端
            EventSystem.Instance.Publish(self.Scene(), new UnitLeaveSightRange { A = self, B = leave });
        }

        /// <summary>
        /// 是否在Unit视野范围内
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>
        public static bool IsBeSee(this AOIEntity self, long unitId)
        {
            return self.BeSeePlayers.ContainsKey(unitId);
        }
    }
}