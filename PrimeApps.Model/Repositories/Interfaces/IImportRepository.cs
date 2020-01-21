﻿using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Import;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IImportRepository : IRepositoryBaseTenant
    {
        Task<Import> GetById(int id);
        Task<ICollection<Import>> Find(ImportRequest request);
        Task<int> Count(ImportRequest request);
        Task<int> Create(Import import);
        Task<int> Update(Import import);
        Task<int> DeleteSoft(Import import);
        Task<int> DeleteHard(Import import);
        Task<int> Revert(Import import);

        //For Import Mapping 
        Task<ImportMap> GetImportMappingByName(string name, int moduleId);
        Task<ImportMap> GetImportMappingById(int id);
        Task<ICollection<ImportMap>> GetImportMappingByModuleId(int moduleId);
        Task<int> ImportMappingSave(ImportMap import);
        Task<int> ImportMappingUpdate(ImportMap import);
        Task<int> ImportMappingSoftDelete(ImportMap import);
    }
}