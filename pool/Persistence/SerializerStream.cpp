#include "SerializerStream.h"

#include <exception>

using namespace std;
using namespace Persistence;

SerializerStream::Element::Element(SerializerStream *writer,
    const std::string &localName, bool fullEnd)
    : _LocalName(localName)
{
    _writer = writer;
    _FullEnd = fullEnd;

    writer->writeStartElement(localName);

    _Previous = writer->GetCurrentElement();
    if (_Previous != NULL)
        _Previous->_Next = this;
    _Next = NULL;
    writer->setCurrentElement(this);

    _disposed = false;
}

SerializerStream::Element::~Element()
{
    Close();
}

SerializerStream::Element * SerializerStream::Element::GetNext()
{
    return _Next;
}

SerializerStream::Element * SerializerStream::Element::GetPrevious()
{
    return _Previous;
}

bool SerializerStream::Element::IsFullEnd()
{
    return _FullEnd;
}

void SerializerStream::Element::SetFullEnd(bool fullEnd)
{
    if (_disposed)
        throw exception("The element has alrady been closed");

    _FullEnd = fullEnd;
}

const string & SerializerStream::Element::GetLocalName()
{
    return _LocalName;
}

void SerializerStream::Element::Close()
{
    if (_disposed)
        return;

    if (_Next != NULL)
        _Next->Close();

    _writer->writeEndElement(_FullEnd);

    if (_Previous != NULL)
        _Previous->_Next = NULL;
    _writer->setCurrentElement(_Previous);

    _disposed = true;
}

SerializerStream::SerializerStream()
{
    _FirstElement = NULL;
    _CurrentElement = NULL;
}

SerializerStream::~SerializerStream() { }

SerializerStream::Element SerializerStream::OpenElement(const std::string &localName,
    bool fullEnd)
{
    return Element(this, localName, fullEnd);
}

SerializerStream::Element * SerializerStream::GetFirstElement()
{
    return _FirstElement;
}

SerializerStream::Element * SerializerStream::GetCurrentElement()
{
    return _CurrentElement;
}

void SerializerStream::setCurrentElement(Element *element)
{
    _CurrentElement = element;
    if (element == NULL)
    {
        _FirstElement = NULL;
        return;
    }
    if (_FirstElement == NULL)
        _FirstElement = element;
}
