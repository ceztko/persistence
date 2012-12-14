using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace NET.Persistence
{
    public abstract class SerializerStream : IDisposable
    {
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

        protected SerializerStream(Persister serializer)
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

        ~SerializerStream()
        {
            Close();
        }

        #endregion // Constructors

        #region Inquiry

        public Element OpenElement(string localName)
        {
            Assert();
            return new Element(this, localName, true);
        }

        public Element OpenElement(string localName, bool fullend)
        {
            Assert();
            return new Element(this, localName, fullend);
        }

        /// <summary>
        /// Write reference
        /// Value types can't be referenced
        /// </summary>
        public void WriteElement<T>(T item, bool link = false)
            where T : class, IPersistableItem
        {
            Assert();

            ILinkRef linkRef = null;
            if (item != null)
            {
                if (link)
                {
                    // Persist item if needed
                    _Persister.GetLinkRef(item, out linkRef);
                }
                else
                {
                    bool found = _Persister.TryGetLinkRef(item, out linkRef);
                    if (found)
                    {
                        // Clear the linked items list before serializing
                        linkRef.ClearLinked();
                    }
                }
            }

            WriteElement(ItemUtils.GetTypeSafe(ref item).Name, ref item, linkRef, false, link);
        }

        public void WriteElement<T>(ref T item)
            where T : IPersistableItem
        {
            Assert();
            WriteElement(ItemUtils.GetTypeSafe(ref item).Name, ref item, null, false, false);
        }

        public void WriteElement(string value)
        {
            Assert();
            WriteElementString("String", value);
        }

        public void WriteElement(bool value)
        {
            Assert();
            WriteElementString("Boolean", value.ToString());
        }

        public void WriteElement(int value)
        {
            Assert();
            WriteElementString("Int32", value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(long value)
        {
            Assert();
            WriteElementString("Int64", value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(uint value)
        {
            Assert();
            WriteElementString("UInt32", value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(ulong value)
        {
            Assert();
            WriteElementString("UInt64", value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(float value)
        {
            Assert();
            WriteElementString("Float", value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(double value)
        {
            Assert();
            WriteElementString("Double", value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(DateTime value)
        {
            Assert();
            WriteElementString("DateTime", value.ToBinary().ToString());
        }

        public void WriteElement(Guid value)
        {
            Assert();
            WriteElementString("Guid", value.ToString());
        }

        /// <summary>
        /// Write reference
        /// Value types can't be referenced
        /// </summary>
        public void WriteElement<T>(string localName, T item, bool link = false)
            where T : class, IPersistableItem
        {
            Assert();

            ILinkRef linkRef = null;
            if (item != null)
            {
                if (link)
                {
                    // Persist item if needed
                    _Persister.GetLinkRef(item, out linkRef);
                }
                else
                {
                    bool found = _Persister.TryGetLinkRef(item, out linkRef);
                    if (found)
                    {
                        // Clear the linked items list before serializing
                        linkRef.ClearLinked();
                    }
                }
            }

            WriteElement(localName, ref item, linkRef, false, link);
        }

        public void WriteElement<T>(string localName, ref T item)
            where T : IPersistableItem
        {
            Assert();
            WriteElement(localName, ref item, null, false, false);
        }

        public void WriteElement(string localName, string value)
        {
            Assert();
            WriteElementString(localName, value);
        }

        public void WriteElement(string localName, bool value)
        {
            Assert();
            WriteElementString(localName, value.ToString());
        }

        public void WriteElement(string localName, int value)
        {
            Assert();
            WriteElementString(localName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(string localName, long value)
        {
            Assert();
            WriteElementString(localName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(string localName, uint value)
        {
            Assert();
            WriteElementString(localName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(string localName, ulong value)
        {
            Assert();
            WriteElementString(localName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(string localName, float value)
        {
            Assert();
            WriteElementString(localName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(string localName, double value)
        {
            Assert();
            WriteElementString(localName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteElement(string name, DateTime value)
        {
            Assert();
            WriteElementString(name, value.ToBinary().ToString());
        }

        public void WriteElement(string name, Guid value)
        {
            Assert();
            WriteElementString(name, value.ToString());
        }

        public void Flush()
        {
            Assert();
            FlushInternal();
        }

        #endregion // Inquiry

        #region Private methods

        private void WriteElementString(string localName, string value)
        {
            WriteStartElement(localName);
            if (value == null)
                WriteEndElement(false);
            else
            {
                WriteRaw(value);
                WriteEndElement(true);
            }
        }

        internal void WriteElement<T>(string localName, ref T item,
            ILinkRef currentLinkRef, bool writeVersion, bool link)
            where T : IPersistableItem
        {
            // NB: typeSafe is item.GetType() if item != null
            Type typeSafe = ItemUtils.GetTypeSafe(ref item);

            IPersistableItem prevParent = _ParentItem;
            IPersistableItem prevCurrent = _currentItem;
            ILinkRef prevParentLinkRef = _parentLinkRef;
            ILinkRef prevCurrentLinkRef = _currentLinkRef;

            _ParentItem = _currentItem;
            // If value type, don't keep track of the current item
            if (!typeSafe.IsValueType)
                _currentItem = item;
            _parentLinkRef = _currentLinkRef;
            _currentLinkRef = currentLinkRef;

            using (Element newelement = new Element(this, localName, true))
            {
                if (item == null)
                {
                    newelement.FullEnd = false;
                    WriteTypeAttribute(typeSafe);
                }
                else
                {
                    WriteTypeAttribute(typeSafe);
                    if (writeVersion)
                        WriteVersionAttribute(typeSafe);

                    if (link)
                    {
                        if (_parentLinkRef != null)
                            _parentLinkRef.Link(currentLinkRef);

                        Guid guid = currentLinkRef.Guid;
                        WriteGuidAttribute(ref guid);
                    }
                    else
                        Hiearchical.Serialize(ref item, this);
                }
            }

            _ParentItem = prevParent;
            _currentItem = prevCurrent;
            _parentLinkRef = prevParentLinkRef;
            _currentLinkRef = prevCurrentLinkRef;
        }

        private void WriteVersionAttribute(Type type)
        {
            string versionAttribute = ItemUtils.ComputeVersion(type);
            if (versionAttribute != null)
                WriteAttribute("Version", versionAttribute);
        }

        private void WriteTypeAttribute(Type type)
        {
            WriteAttribute("Type", type.AssemblyQualifiedName);
        }

        private void WriteGuidAttribute(ref Guid guid)
        {
            WriteAttribute("Guid", guid.ToString());
        }

        private void Assert()
        {
            if (_Disposed)
                throw new ObjectDisposedException("SerializerStream");
        }

        protected internal abstract void WriteStartElement(string localName);

        protected internal abstract void WriteEndElement(bool full);

        protected abstract void WriteRaw(string data);

        protected abstract void CloseInternal();

        protected abstract void WriteAttribute(string name, string value);

        protected abstract void FlushInternal();

        #endregion // Private methods

        #region Properties

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
            private SerializerStream _writer;
            private Element _Previous;
            private Element _Next;
            private bool _FullEnd;

            internal Element(SerializerStream writer, string localName,
                bool fullend)
            {
                _writer = writer;
                _FullEnd = fullend;

                writer.WriteStartElement(localName);

                _Previous = writer.CurrentElement;
                if (_Previous != null)
                    _Previous._Next = this;
                _Next = null;
                writer.CurrentElement = this;

                _Disposed = false;
            }

            ~Element()
            {
                Close();
            }

            public Element Previous
            {
                get { return _Previous; }
            }

            public Element Next
            {
                get { return _Next; }
            }

            public bool FullEnd
            {
                get { return _FullEnd; }
                set
                {
                    if (_Disposed)
                        throw new ObjectDisposedException("Element");

                    _FullEnd = value;
                }
            }

            public void Close()
            {
                if (_Disposed)
                    return;

                if (_Next != null)
                    _Next.Close();
                
                _writer.WriteEndElement(_FullEnd);

                if (_Previous != null)
                    _Previous._Next = null;
                _writer.CurrentElement = _Previous;

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
}
