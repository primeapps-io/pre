﻿using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common;
using System.Linq;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IFunctionRepository : IRepositoryBaseTenant
    {
        Task<int> Count();
        Task<bool> IsFunctionNameAvailable(string name);
        IQueryable<Function> Find();
        Task<Function> Get(int id);
        Task<Function> Get(string name);
        Task<int> Create(Function component);
        Task<int> Update(Function organization);
        Task<int> Delete(Function organization);
    }
}