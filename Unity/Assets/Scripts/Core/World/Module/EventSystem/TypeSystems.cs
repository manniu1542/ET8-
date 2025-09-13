using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 存储所有Entiy类的回调方法管理系统
    /// </summary>
    public class TypeSystems
    {
        public class OneTypeSystems
        {
            public OneTypeSystems(int count)
            {
                this.QueueFlag = new bool[count];
            }
            //无序的 key对应一个list列表的容器,  key是继承了ISystemType接口类型，以及所对应的 生成的方法类型。
            public readonly UnOrderMultiMap<Type, SystemObject> Map = new();
            // 这里不用hash，数量比较少，直接for循环速度更快
            public readonly bool[] QueueFlag;
        }

        private readonly int count;

        public TypeSystems(int count)
        {
            this.count = count;
        }
        //存储 key：打标签EnitySystem的方法 所属的 组件类型， value是这个类型的  生成方法的类型。以及她所对应的具体方法
        private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

        public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
        {
            OneTypeSystems systems = null;
            this.typeSystemsMap.TryGetValue(type, out systems);
            if (systems != null)
            {
                return systems;
            }

            systems = new OneTypeSystems(this.count);
            this.typeSystemsMap.Add(type, systems);
            return systems;
        }

        public OneTypeSystems GetOneTypeSystems(Type type)
        {
            OneTypeSystems systems = null;
            this.typeSystemsMap.TryGetValue(type, out systems);
            return systems;
        }

        public List<SystemObject> GetSystems(Type type, Type systemType)
        {
            OneTypeSystems oneTypeSystems = null;
            if (!this.typeSystemsMap.TryGetValue(type, out oneTypeSystems))
            {
                return null;
            }

            if (!oneTypeSystems.Map.TryGetValue(systemType, out List<SystemObject> systems))
            {
                return null;
            }

            return systems;
        }
    }
}