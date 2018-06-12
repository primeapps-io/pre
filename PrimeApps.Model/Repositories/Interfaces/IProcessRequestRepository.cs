﻿using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IProcessRequestRepository : IRepositoryBaseTenant
    {
        Task<ICollection<ProcessRequest>> GetByProcessId(int id);
        Task<ProcessRequest> GetByIdBasic(int id);
        Task<ProcessRequest> GetByRecordId(int id, string moduleName, OperationType operationType);
        Task<int> Update(ProcessRequest request);
    }
}
