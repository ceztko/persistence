#pragma once

#include <memory>
#include <string>
#include <stdint.h>

#include "defines.h"

namespace Persistence
{
    class PERSISTENCE_API SerializerStream
    {
    public:
        class PERSISTENCE_API Element
        {
            friend class SerializerStream;
        private:
            bool _disposed;
            SerializerStream *_writer;
            Element *_Previous;
            Element *_Next;
            bool _FullEnd;
            std::string _LocalName;
            Element(SerializerStream *writer, const std::string &localName,
                bool fullEnd);
        public:
            ~Element();
            Element * GetPrevious();
            Element * GetNext();
            bool IsFullEnd();
            void SetFullEnd(bool fullEnd);
            const std::string & GetLocalName();
            void Close();
        };
    private:
        Element *_FirstElement;
        Element *_CurrentElement;
        void setCurrentElement(Element *currentElement);
    protected:
        SerializerStream();
        virtual void writeStartElement(const std::string &localName) = 0;
        virtual void writeEndElement(bool fullEnd) = 0;
    public:
        ~SerializerStream();
        Element * GetFirstElement();
        Element * GetCurrentElement();
        Element OpenElement(const std::string &localName, bool fullend = true);

        template <typename ItemT>
        void WriteElement(ItemT *item);

        template <typename ItemT>
        void WriteElement(const std::string &localName, ItemT *item);

        virtual void WriteElement(const std::string &value) = 0;
        virtual void WriteElement(bool value) = 0;
        virtual void WriteElement(int32_t value) = 0;
        virtual void WriteElement(int64_t value) = 0;
        virtual void WriteElement(uint32_t value) = 0;
        virtual void WriteElement(uint64_t value) = 0;
        virtual void WriteElement(float value) = 0;
        virtual void WriteElement(double value) = 0;
        virtual void WriteElement(const std::string &localName, const std::string &value) = 0;
        virtual void WriteElement(const std::string &localName, bool value) = 0;
        virtual void WriteElement(const std::string &localName, int32_t value) = 0;
        virtual void WriteElement(const std::string &localName, int64_t value) = 0;
        virtual void WriteElement(const std::string &localName, uint32_t value) = 0;
        virtual void WriteElement(const std::string &localName, uint64_t value) = 0;
        virtual void WriteElement(const std::string &localName, float value) = 0;
        virtual void WriteElement(const std::string &localName, double value) = 0;
        virtual void Flush() = 0;
    };

    template <typename ItemT>
    void SerializerStream::WriteElement(ItemT *item)
    {
        WriteElement(GetTypeSafe(item).name(), item);
    }

    template <typename ItemT>
    void SerializerStream::WriteElement(const std::string &localName, ItemT *item)
    {
        Element newelement(this, localName, item);

        if (item == null)
        {
            newelement.SetFullEnd(false);
        }
        else
        {
            item->Serialize(this);
        }
    }
}
