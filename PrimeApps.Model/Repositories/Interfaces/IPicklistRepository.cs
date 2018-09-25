﻿using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPicklistRepository : IRepositoryBaseTenant
    {
        Task<Picklist> GetById(int id);
        Task<ICollection<Picklist>> GetAll();
        Task<ICollection<Picklist>> Find(List<int> ids);
        Task<PicklistItem> GetItemById(int id);
        Task<PicklistItem> GetItemBySystemCode(string systemCode);
        Task<ICollection<PicklistItem>> GetItemsAll();
        Task<ICollection<PicklistItem>> FindItems(List<int> ids);
        Task<PicklistItem> FindItemByLabel(int picklistId, string label, string language);
        Task<int> Create(Picklist picklist);
        Task<int> Update(Picklist picklist);
        Task<int> DeleteSoft(Picklist picklist);
        Task<int> DeleteHard(Picklist picklist);
    }
}
