﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Persistence
{
    public enum ReviseMode
    {
        /// <summary>Called after instantiation</summary>
        INIT = 0,

        /// <summary>Activate the item: register event handlers, ...</summary>
        ACTIVATE,

        /// <summary>Deactivate the item: degister event handlers, ...</summary>
        DEACTIVATE,

        /// <summary>Clear the item prior deserialization/defaulting</summary>
        CLEAR
    }

    public interface IPersistableItem
    {
        void Serialize(SerializerStream writer);
        void Deserialize(DeserializerStream reader);
        
        /// <summary>
        /// Fill the arguments list needed to initialize the first non-IpersistableItem
        /// base type in the hiearchy of this reference 
        /// </summary>
        /// <param name="args">Fill with the arguments needed to initialize the
        ///     non-IPersistableItem base type</param>
        void SetInitList(ref object[] args);

        void Revise(ReviseMode mode);
    }

    public interface IDefaultableItem
    {
        void Default();
    }

    public interface IContextAwareItem<T>
    {
        T Context
        {
            set;
        }
    }
}
