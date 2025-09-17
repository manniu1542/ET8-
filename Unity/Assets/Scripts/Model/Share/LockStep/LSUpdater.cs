using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// LSUpdater 用于管理并驱动 LSEntity 的逐帧更新
    /// </summary>
    public class LSUpdater : Object
    {
        /// <summary>
        /// 当前每帧需要更新的组件实体 Id 列表
        /// </summary>
        private List<long> updateIds = new();

        /// <summary>
        /// 在本帧中新增的组件实体 Id 列表
        /// </summary>
        private List<long> newUpdateIds = new();

        /// <summary>
        /// 实体字典（key: 实体Id, value: 实体引用）
        /// </summary>
        private readonly Dictionary<long, EntityRef<LSEntity>> lsEntities = new();

        /// <summary>
        /// 每帧调用，驱动所有 LSEntity 的更新逻辑
        /// </summary>
        public void Update()
        {
            // 如果有新增的实体，需要先合并进 updateIds
            if (this.newUpdateIds.Count > 0)
            {
                foreach (long id in this.newUpdateIds)
                {
                    this.updateIds.Add(id);
                }

                // 保证 updateIds 的顺序（通常按 Id 排序）
                this.updateIds.Sort();

                // 清空 newUpdateIds，准备给下一轮使用
                this.newUpdateIds.Clear();
            }

            // 遍历所有需要更新的实体
            foreach (long id in this.updateIds)
            {
                // 从字典中取出实体
                LSEntity entity = lsEntities[id];

                if (entity == null)
                {
                    // 如果实体已经被释放，移除无效引用
                    this.lsEntities.Remove(id);
                    continue;
                }

                this.newUpdateIds.Add(id);

                // 执行实体的逐帧逻辑更新
                LSEntitySystemSingleton.Instance.LSUpdate(entity);
            }

            // 清空当前 updateIds
            this.updateIds.Clear();

            // 交换两个列表的引用，避免频繁分配内存
            ObjectHelper.Swap(ref this.updateIds, ref this.newUpdateIds);
        }

        /// <summary>
        /// 添加一个新的 LSEntity 进入更新系统
        /// </summary>
        public void Add(LSEntity entity)
        {
            // 新加入的实体，先放到 newUpdateIds，下一帧会合并进 updateIds
            this.newUpdateIds.Add(entity.Id);

            // 存入实体字典
            this.lsEntities.Add(entity.Id, entity);
        }
    }
}
