#pragma once

#include <string>

#include <gcroot.h>

#include <DeserializerStream.h>

namespace Persistence { namespace Interop 
{
    typedef NET::Persistence::DeserializerStream DeserializerStreamNET;
    
    class DeserializerStreamW : public DeserializerStream
    {
    private:
        gcroot<DeserializerStreamNET ^> _wrapped;
    protected:
        virtual void readElementMetaData();
        virtual std::string getAttribute(Attribute attribute);
        virtual void descendElement();
        virtual void ascendElement();
        virtual void readNext();
    public:
        DeserializerStreamW(DeserializerStreamNET ^wrapped);
        virtual std::string GetName();
        virtual bool IsEmptyElement();
        virtual int64_t GetLength();
        virtual int64_t GetPosition();
        virtual bool IsEndOfElement();
        virtual bool ReadNext();
        virtual void ReadElementAs(std::string &str);
        virtual void ReadElementAs(bool *boolean);
        virtual void ReadElementAs(int32_t *intenger);
        virtual void ReadElementAs(int64_t *intenger);
        virtual void ReadElementAs(uint32_t *intenger);
        virtual void ReadElementAs(uint64_t *intenger);
        virtual void ReadElementAs(float *floating);
        virtual void ReadElementAs(double *floating);
        virtual std::string ReadElementAsString();
        virtual bool ReadElementAsBool();
        virtual int32_t ReadElementAsInt32();
        virtual int64_t ReadElementAsInt64();
        virtual uint32_t ReadElementAsUInt32();
        virtual uint64_t ReadElementAsUInt64();
        virtual float ReadElementAsFloat();
        virtual double ReadElementAsDouble();
    };
} }
