using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Reflection;

namespace NET.Persistence
{
    internal static class Hiearchical
    {
        public static void Revise<T>(ref T item, ReviseMode mode)
            where T : IPersistableItem
        {
            // NB: Value types can't be inherited
            if (item.GetType().IsValueType)
            {
                item.Revise(mode);
                return;
            }

            switch (mode)
            {
                case ReviseMode.INIT:
                case ReviseMode.ACTIVATE:
                {
                    MethodUtils.HierarchicalInvoke(item, "Revise",
                        new object[] { mode }, false);
                    break;
                }
                case ReviseMode.CLEAR:
                case ReviseMode.DEACTIVATE:
                {
                    MethodUtils.HierarchicalInvoke(item, "Revise",
                        new object[] { mode }, true);
                    break;
                }
            }
        }

        public static void Serialize<T>(ref T item, SerializerStream writer)
            where T : IPersistableItem
        {
            // NB: Value types can't be inherited
            if (item.GetType().IsValueType)
            {
                item.Serialize(writer);
                return;
            }

            MethodUtils.HierarchicalInvoke(item, "Serialize",
                new object[] { writer }, false);
        }

        public static void Deserialize<T>(ref T item, DeserializerStream reader)
            where T : IPersistableItem
        {
            // NB: Value types can't be inherited
            if (item.GetType().IsValueType)
            {
                item.Deserialize(reader);
                return;
            }

            MethodUtils.HierarchicalInvoke(item, "Deserialize",
                new object[] { reader }, false);
        }
    }

    public static class ItemUtils
    {
        private delegate void ItemAction(IPersistableItem item);

        /// <summary>
        /// Reinitialize to default values the provided item
        /// The class constraint is necessary because you can't side effect value types
        /// with extension methods
        /// </summary>
        public static void ToDefault<T>(this T item)
            where T : class, IPersistableItem, IDefaultableItem
        {
            Hiearchical.Revise(ref item, ReviseMode.DEACTIVATE);
            Hiearchical.Revise(ref item, ReviseMode.CLEAR);

            MethodUtils.HierarchicalInvoke(item, "Default",
                new object[0], false);

            Hiearchical.Revise(ref item, ReviseMode.ACTIVATE);
        }

        /// <summary>
        /// Reinitialize to default values the provided item
        /// </summary>
        public static void ToDefault<T>(ref T item)
            where T : IPersistableItem, IDefaultableItem
        {
            Hiearchical.Revise(ref item, ReviseMode.DEACTIVATE);
            Hiearchical.Revise(ref item, ReviseMode.CLEAR);

            Type typeSafe = ItemUtils.GetTypeSafe(ref item);
            if (typeSafe.IsValueType)
                item.Default();
            else
                MethodUtils.HierarchicalInvoke(item, "Default",
                    new object[0], false);

            Hiearchical.Revise(ref item, ReviseMode.ACTIVATE);
        }

        internal static Type GetTypeSafe<T>(ref T item)
            where T : IPersistableItem
        {
            if (item == null)
                return typeof(T);
            else
                return item.GetType();
        }

        internal static string ComputeVersion(Type type)
        {
            StringBuilder version = new StringBuilder();

            foreach (object attribute in type.GetCustomAttributes(true))
            {
                if (attribute is ItemVersion)
                    version.Append((attribute as ItemVersion).Version);
            }

            if (version.Length == 0)
                return null;
            else
                return version.ToString();
        }
    }
}
