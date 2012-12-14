#include <atlstr.h>

#include "DeserializerStreamW.h"

using namespace std;
using namespace Persistence::Interop;

string DeserializerStreamW::GetName()
{
    CString netstr(_wrapped->Name);
	CT2A u8str_netstr(netstr, CP_UTF8);
    return string(u8str_netstr);
}

bool DeserializerStreamW::IsEmptyElement()
{
    return _wrapped->EmptyElement;
}

int64_t DeserializerStreamW::GetLength()
{
    return _wrapped->Length;
}

int64_t DeserializerStreamW::GetPosition()
{
    return _wrapped->Position;
}

bool DeserializerStreamW::IsEndOfElement()
{
    return _wrapped->EndOfElement;
}

bool DeserializerStreamW::ReadNext()
{
    return _wrapped->ReadNext();
}

void DeserializerStreamW::ReadElementAs(string &str)
{
    CString netstr(_wrapped->ReadElementAsString());
	CT2A u8str_netstr(netstr, CP_UTF8);
    str.assign(u8str_netstr);
}

void DeserializerStreamW::ReadElementAs(bool *boolean)
{
    _wrapped->ReadElementAs(*boolean);
}

void DeserializerStreamW::ReadElementAs(int32_t *integer)
{
    _wrapped->ReadElementAs(*integer);
}

void DeserializerStreamW::ReadElementAs(int64_t *integer)
{
    _wrapped->ReadElementAs(*integer);
}

void DeserializerStreamW::ReadElementAs(uint32_t *integer)
{
    _wrapped->ReadElementAs(*integer);
}

void DeserializerStreamW::ReadElementAs(uint64_t *integer)
{
    _wrapped->ReadElementAs(*integer);
}

void DeserializerStreamW::ReadElementAs(float *floating)
{
    _wrapped->ReadElementAs(*floating);
}

void DeserializerStreamW::ReadElementAs(double *floating)
{
    _wrapped->ReadElementAs(*floating);
}

string DeserializerStreamW::ReadElementAsString()
{
    CString netstr(_wrapped->ReadElementAsString());
	CT2A u8str_netstr(netstr, CP_UTF8);
    return string(u8str_netstr);
}

bool DeserializerStreamW::ReadElementAsBool()
{
    return _wrapped->ReadElementAsBool();
}

int32_t DeserializerStreamW::ReadElementAsInt32()
{
    return _wrapped->ReadElementAsInt32();
}

int64_t DeserializerStreamW::ReadElementAsInt64()
{
    return _wrapped->ReadElementAsInt64();
}

uint32_t DeserializerStreamW::ReadElementAsUInt32()
{
    return _wrapped->ReadElementAsUInt32();
}

uint64_t DeserializerStreamW::ReadElementAsUInt64()
{
    return _wrapped->ReadElementAsUInt64();
}

float DeserializerStreamW::ReadElementAsFloat()
{
    return _wrapped->ReadElementAsFloat();
}

double DeserializerStreamW::ReadElementAsDouble()
{
    return _wrapped->ReadElementAsDouble();
}

void DeserializerStreamW::readElementMetaData()
{
    // Reading MetaData corresponds to opening an element
    _wrapped->OpenElement();
}

std::string DeserializerStreamW::getAttribute(Attribute attribute)
{
    switch(attribute)
    {
    case VERSION:
    {
        CString netstr(_wrapped->CurrentElement->Version);
        CT2A u8str_netstr(netstr, CP_UTF8);
        return string(u8str_netstr);
    }
    default:
        throw exception("Non existant attribute");
    }
}

void DeserializerStreamW::descendElement()
{
    // Nothing to do here, the element has been already descended
}

void DeserializerStreamW::ascendElement()
{
    // Ascend the element corresponds to closing the current element
    _wrapped->CurrentElement->Close();
}

void DeserializerStreamW::readNext()
{
    // Internal read next corresponds in effect to public ReadNext() of wrapped
    // DeserializedStream
    _wrapped->ReadNext();
}
