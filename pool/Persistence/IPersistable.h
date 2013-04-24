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

	template <typename T>
    class PERSISTENCE_API IPersistable
    {
    public:
        void Serialize(SerializerStream &writer);
        void Deserialize(DeserializerStream &reader);
        void Revise(ReviseMode mode);
    };

	template <typename T>
    class PERSISTENCE_API IDefaultable
    {
	public:
        void Default();
    };

    template <typename T, typename TContext>
    class PERSISTENCE_API IContextAware
    {
    public:
        void SetContext(TContext context);
    };

	#pragma region Implementation

    template <typename T>
    void IPersistable<T>::Serialize(SerializerStream &writer)
	{
		static_cast<T*>(this)->Serialize(writer);
	}

    template <typename T>
    void IPersistable<T>::Deserialize(DeserializerStream &reader)
	{
		static_cast<T*>(this)->Deserialize(reader);
	}

    template <typename T>
    void IPersistable<T>::Revise(ReviseMode mode)
	{
		static_cast<T*>(this)->Revise(mode);
	}

    template <typename T>
    void IDefaultable<T>::Default()
	{
		static_cast<T*>(this)->Default();
	}

    template <typename T, typename TContext>
    void IContextAware<T, TContext>::SetContext(TContext context)
	{
		static_cast<T*>(this)->SetContext(context);
	}

    template <typename ItemT>
    std::type_info GetTypeSafe(ItemT *item)
    {
        if (item == NULL)
            return typeid(ItemT);
        else
            return typeid(*item);
    }
	#pragma endregion

}
