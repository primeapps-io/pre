using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/relation")]
    public class RelationController : DraftBaseController
    {
        private IRelationRepository _relationRepository;
        private IProfileRepository _profileRepository;
        private ISettingRepository _settingRepository;
        private IModuleRepository _moduleRepository;
        private IConfiguration _configuration;
        private Warehouse _warehouse;

        private IModuleHelper _moduleHelper;

        public RelationController(IRelationRepository relationRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, IModuleRepository moduleRepository, Warehouse warehouse, IModuleHelper moduleHelper, IConfiguration configuration)
        {
            _relationRepository = relationRepository;
            _profileRepository = profileRepository;
            _settingRepository = settingRepository;
            _moduleRepository = moduleRepository;
            _warehouse = warehouse;
            _configuration = configuration;
            _moduleHelper = moduleHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_relationRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_profileRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_moduleRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        [Route("count/{id:int}"), HttpGet]
        public async Task<IActionResult> Count(int id)
        {
            var count = await _relationRepository.Count(id);

            if (count < 1)
                return Ok(null);

            return Ok(count);
        }

        [Route("find/{id:int}")]
        public IActionResult Find(int id, ODataQueryOptions<Relation> queryOptions)
        {
            var relations = _relationRepository.Find(id);
            var queryResults = (IQueryable<Relation>)queryOptions.ApplyTo(relations, new ODataQuerySettings() { EnsureStableOrdering = false });
            return Ok(new PageResult<Relation>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }

        [Route("get_all"), HttpGet]
        public async Task<ICollection<Relation>> GetAll()
        {
            return await _relationRepository.GetAll();
        }

        [Route("get_all_deleted"), HttpGet]
        public async Task<ICollection<Relation>> GetAllDeleted()
        {
            return await _relationRepository.GetAllDeleted();
        }

    }
}