﻿using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPlatformWarehouseRepository : IRepositoryBasePlatform
    {
        Task<PlatformWarehouse> GetByTenantId(int tenantId);
        PlatformWarehouse GetByTenantIdSync(int tenantId);
        PlatformWarehouse Create(PlatformWarehouse warehouse);
        void SetCompleted(PlatformWarehouse warehouse, string userEmail);
    }
}