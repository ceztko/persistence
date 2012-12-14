using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;

namespace NET.Persistence
{
    [DebuggerDisplay("Name = {Name}")]
    public abstract class DeserializerStream : IDisposable
    {
        private delegate void ActionRef<T>(ref T item);

        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _Disposed;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IPersistableItem _currentItem;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IPersistableItem _ParentItem;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILinkRef _currentLinkRef;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILinkRef _parentLinkRef;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Element _FirstElement;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Element _CurrentElement;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Persister _Persister;

        #endregion // Fields

        #region Constructors

        protected DeserializerStream(Persister serializer)
        {
            _Disposed = false;
            _currentItem = null;
            _ParentItem = null;
            _currentLinkRef = null;
            _parentLinkRef = null;
            _FirstElement = null;
            _CurrentElement = null;
            _Persister = serializer;
        }

        ~DeserializerStream()
        {
            Close();
        }

        #endregion // Constructors

        #region Inquiry

        public void ReadElementAs<T>(ref T itemref)
            where T : IPersistableItem
        {
            Assert();
            Guid guid = Guid.Empty;
            ReadElementAs(ref itemref, ref guid, false);
        }

        public void ReadElementAs<T, TCtx>(ref T itemref, TCtx context)
            where T : IPersistableItem, IContextAwareItem<TCtx>
        {
            Assert();
            Guid guid = Guid.Empty;
            ReadElementAs(ref itemref, ref guid, context, false);
        }

        public void ReadElementAs<T, TCtx1, TCtx2>(ref T itemref, TCtx1 context1, TCtx2 context2)
            where T : IPersistableItem, IContextAwareItem<TCtx1>, IContextAwareItem<TCtx2>
        {
            Assert();
            Guid guid = Guid.Empty;
            ReadElementAs(ref itemref, ref guid, context1, context2, false);
        }

        public Element OpenElement()
        {
            Assert();
            return new Element(this);
        }

        public bool ReadNext()
        {
            Assert();

            if (IsEndOfElement())
                return false;

            ReadNextInternal();

            if (IsEndOfElement())
                return false;

            return true;
        }

        public void ReadElementAs(out string str)
        {
            Assert();
            str = ReadElementAsStringInternal();
        }

        public void ReadElementAs(out bool boolean)
        {
            Assert();
            boolean = bool.Parse(ReadElementAsStringInternal());
        }

        public void ReadElementAs(out int intenger)
        {
            Assert();
            intenger = int.Parse(ReadElementAsStringInternal(),
                CultureInfo.InvariantCulture);
        }

        public void ReadElementAs(out long intenger)
        {
            Assert();
            intenger = long.Parse(ReadElementAsStringInternal(),
                CultureInfo.InvariantCulture);
        }

        public void ReadElementAs(out uint intenger)
        {
            Assert();
            intenger = uint.Parse(ReadElementAsStringInternal(),
                CultureInfo.InvariantCulture);
        }

        public void ReadElementAs(out ulong intenger)
        {
            Assert();
            intenger = ulong.Parse(ReadElementAsStringInternal(),
                CultureInfo.InvariantCulture);
        }

        public void ReadElementAs(out float floating)
        {
            Assert();
            floating = float.Parse(ReadElementAsStringInternal(),
                CultureInfo.InvariantCulture);
        }

        public void ReadElementAs(out double floating)
        {
            Assert();
            floating = double.Parse(ReadElementAsStringInternal(),
                CultureInfo.InvariantCulture);
        }

        public void ReadElementAs(out DateTime dateTime)
        {
            Assert();
            dateTime = DateTime.FromBinary(long.Parse(ReadElementAsStringInternal(),
                CultureInfo.InvariantCulture));
        }

        public void ReadElementAs(out Guid guid)
        {
            Assert();
            guid = new Guid(ReadElementAsStringInternal());
        }

        public T ReadElementAs<T>()
            where T : IPersistableItem
        {
            Assert();
            T ret = default(T);
            Guid guid = Guid.Empty;
            ReadElementAs(ref ret, ref guid, false);
            return ret;
        }

