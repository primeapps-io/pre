﻿using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ITeamRepository : IRepositoryBaseConsole
    {
        Task<Team> Get(int id);
        Task<List<Team>> GetByOrganizationId(int organizationId);
        Task<int> Create(Team team);
        Task<int> Delete(Team team);
        Task<int> Update(Team team);
    }
}