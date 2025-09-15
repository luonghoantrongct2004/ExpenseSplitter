﻿using System.Text.Json;

namespace BE.Application.Models;

public class PagedList<T> : List<T>
{
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await Task.Run(() => source.Count());
        var items = await Task.Run(() => source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList());

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }

    public string GetPaginationHeader()
    {
        var metadata = new
        {
            CurrentPage,
            TotalPages,
            PageSize,
            TotalCount,
            HasPrevious,
            HasNext
        };

        return JsonSerializer.Serialize(metadata);
    }
}