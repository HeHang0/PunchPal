﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace PunchPal.Core.Services
{
    public interface IService<T>
    {
        Task<bool> Add(T entity);
        Task<int> Add(IEnumerable<T> entities);
        Task<bool> Remove(T entity);
    }
}
