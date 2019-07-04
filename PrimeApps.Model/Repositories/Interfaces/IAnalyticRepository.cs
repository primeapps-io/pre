﻿using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IAnalyticRepository : IRepositoryBaseTenant
    {
        Task<Analytic> GetById(int id);
        Task<ICollection<Analytic>> GetAll();
        Task<ICollection<Analytic>> GetReports(bool hasAdminProfile);
        Task<int> Create(Analytic analytic);
        Task<int> Update(Analytic analytic);
        Task<int> DeleteSoft(Analytic analytic);
        Task<int> DeleteHard(Analytic analytic);
        Task<int> DeleteAnalyticShare(AnalyticShares analytic, TenantUser user);
    }
}