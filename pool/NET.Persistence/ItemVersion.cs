using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Persistence
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ItemVersion : Attribute
    {
        private int _Version;

        public ItemVersion(int version)
        {
            _Version = version;
        }

        public int Version
        {
            get { return _Version; }
        }
    }
}
