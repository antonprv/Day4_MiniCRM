#include "pch.h"

#include <vector>
#include <algorithm>
#include <cwctype>

#include "ClientFilter.h"

static bool ContainsIgnoreCase(const wchar_t* haystack, const wchar_t* needle)
{
    if (!*needle)
        return true;

    for (const wchar_t* h = haystack; *h; ++h)
    {
        const wchar_t* hIt = h;
        const wchar_t* nIt = needle;

        while (*hIt && *nIt &&
            towlower(*hIt) == towlower(*nIt))
        {
            ++hIt;
            ++nIt;
        }

        if (!*nIt)
            return true;
    }

    return false;
}

CLIENTFILTER_API bool FilterClients(
    const ClientRecord* records,
    int count,
    const wchar_t* query,
    int* outIndices,
    int maxOut,
    int* outCount)
{
    // Валидация
    if (!outCount)
        return false;

    *outCount = 0;

    if (!records || count <= 0 || !query)
        return false;

    std::vector<int> results;
    results.reserve(count);

    for (int i = 0; i < count; i++)
    {
        if (ContainsIgnoreCase(records[i].FullName, query) ||
            ContainsIgnoreCase(records[i].Company, query) ||
            ContainsIgnoreCase(records[i].Email, query))
        {
            results.push_back(i);
        }
    }

    *outCount = (int)results.size();

    // Если буфер не передан - это норм, просто запрос размера
    if (!outIndices || maxOut <= 0)
        return true;

    int toCopy = std::min(*outCount, maxOut);

    for (int i = 0; i < toCopy; i++)
    {
        outIndices[i] = results[i];
    }

    return true;
}