using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace NET.Persistence
{
    public class XmlSerializerStream : SerializerStream
    {
        private XmlWriter _xmlWriter;

        internal XmlSerializerStream(string filePath, Persister serializer)
            : base(serializer)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(true);
            settings.Indent = true;
            settings.CloseOutput = true;

            _xmlWriter = XmlWriter.Create(filePath, settings);
        }

        protected override void CloseInternal()
        {
            if (_xmlWriter != null)
                _xmlWriter.Close();
        }

        protected override void FlushInternal()
        {
            _xmlWriter.Flush();
        }

        protected internal override void WriteStartElement(string localName)
        {
            _xmlWriter.WriteStartElement(localName);
        }

        protected internal override void WriteEndElement(bool full)
        {
            if (full)
                _xmlWriter.WriteFullEndElement();
            else
                _xmlWriter.WriteEndElement();
        }

        protected override void WriteRaw(string data)
        {
            _xmlWriter.WriteRaw(data);
        }

        protected override void WriteAttribute(string name, string value)
        {
            _xmlWriter.WriteAttributeString(name, value);
        }
    }
}
