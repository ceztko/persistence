#pragma once

#include <string>

#include <gcroot.h>

#include <SerializerStream.h>

namespace Persistence { namespace Interop
{
    typedef NET::Persistence::SerializerStream SerializerStreamNET;

    class SerializerStreamW : public SerializerStream
    {
    private:
        gcroot<SerializerStreamNET ^> _wrapped;
    protected:
        virtual void writeStartElement(const std::string &localName);
        virtual void writeEndElement(bool fullEnd);
    public:
        SerializerStreamW(SerializerStreamNET ^wrapped);
        virtual void WriteElement(const std::string &value);
        virtual void WriteElement(bool value);
        virtual void WriteElement(int32_t value);
        virtual void WriteElement(int64_t value);
        virtual void WriteElement(uint32_t value);
        virtual void WriteElement(uint64_t value);
        virtual void WriteElement(float value);
        virtual void WriteElement(double value);
        virtual void WriteElement(const std::string &localName, const std::string &value);
        virtual void WriteElement(const std::string &localName, bool value);
        virtual void WriteElement(const std::string &localName, int32_t value);
        virtual void WriteElement(const std::string &localName, int64_t value);
        virtual void WriteElement(const std::string &localName, uint32_t value);
        virtual void WriteElement(const std::string &localName, uint64_t value);
        virtual void WriteElement(const std::string &localName, float value);
        virtual void WriteElement(const std::string &localName, double value);
    };
} }