        public string ReadElementAsString()
        {
            string ret;
            ReadElementAs(out ret);
            return ret;
        }

        public bool ReadElementAsBool()
        {
            bool ret;
            ReadElementAs(out ret);
            return ret;
        }

        public int ReadElementAsInt32()
        {
            int ret;
            ReadElementAs(out ret);
            return ret;
        }

        public long ReadElementAsInt64()
        {
            long ret;
            ReadElementAs(out ret);
            return ret;
        }

        public uint ReadElementAsUInt32()
        {
            uint ret;
            ReadElementAs(out ret);
            return ret;
        }

        public ulong ReadElementAsUInt64()
        {
            ulong ret;
            ReadElementAs(out ret);
            return ret;
        }

        public float ReadElementAsFloat()
        {
            float ret;
            ReadElementAs(out ret);
            return ret;
        }

        public double ReadElementAsDouble()
        {
            double ret;
            ReadElementAs(out ret);
            return ret;
        }

        public DateTime ReadElementAsDateTime()
        {
            DateTime ret;
            ReadElementAs(out ret);
            return ret;
        }

        public Guid ReadElementAsGuid()
        {
            Guid ret;
            ReadElementAs(out ret);
            return ret;
        }

        #endregion // Inquiry

        #region Private methods

        internal ILinkRef ReadElementAs<T>(ref T item, ref Guid guid,
            bool checkVersion)
            where T : IPersistableItem
        {
            return ReadElementAs(ref item, ref guid, checkVersion,
            delegate(ref T reviseditem)
            {
                // No context: do nothing
            });
        }

        internal ILinkRef ReadElementAs<T, TCtx>(ref T item, ref Guid guid,
            TCtx context, bool checkVersion)
            where T : IPersistableItem, IContextAwareItem<TCtx>
        {
            return ReadElementAs(ref item, ref guid, checkVersion,
            delegate(ref T reviseditem)
            {
                reviseditem.Context = context;
            });
        }

        internal ILinkRef ReadElementAs<T, TCtx1, TCtx2>(ref T item, ref Guid guid,
            TCtx1 context1, TCtx2 context2, bool checkVersion)
            where T : IPersistableItem, IContextAwareItem<TCtx1>, IContextAwareItem<TCtx2>
        {
            return ReadElementAs(ref item, ref guid, checkVersion,
            delegate(ref T reviseditem)
            {
                (reviseditem as IContextAwareItem<TCtx1>).Context = context1;
                (reviseditem as IContextAwareItem<TCtx2>).Context = context2;
            });
        }

