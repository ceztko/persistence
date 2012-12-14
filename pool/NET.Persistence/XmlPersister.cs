using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Persistence
{
    public class XmlPersister : Persister
    {
        protected override DeserializerStream GetDeserializerStream(string filepath)
        {
            return new XmlDeserializerStream(filepath, this);
        }

        protected override SerializerStream GetSerializerStream(string filepath)
        {
            return new XmlSerializerStream(filepath, this);
        }

        public override string FileExtension
        {
            get { return "xml"; }
        }
    }
}
