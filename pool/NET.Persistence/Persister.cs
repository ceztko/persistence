using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace NET.Persistence
{
    /// <summary>
    /// Serialization/deserialization is supported for both reference and value types
    /// Persistence is supported for reference types only
    /// </summary>
    public abstract class Persister
    {
        #region Fields

        private ConditionalWeakTable<IPersistable, ILinkRef> _linkRefTable;
        private Dictionary<Guid, WeakReference> _guidRefTable;
        private Dictionary<Type, string> _savePaths;

        #endregion // Fields

        #region Constructors

        public Persister()
        {
            _linkRefTable = new ConditionalWeakTable<IPersistable, ILinkRef>();
            _guidRefTable = new Dictionary<Guid, WeakReference>();
            _savePaths = new Dictionary<Type, string>();
        }

        #endregion // Constructors

        #region Inquiry

        public void Serialize<T>(T item, string filepath)
            where T : class, IPersistable
        {
            using (SerializerStream serializableStream =
                GetSerializerStream(filepath))
            {
                serializableStream.WriteElement(PersistableUtils.GetTypeSafe(ref item).Name, ref item,
                    null, true, false);
            }
        }

        public void Serialize<T>(ref T item, string filepath)
            where T : IPersistable
        {
            using (SerializerStream serializableStream =
                GetSerializerStream(filepath))
            {
                serializableStream.WriteElement(PersistableUtils.GetTypeSafe(ref item).Name, ref item,
                    null, true, false);
            }
        }

        public void Deserialize<T>(ref T item, string filepath)
            where T : IPersistable
        {
            using (DeserializerStream deserializableStream =
                GetDeserializerStream(filepath))
            {
                Guid guid = Guid.Empty;
                deserializableStream.ReadElementAs(ref item, ref guid, true);
            }
        }

        public void Deserialize<T, TCtx>(ref T item, TCtx context, string filepath)
            where T : IPersistable, IContextAware<TCtx>
        {
            using (DeserializerStream deserializableStream =
                GetDeserializerStream(filepath))
            {
                Guid guid = Guid.Empty;
                deserializableStream.ReadElementAs(ref item, ref guid, context, true);
            }
        }

        public void Deserialize<T, TCtx1, TCtx2>(ref T item, TCtx1 context1, TCtx2 context2, string filepath)
            where T : IPersistable, IContextAware<TCtx1>, IContextAware<TCtx2>
        {
            using (DeserializerStream deserializableStream =
                GetDeserializerStream(filepath))
            {
                Guid guid = Guid.Empty;
                deserializableStream.ReadElementAs(ref item, ref guid, context1, context2, true);
            }
        }

        public ILinkInfo Persist<T>(T item)
            where T : class, IPersistable
        {
            ILinkRef linkRef = _linkRefTable.GetValue(item,
            delegate(IPersistable localitem)
            {
                // NB: Ignore the localitem argument otherwise we loose the type
                // information
                return registerItem(item);
            });
            Persist(item, linkRef);
            return linkRef;
        }

        public T GetItem<T>(ref Guid guid)
            where T : class, IPersistable
        {
            T item;
            GetItem(out item, ref guid);
            return item;
        }

        public ILinkInfo GetItem<T>(out T item, ref Guid guid)
            where T : class, IPersistable
        {
            item = default(T);
            ILinkRef linkRef;
            GetItem(out linkRef, ref item, ref guid);
            return linkRef;
        }

        public IEnumerable<T> GetItems<T>()
            where T : class, IPersistable
        {
            string savepath = _savePaths[typeof(T)];
            if (!Directory.Exists(savepath))
                yield break;

            foreach (string filepath in Directory.GetFiles(savepath, "*." + this.FileExtension))
            {
                string guidstr = Path.GetFileNameWithoutExtension(filepath);
                Guid guid;
                bool success = Guid.TryParse(guidstr, out guid);
                if (!success)
                    continue;

                WeakReference weakReference;
                bool found = _guidRefTable.TryGetValue(guid, out weakReference);
                if (found)
                {
                    object target = weakReference.Target;
                    if (target != null)
                        yield return (T)target;
                    continue;
                }

                // Not found: register and read the element
                T item = null;
                using (DeserializerStream deserializableStream =
                    GetDeserializerStream(filepath))
                {
                    deserializableStream.ReadElementAs(ref item, ref guid, true);
                }

                yield return item;
            };
        }

        public bool Release<T>(T item)
            where T : class, IPersistable
        {
            ILinkRef linkRef;
            bool found = _linkRefTable.TryGetValue(item, out linkRef);
            if (!found)
                return false;

            if (linkRef.LinkingCount != 0)
                throw new Exception("Item still linked");

            string savepath = GetSavePathSafe(item);
            string filename = linkRef.Guid.ToString() + "." + this.FileExtension;
            string filepath = Path.Combine(savepath, filename);

            File.Delete(filepath);

            return _linkRefTable.Remove(item);
        }

        public void Register(Type type, string savepath)
        {
            _savePaths[type] = savepath;
        }

        public void Deregister(Type type)
        {
            _savePaths.Remove(type);
        }

        #endregion // Inquiry

        #region Private

        internal ILinkRef RegisterItem<T>(T item, ref Guid guid)
            where T : IPersistable
        {
            ILinkRef linkRef = registerItem(item, ref guid);
            _linkRefTable.Add(item, linkRef);
            return linkRef;
        }

        internal bool GetLinkRef<T>(T item, out ILinkRef linkRef)
            where T : IPersistable
        {
            bool found = true;
            linkRef = _linkRefTable.GetValue(item,
            delegate(IPersistable localitem)
            {
                found = false;
                // NB: Ignore the localitem argument otherwise we loose the type
                // information
                return registerItem(item);
            });

            if (found)
                return true;

            // If not found, persist it
            Persist(item, linkRef);

            return false;
        }

        internal bool TryGetLinkRef(IPersistable item, out ILinkRef linkRef)
        {
            return _linkRefTable.TryGetValue(item, out linkRef);
        }

        internal void GetItem<T>(out ILinkRef linkRef, ref T item, ref Guid guid)
            where T : IPersistable
        {
            WeakReference weakReference;
            bool found = _guidRefTable.TryGetValue(guid, out weakReference);
            object target = null;
            if (found)
                target = weakReference.Target;

            if (target != null)
            {
                item = (T)target;
                // Invariant: the item must exist
                _linkRefTable.TryGetValue(item, out linkRef);
            }
            else
            {
                // Not found: read and register the element
                string filename = guid.ToString() + "." + this.FileExtension;
                string filepath = Path.Combine(_savePaths[typeof(T)], filename);

                using (DeserializerStream deserializableStream =
                    GetDeserializerStream(filepath))
                {
                    linkRef = deserializableStream.ReadElementAs(ref item,
                        ref guid, true);
                }
            }
        }

        internal void DeregisterGuid(ref Guid guid)
        {
            _guidRefTable.Remove(guid);
        }

        private void Persist<T>(T item, ILinkRef linkRef)
            where T : IPersistable
        {
            string savePath = GetSavePathSafe(item);
            Directory.CreateDirectory(savePath);

            string filename = linkRef.Guid.ToString() + "." + this.FileExtension;
            string filepath = Path.Combine(savePath, filename);

            using (SerializerStream serializableStream =
                GetSerializerStream(filepath))
            {
                serializableStream.WriteElement(PersistableUtils.GetTypeSafe(ref item).Name, ref item,
                    linkRef, true, false);
            }
        }

        private ILinkRef registerItem<T>(T item)
            where T : IPersistable
        {
            Guid guid = Guid.NewGuid();
            WeakReference weakReference = new WeakReference(item);
            ILinkRef ret = new LinkRef<T>(weakReference, ref guid, this);
            _guidRefTable[guid] = weakReference;
            return ret;
        }

        private ILinkRef registerItem<T>(T item, ref Guid guid)
            where T : IPersistable
        {
            WeakReference weakReference = new WeakReference(item);
            ILinkRef ret = new LinkRef<T>(weakReference, ref guid, this);
            _guidRefTable[guid] = weakReference;
            return ret;
        }

        private string GetSavePathSafe<T>(T item)
            where T : IPersistable
        {
            string savePath;
            bool found = _savePaths.TryGetValue(item.GetType(), out savePath);

            if (!found)
                savePath = _savePaths[typeof(T)];

            return savePath;
        }

        protected abstract DeserializerStream GetDeserializerStream(string filepath);

        protected abstract SerializerStream GetSerializerStream(string filepath);

        #endregion // Private

        #region Properties

        /// <summary>
        /// Get the corrispective ILinkInfo
        /// NB: struct can't be registered so it will always return null for structs
        /// </summary>
        public ILinkInfo this[IPersistable item]
        {
            get
            {
                ILinkRef linkRef;
                bool found = _linkRefTable.TryGetValue(item, out linkRef);
                if (!found)
                    return null;

                 return linkRef;
            }
        }

        public abstract string FileExtension
        {
            get;
        }

        #endregion // Properties
    }

    #region Support

    internal class LinkRef<T> : ILinkRef
        where T : IPersistable
    {
        #region Fields

        private WeakReference _WeakRef;
        private Guid _Guid;
        private Persister _persister;
        private HashSet<ILinkRef> _Linked;
        private HashSet<LinkClosure> _Linking;

        #endregion // Fields

        #region Constructor

        public LinkRef(WeakReference weakRef, ref Guid guid, Persister serializer)
        {
            _WeakRef = weakRef;
            _Guid = guid;
            _persister = serializer;
            _Linked = new HashSet<ILinkRef>();
            _Linking = new HashSet<LinkClosure>();
        }

        ~LinkRef()
        {
            _persister.DeregisterGuid(ref _Guid);
        }

        #endregion

        #region Inquiry

        public void ClearLinked()
        {
            foreach (ILinkRef linked in _Linked)
                linked.RemoveLinking(this);

            _Linked.Clear();
        }

        public void Link(ILinkRef linked)
        {
            linked.AddLinking(this);
            _Linked.Add(linked);
        }

        public void AddLinking(ILinkRef linking)
        {
            _Linking.Add(new LinkClosure(linking));
        }

        public void RemoveLinking(ILinkRef linking)
        {
            _Linking.Remove(new LinkClosure(linking));
        }

        #endregion // Inquiry

        #region Properties

        public WeakReference WeakRef
        {
            get { return _WeakRef; }
        }

        public Type RefType
        {
            get { return typeof(T); }
        }

        public Guid Guid
        {
            get { return _Guid; }
        }

        public int LinkingCount
        {
            get { return _Linking.Count; }
        }

        public IEnumerable<LinkClosure> Linking
        {
            get { return _Linking; }
        }

        public int LinkedCount
        {
            get { return _Linked.Count; }
        }

        public IEnumerable<ILinkInfo> Linked
        {
            get { return _Linked; }
        }

        #endregion // Properties
    }

    public interface ILinkInfo
    {
        Guid Guid
        {
            get;
        }
        int LinkingCount
        {
            get;
        }
        IEnumerable<LinkClosure> Linking
        {
            get;
        }
        int LinkedCount
        {
            get;
        }
        IEnumerable<ILinkInfo> Linked
        {
            get;
        }
    }

    internal interface ILinkRef : ILinkInfo
    {
        void ClearLinked();
        void Link(ILinkRef linked);
        void AddLinking(ILinkRef linking);
        void RemoveLinking(ILinkRef linking);
        WeakReference WeakRef
        {
            get;
        }
        Type RefType
        {
            get;
        }
    }

    public struct LinkClosure
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Guid _Guid;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Type _Type;

        internal LinkClosure(ILinkRef linkRef)
        {
            _Guid = linkRef.Guid;
            _Type = linkRef.RefType;
        }

        public Guid Guid
        {
            get { return _Guid; }
        }

        public Type Type
        {
            get { return _Type; }
        }
    }

    internal enum GetMode
    {
        TRY,
        REGISTER,
        REGISTER_PERSIST
    }

    #endregion // Support
}