        /// <returns>The found LinkRef for this item, if needed during
        ///     reading</returns>
        /// <param name="guid">When passed non-zero and the current element is not a
        ///     link, this is the guid of a detached element</param>
        private ILinkRef ReadElementAs<T>(ref T item, ref Guid guid,
            bool checkVersion, ActionRef<T> setContext)
            where T : IPersistableItem
        {
            if (this.EmptyElement)
            {
                item = default(T);
                ReadNextInternal();
                return null;
            }

            // NB: typeSafe is item.GetType() if item != null
            Type typeSafe = ItemUtils.GetTypeSafe(ref item);

            IPersistableItem prevParent = _ParentItem;
            IPersistableItem prevCurrent = _currentItem;
            ILinkRef prevParentLinkRef = _parentLinkRef;
            ILinkRef prevCurrentLinkRef = _currentLinkRef;

            _ParentItem = _currentItem;
            _parentLinkRef = _currentLinkRef;
            _currentLinkRef = null; // Look-up for it only if required

            using (Element childElement = new Element(this))
            {
                if (!typeof(T).IsAssignableFrom(childElement.Type))
                    throw new Exception("Ref type incompatible with read stream type");

                // If non-null, deactivate the object first
                if (item != null)
                    Hiearchical.Revise(ref item, ReviseMode.DEACTIVATE);

                if (checkVersion)
                {
                    string refVersion = ItemUtils.ComputeVersion(childElement.Type);
                    if (refVersion != childElement.Version)
                        throw new ItemVersionMismatchException();
                }

                if (childElement.IsLink)
                {
                    if (typeSafe.IsValueType)
                        throw new Exception("A value type ref can't be a link");

                    guid = childElement.Guid;
                    ILinkRef linkedLinkRef;
                    _Persister.GetItem(out linkedLinkRef, ref item, ref guid);

                    if (_parentLinkRef != null)
                        _parentLinkRef.Link(linkedLinkRef);

                    goto Finally;
                }

                if (item == null || typeSafe != childElement.Type)
                {
                    if (typeSafe.IsValueType)
                        throw new Exception("Can't reinitialize a different value type");

                    T reviseditem = InitializeObject<T>(childElement.Type);
                    setContext(ref reviseditem);
                    Hiearchical.Revise(ref reviseditem, ReviseMode.INIT);
                    _currentItem = reviseditem;
                    if (guid != Guid.Empty)
                        _currentLinkRef = _Persister.RegisterItem(reviseditem, ref guid);

                    Hiearchical.Deserialize(ref reviseditem, this);
                    Hiearchical.Revise(ref reviseditem, ReviseMode.ACTIVATE);

                    item = reviseditem;
                }
                else
                {
                    bool found = _Persister.TryGetLinkRef(item, out _currentLinkRef);
                    if (found)
                    {
                        // Clear the linked items list before deserializing 
                        _currentLinkRef.ClearLinked();
                    }

                    Hiearchical.Revise(ref item, ReviseMode.CLEAR);
                    setContext(ref item);

                    Hiearchical.Deserialize(ref item, this);
                    Hiearchical.Revise(ref item, ReviseMode.ACTIVATE);

                    // If value type, don't keep track of the current item
                    if (!typeSafe.IsValueType)
                        _currentItem = item;
                }
            }

            // Reset states and return current LinkRef for this item
        Finally:
            ILinkRef ret = _currentLinkRef;
            _ParentItem = prevParent;
            _currentItem = prevCurrent;
            _parentLinkRef = prevParentLinkRef;
            _currentLinkRef = prevCurrentLinkRef;
            return ret;
        }

        private void AscendElement()
        {
            while (!IsEndOfElement())
                ReadNextInternal();

            AscendElementInternal();
        }

