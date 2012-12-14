#pragma once

#ifdef PERSISTENCE_EXPORTS
#define PERSISTENCE_API __declspec(dllexport)
#else
#define PERSISTENCE_API __declspec(dllimport)
#endif
