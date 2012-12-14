#include "DeserializerStream.h"

using namespace std;
using namespace Persistence;

DeserializerStream::Element::Element(DeserializerStream *reader)
{
    _reader = reader;

    reader->readElementMetaData();

    _Version = reader->getAttribute(VERSION);

    reader->descendElement();

    _Previous = reader->GetCurrentElement();
    if (_Previous != NULL)
        _Previous->_Next = this;
    _Next = NULL;
    reader->setCurrentElement(this);

    _disposed = false;
}

DeserializerStream::Element::~Element()
{
    Close();
}

const string & DeserializerStream::Element::GetVersion()
{
    return _Version;
}

DeserializerStream::Element * DeserializerStream::Element::GetPrevious()
{
    return _Previous;
}

DeserializerStream::Element * DeserializerStream::Element::GetNext()
{
    return _Next;
}

void DeserializerStream::Element::Close()
{
    if (_disposed)
        return;

    if (_Next != NULL)
        _Next->Close();

    _reader->ascendElement();

    if (_Previous != NULL)
        _Previous->_Next = NULL;
    _reader->setCurrentElement(_Previous);

    _disposed = true;
}

DeserializerStream::DeserializerStream()
{
    _FirstElement = NULL;
    _CurrentElement = NULL;
}

DeserializerStream::~DeserializerStream() { }

bool DeserializerStream::ReadNext()
{
    if (IsEndOfElement())
        return false;

    readNext();

    if (IsEndOfElement())
        return false;

    return true;
}

DeserializerStream::Element DeserializerStream::OpenElement()
{
    return Element(this);
}

DeserializerStream::Element * DeserializerStream::GetFirstElement()
{
    return _FirstElement;
}
DeserializerStream::Element * DeserializerStream::GetCurrentElement()
{
    return _CurrentElement;
}

void DeserializerStream::setCurrentElement(Element *element)
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
