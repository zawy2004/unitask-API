using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Common;

public class PagedResult<T>
{
    public int Total { get; set; }

    public int Page { get; set; }

    public int Limit { get; set; }

    public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();
}
