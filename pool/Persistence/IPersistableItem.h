#pragma once

#include <string>
#include <typeinfo>

#include "defines.h"
#include "DeserializerStream.h"
#include "SerializerStream.h"

namespace Persistence
{
    enum PERSISTENCE_API ReviseMode
    {
        // For compatibility with the .NET counterpart that has an INIT=0 ReviseMode,
        // we start enum with ACTIVATE=1
        ACTIVATE = 1,
        DEACTIVATE,
        CLEAR
    };

    class PERSISTENCE_API IPersistableItem
    {
    public:
        ~IPersistableItem();
        virtual void Serialize(SerializerStream &writer) = 0;
        virtual void Deserialize(DeserializerStream &reader) = 0;
        virtual void Revise(ReviseMode mode) = 0;
    };

    class PERSISTENCE_API IDefaultableItem
    {
        ~IDefaultableItem();
    };

    template <typename T>
    class IContextAwareItem
    {
    public:
        ~IContextAwareItem();
        virtual void SetContext(T context) = 0;
    };

    template <typename T>
    IContextAwareItem<T>::~IContextAwareItem() { }

    template <typename ItemT>
    std::type_info GetTypeSafe(ItemT *item)
    {
        if (item == NULL)
            return typeid(ItemT);
        else
            return typeid(*item);
    }
}
