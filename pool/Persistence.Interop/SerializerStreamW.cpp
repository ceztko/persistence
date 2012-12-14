#include "SerializerStreamW.h"

using namespace std;
using namespace System;
using namespace System::Text;
using namespace Persistence::Interop;

SerializerStreamW::SerializerStreamW(SerializerStreamNET ^wrapped)
{
    _wrapped = wrapped;
}

void SerializerStreamW::WriteElement(const string &value)
{
    String ^valuenet = gcnew String(value.c_str(), 0, value.length(), Encoding::UTF8);
    _wrapped->WriteElement(valuenet);
}

void SerializerStreamW::WriteElement(bool value)
{
    _wrapped->WriteElement(value);
}

void SerializerStreamW::WriteElement(int32_t value)
{
    _wrapped->WriteElement(value);
}

void SerializerStreamW::WriteElement(int64_t value)
{
    _wrapped->WriteElement(value);
}

void SerializerStreamW::WriteElement(uint32_t value)
{
    _wrapped->WriteElement(value);
}

void SerializerStreamW::WriteElement(uint64_t value)
{
    _wrapped->WriteElement(value);
}

void SerializerStreamW::WriteElement(float value)
{
    _wrapped->WriteElement(value);
}

void SerializerStreamW::WriteElement(double value)
{
    _wrapped->WriteElement(value);
}

void SerializerStreamW::WriteElement(const string &localName, const string &value)
{
    String ^localnet =
        gcnew String(localName.c_str(), 0, localName.length(), Encoding::UTF8);
    String ^valuenet =
        gcnew String(value.c_str(), 0, value.length(), Encoding::UTF8);
    _wrapped->WriteElement(localnet, valuenet);
}

void SerializerStreamW::WriteElement(const string &localName, bool value)
{
    String ^localnet =
        gcnew String(localName.c_str(), 0, localName.length(), Encoding::UTF8);
    _wrapped->WriteElement(localnet, value);
}

void SerializerStreamW::WriteElement(const string &localName, int32_t value)
{
    String ^localnet =
        gcnew String(localName.c_str(), 0, localName.length(), Encoding::UTF8);
    _wrapped->WriteElement(localnet, value);
}

void SerializerStreamW::WriteElement(const string &localName, int64_t value)
{
    String ^localnet =
        gcnew String(localName.c_str(), 0, localName.length(), Encoding::UTF8);
    _wrapped->WriteElement(localnet, value);
}

void SerializerStreamW::WriteElement(const string &localName, uint32_t value)
{
    String ^localnet =
        gcnew String(localName.c_str(), 0, localName.length(), Encoding::UTF8);
    _wrapped->WriteElement(localnet, value);
}

void SerializerStreamW::WriteElement(const string &localName, uint64_t value)
{
    String ^localnet =
        gcnew String(localName.c_str(), 0, localName.length(), Encoding::UTF8);
    _wrapped->WriteElement(localnet, value);
}

void SerializerStreamW::WriteElement(const string &localName, float value)
{
    String ^localnet =
        gcnew String(localName.c_str(), 0, localName.length(), Encoding::UTF8);
    _wrapped->WriteElement(localnet, value);
}

void SerializerStreamW::WriteElement(const string &localName, double value)
{
    String ^localnet =
        gcnew String(localName.c_str(), 0, localName.length(), Encoding::UTF8);
    _wrapped->WriteElement(localnet, value);
}

void SerializerStreamW::writeStartElement(const std::string &localName)
{
    // Writing start element corresponds to opening an element
    String ^localnet =
        gcnew String(localName.c_str(), 0, localName.length(), Encoding::UTF8);
    _wrapped->OpenElement(localnet);
}

void SerializerStreamW::writeEndElement(bool fullEnd)
{
    // Writing end element corresponds to closing the current element, setting its
    // FullEnd flag
    SerializerStreamNET::Element ^current = _wrapped->CurrentElement;
    current->FullEnd = fullEnd;
    current->Close();
}
