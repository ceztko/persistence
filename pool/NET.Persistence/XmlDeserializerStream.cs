using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using System.IO;
using System.Diagnostics;

namespace NET.Persistence
{
    public class XmlDeserializerStream : DeserializerStream
    {
        private XmlReader _xmlReader;
        private FileStream _fileStream;

        internal XmlDeserializerStream(string filepath, Persister serializer)
            : this(new FileStream(filepath, FileMode.Open), serializer) { }

        private XmlDeserializerStream(FileStream fileStream, Persister serializer)
            : this(XmlReader.Create(fileStream), fileStream, serializer) { }

        private XmlDeserializerStream(XmlReader xmlReader, FileStream fileStream,
            Persister serializer)
            : base(serializer)
        {
            _xmlReader = xmlReader;
            _fileStream = fileStream;
            _xmlReader.MoveToContent();
        }

        protected override string ReadAttribute(string name)
        {
            return _xmlReader.GetAttribute(name);
        }

        protected override void ReadNextInternal()
        {
            _xmlReader.Skip();

            if (_xmlReader.NodeType == XmlNodeType.Whitespace)
                _xmlReader.Read();
        }

        protected override void CloseInternal()
        {
            if (_xmlReader != null)
                _xmlReader.Close();
            if (_fileStream != null)
                _fileStream.Close();
        }

        protected override void DescendElement()
        {
            _xmlReader.Read();
            if (_xmlReader.NodeType == XmlNodeType.Text)
                throw new Exception("Text found in sub element");
            if (_xmlReader.NodeType == XmlNodeType.Whitespace)
                _xmlReader.Read();
        }


        protected override void AscendElementInternal()
        {
            _xmlReader.ReadEndElement();
            if (_xmlReader.NodeType == XmlNodeType.Whitespace)
                _xmlReader.Read();
        }

        protected override string ReadElementAsStringInternal()
        {
            string ret;
            if (_xmlReader.IsEmptyElement)
            {
                ret = null;
                _xmlReader.Skip();
            }
            else
                ret = _xmlReader.ReadElementContentAsString();
            if (_xmlReader.NodeType == XmlNodeType.Whitespace)
                _xmlReader.Read();
            return ret;
        }

        protected override string GetNameInternal()
        {
            return _xmlReader.Name;
        }

        protected override bool IsEmptyElementInternal()
        {
            return _xmlReader.IsEmptyElement;
        }

        protected override long GetLengthInternal()
        {
            return _fileStream.Length;
        }

        protected override long GetPositionInternal()
        {
            return _fileStream.Position;
        }

        protected override bool IsEndOfElement()
        {
            return _xmlReader.NodeType == XmlNodeType.EndElement;
        }
    }
}
