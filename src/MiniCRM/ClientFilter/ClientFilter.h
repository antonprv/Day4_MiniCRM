#pragma once

#ifdef CLIENTFILTER_EXPORTS
#define CLIENTFILTER_API __declspec(dllexport)
#else
#define CLIENTFILTER_API __declspec(dllimport)
#endif

#pragma pack(push, 1)
struct ClientRecord
{
    int   Id;
    wchar_t FullName[100];
    wchar_t Company[100];
    wchar_t Email[100];
    int   Status;
};
#pragma pack(pop)

extern "C"
{
    // Возвращает true если всё ок
    // outCount - сколько найдено всего
    // outIndices - буфер (может быть nullptr для запроса размера)
    // maxOut - размер буфера
    CLIENTFILTER_API bool FilterClients(
        const ClientRecord* records,
        int count,
        const wchar_t* query,
        int* outIndices,
        int maxOut,
        int* outCount
    );
}