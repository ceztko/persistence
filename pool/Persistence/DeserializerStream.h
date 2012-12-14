#pragma once

#pragma once

#include <memory>
#include <string>
#include <stdint.h>

#include "defines.h"

namespace Persistence
{
    class PERSISTENCE_API DeserializerStream
    {
    public:
        class PERSISTENCE_API Element
        {
            friend class DeserializerStream;
        private:
            bool _disposed;
            DeserializerStream *_reader;
            Element *_Previous;
            Element *_Next;
            std::string _Version;
            Element(DeserializerStream *reader);
        public:
            ~Element();
            const std::string & GetVersion();
            Element * GetPrevious();
            Element * GetNext();
            void Close();
        };
        friend class Element;
    private:
        Element *_FirstElement;
        Element *_CurrentElement;
        void setCurrentElement(Element *currentElement);
    protected:
        enum Attribute
        {
            VERSION = 0,
        };
        DeserializerStream();
        virtual void readElementMetaData() = 0;
        virtual std::string getAttribute(Attribute attribute) = 0;
        virtual void descendElement() = 0;
        virtual void ascendElement() = 0;
        virtual void readNext() = 0;
    public:
        ~DeserializerStream();
        Element * GetFirstElement();
        Element * GetCurrentElement();
        Element OpenElement();
        bool ReadNext();

        template <typename ItemT>
        void ReadElementAs(ItemT **itemref);

        template <typename ItemT, typename ContextT>
        void ReadElementAs(ItemT **itemref, ContextT context);

        virtual std::string GetName() = 0;
        virtual bool IsEmptyElement() = 0;
        virtual int64_t GetLength() = 0;
        virtual int64_t GetPosition() = 0;
        virtual bool IsEndOfElement() = 0;
        virtual void ReadElementAs(std::string &str) = 0;
        virtual void ReadElementAs(bool *boolean) = 0;
        virtual void ReadElementAs(int32_t *intenger) = 0;
        virtual void ReadElementAs(int64_t *intenger) = 0;
        virtual void ReadElementAs(uint32_t *intenger) = 0;
        virtual void ReadElementAs(uint64_t *intenger) = 0;
        virtual void ReadElementAs(float *floating) = 0;
        virtual void ReadElementAs(double *floating) = 0;
        virtual std::string ReadElementAsString() = 0;
        virtual bool ReadElementAsBool() = 0;
        virtual int32_t ReadElementAsInt32() = 0;
        virtual int64_t ReadElementAsInt64() = 0;
        virtual uint32_t ReadElementAsUInt32() = 0;
        virtual uint64_t ReadElementAsUInt64() = 0;
        virtual float ReadElementAsFloat() = 0;
        virtual double ReadElementAsDouble() = 0;
    };

    template <typename ItemT>
    void DeserializerStream::ReadElementAs(ItemT **itemref)
    {
        if (IsEmptyElement())
        {
            *itemref = NULL;
            readNext();
            return;
        }

        Element childElement(this);

         // If non-null, deactivate the object first
        if (itemref != null)
            itemref->Revise(DEACTIVATE);

        // Will reinitialize if type of non-null item is different than the
        // stream readed type
        ItemT *reviseditem;
        if (*itemref == null)
        {
            throw exception("Unsupported");
        }
        else
        {
            reviseditem = *itemref;
            reviseditem->Revise(CLEAR);
        }

        reviseditem->Deserialize(this);

        reviseditem->Revise(ACTIVATE);

        *itemref = reviseditem;
    }

    template <typename ItemT, typename ContextT>
    void DeserializerStream::ReadElementAs(ItemT **itemref, ContextT context)
    {
        if (IsEmptyElement())
        {
            *itemref = NULL;
            readNext();
            return;
        }

        Element childElement(this);

         // If non-null, deactivate the object first
        if (itemref != null)
            itemref->Revise(DEACTIVATE);

        // Will reinitialize if type of non-null item is different than the
        // stream readed type
        ItemT *reviseditem;
        if (*itemref == null)
        {
            throw exception("Unsupported");
        }
        else
        {
            reviseditem = *itemref;
            reviseditem->SetContext(context);
            reviseditem->Revise(CLEAR);
        }

        reviseditem->Deserialize(this);

        reviseditem->Revise(ACTIVATE);

        *itemref = reviseditem;
    }
}