        private static T InitializeObject<T>(Type type)
            where T : IPersistableItem
        {
            T ret = (T)FormatterServices.GetUninitializedObject(type);

            while (type != typeof(object))
            {
                // Looks for the first non-IPersistableItem type in the hierarchy
                if (typeof(IPersistableItem).IsAssignableFrom(type))
                {
                    type = type.BaseType;
                    continue;
                }

                // Find all the constructors of this non-IPersistableItem type
                ConstructorInfo[] constructors = type.GetConstructors(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                // Get the init argument list from the current reference
                object[] args = null;
                ret.SetInitList(ref args);
                if (args == null)
                    args = new object[0];

                // Bind to the correct constructor with this argument list
                object state;
                MethodBase constructor = Type.DefaultBinder.BindToMethod(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    constructors, ref args, null, CultureInfo.CurrentCulture, null,
                    out state);

                // Invoke the constructor on the initialized reference
                constructor.Invoke(ret, args);

                // Binding may modify arguments order
                if (state != null)
                    Type.DefaultBinder.ReorderArgumentArray(ref args, state);

                break;
            }
            
            return ret;
        }

        private string ReadVersionAttribute()
        {
            return ReadAttribute("Version");
        }

        private Type ReadTypeAttribute()
        {
            string typeName = ReadAttribute("Type");
            if (typeName == null)
                return null;
            else
                return Type.GetType(typeName);
        }

        private bool ReadGuidAttribute(out Guid guid)
        {
            string guidstr = ReadAttribute("Guid");
            if (guidstr == null)
            {
                guid = Guid.Empty;
                return false;
            }
            else
            {
                guid = new Guid(guidstr);
                return true;
            }
        }

        private void Assert()
        {
            if (_Disposed)
                throw new ObjectDisposedException("DeserializerStream");
        }

        protected abstract string ReadAttribute(string name);

        protected abstract void DescendElement();

        protected abstract void AscendElementInternal();

        protected abstract void ReadNextInternal();

        protected abstract string ReadElementAsStringInternal();

        protected abstract string GetNameInternal();

        protected abstract bool IsEndOfElement();

        protected abstract bool IsEmptyElementInternal();

        protected abstract long GetLengthInternal();

        protected abstract long GetPositionInternal();

        protected abstract void CloseInternal();

        #endregion // Private methods

        #region Properties

        public string Name
        {
            get
            {
                Assert();
                return GetNameInternal();
            }
        }

        public Type Type
        {
            get
            {
                Assert();
                return ReadTypeAttribute();
            }
        }

        public bool EmptyElement
        {
            get
            {
                Assert();
                return IsEmptyElementInternal();
            }
        }

        public long Length
        {
            get
            {
                Assert();
                return GetLengthInternal();
            }
        }

        public long Position
        {
            get
            {
                Assert();
                return GetPositionInternal();
            }
        }

        public bool EndOfElement
        {
            get
            {
                Assert();
                return IsEndOfElement();
            }
        }

        public IPersistableItem ParentItem
        {
            get { return _ParentItem; }
        }

        public Element FirstElement
        {
            get { return _FirstElement; }
        }

        public Element CurrentElement
        {
            get { return _CurrentElement; }
            internal set
            {
                _CurrentElement = value;
                if (value == null)
                {
                    _FirstElement = null;
                    return;
                }
                if (_FirstElement == null)
                    _FirstElement = value;
            }
        }

        public Persister Persister
        {
            get { return _Persister; }
        }

        public bool Disposed
        {
            get { return _Disposed; }
        }

        #endregion // Properties

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (_Disposed)
                return;

            if (_FirstElement != null)
                _FirstElement.Close();

            CloseInternal();

            GC.SuppressFinalize(this);

            _Disposed = true;
        }

        #endregion // IDisposable Members

        #region Support

        public class Element : IDisposable
        {
            private bool _Disposed;
            private DeserializerStream _reader;
            private Element _Previous;
            private Element _Next;
            private Type _Type;
            private string _Version;
            private Guid _Guid;
            private bool _IsLink;

            internal Element(DeserializerStream reader)
            {
                _reader = reader;
                _Type = reader.ReadTypeAttribute();
                _Version = reader.ReadVersionAttribute();
                _IsLink = reader.ReadGuidAttribute(out _Guid);
                reader.DescendElement();

                _Previous = reader.CurrentElement;
                if (_Previous != null)
                    _Previous._Next = this;
                _Next = null;
                reader.CurrentElement = this;

                _Disposed = false;
            }

            ~Element()
            {
                Close();
            }

            public Type Type
            {
                get { return _Type; }
            }

            public string Version
            {
                get { return _Version; }
            }

            public Element Previous
            {
                get { return _Previous; }
            }

            public Guid Guid
            {
                get { return _Guid; }
            }

            public bool IsLink
            {
                get { return _IsLink; }
            }

            public Element Next
            {
                get { return _Next; }
            }

            public void Close()
            {
                if (_Disposed)
                    return;

                if (_Next != null)
                    _Next.Close();

                _reader.AscendElement();

                if (_Previous != null)
                    _Previous._Next = null;
                _reader.CurrentElement = _Previous;

                GC.SuppressFinalize(this);

                _Disposed = true;
            }

            #region IDisposable Members

            public void Dispose()
            {
                Close();
            }

            #endregion // IDisposable Members
        }

        #endregion // Support
    }

    #region Support

    public class ItemVersionMismatchException : Exception { }

    #endregion // Support
}
