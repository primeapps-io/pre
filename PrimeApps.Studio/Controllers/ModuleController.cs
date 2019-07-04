using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Constants;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/module")]
    public class ModuleController : DraftBaseController
    {
        private IModuleRepository _moduleRepository;
        private IViewRepository _viewRepository;
        private IProfileRepository _profileRepository;
        private ISettingRepository _settingRepository;
        private IMenuRepository _menuRepository;
        private IConfiguration _configuration;
        private Warehouse _warehouse;

        private IModuleHelper _moduleHelper;
        private IPermissionHelper _permissionHelper;

        public ModuleController(IModuleRepository moduleRepository, IViewRepository viewRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, Warehouse warehouse, IMenuRepository menuRepository, IModuleHelper moduleHelper, IConfiguration configuration, IPermissionHelper permissionHelper)
        {
            _moduleRepository = moduleRepository;
            _viewRepository = viewRepository;
            _profileRepository = profileRepository;
            _settingRepository = settingRepository;
            _warehouse = warehouse;
            _configuration = configuration;
            _menuRepository = menuRepository;
            _permissionHelper = permissionHelper;

            _moduleHelper = moduleHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_moduleRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_viewRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_menuRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_profileRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.View))
                return StatusCode(403);

            var count = await _moduleRepository.Count();

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.View))
                return StatusCode(403);

            var modules = await _moduleRepository.Find(paginationModel);

            return Ok(modules);
        }

        [Route("get_by_id/{id:int}"), HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.View))
                return StatusCode(403);

            var module = await _moduleRepository.GetById(id);

            return Ok(module);
        }

        [Route("get_by_name/{name:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpGet]
        public async Task<IActionResult> GetByName(string name)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.View))
                return StatusCode(403);

            var module = await _moduleRepository.GetByName(name);

            return Ok(module);
        }

        [Route("get_all"), HttpGet]
        public async Task<ICollection<Module>> GetAll()
        {
            return await _moduleRepository.GetAll();
        }

        [Route("get_all_deleted"), HttpGet]
        public async Task<ICollection<Module>> GetAllDeleted()
        {
            return await _moduleRepository.GetAllDeleted();
        }

        [Route("get_all_basic"), HttpGet]
        public async Task<ICollection<Module>> GetAllBasic()
        {
            return await _moduleRepository.GetAllBasic();
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ModuleBindingModel module)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Create module
            var moduleEntity = _moduleHelper.CreateEntity(module);
            var resultCreate = await _moduleRepository.Create(moduleEntity);

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            //Create default views
            try
            {
                var defaultViewAllRecordsEntity = await ViewHelper.CreateDefaultViewAllRecords(moduleEntity, _moduleRepository, AppUser.TenantLanguage);
                //var defaultViewMyRecordsEntity = ViewHelper.CreateDefaultViewMyRecords(moduleEntity);

                var resultCreateViewAllRecords = await _viewRepository.Create(defaultViewAllRecordsEntity);

                if (resultCreateViewAllRecords < 1)
                {
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }

                //var resultCreateViewMyRecords = await _viewRepository.Create(defaultViewMyRecordsEntity);

                //if (resultCreateViewMyRecords < 1)
                //{
                //    await _moduleRepository.DeleteHard(moduleEntity);
                //    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                //}
            }
            catch (Exception)
            {
                await _moduleRepository.DeleteHard(moduleEntity);
                throw;
            }

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            //Create dynamic table
            try
            {
                moduleEntity.Name = char.IsNumber(moduleEntity.Name[0]) ? "n" + moduleEntity.Name : moduleEntity.Name;
                var resultCreateTable = await _moduleRepository.CreateTable(moduleEntity, AppUser.TenantLanguage);

                if (resultCreateTable != -1)
                {
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }
            }
            catch (Exception)
            {
                await _moduleRepository.DeleteHard(moduleEntity);
                throw;
            }

            //Create dynamic table indexes
            try
            {
                var resultCreateIndexes = await _moduleRepository.CreateIndexes(moduleEntity);

                if (resultCreateIndexes != -1)
                {
                    await _moduleRepository.DeleteTable(moduleEntity);
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }

            }
            catch (Exception)
            {
                await _moduleRepository.DeleteTable(moduleEntity);
                await _moduleRepository.DeleteHard(moduleEntity);
                throw;
            }

            //Create default permissions for the new module.
            await _profileRepository.AddModuleAsync(moduleEntity.Id);
            await _menuRepository.AddModuleToMenuAsync(moduleEntity);

            _moduleHelper.AfterCreate(AppUser, moduleEntity);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ModuleBindingModel module)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetById(id);

            if (moduleEntity == null)
                return NotFound();

            //Update module
            var moduleChanges = _moduleHelper.UpdateEntity(module, moduleEntity);
            await _moduleRepository.Update(moduleEntity);

            //If there is no changes for dynamic tables then return ok
            if (moduleChanges == null)
                return Ok();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            //Alter dynamic table
            try
            {
                var resultAlterTable = await _moduleRepository.AlterTable(moduleEntity, moduleChanges, AppUser.TenantLanguage);

                if (resultAlterTable != -1)
                {
                    var entityRevert = _moduleHelper.RevertEntity(moduleChanges, moduleEntity);
                    await _moduleRepository.Update(entityRevert);

                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }
            }
            catch
            {
                var entityRevert = _moduleHelper.RevertEntity(moduleChanges, moduleEntity);
                await _moduleRepository.Update(entityRevert);

                throw;
            }

            //Delete View Fields
            var views = await _viewRepository.GetAll(id);
            var fields = _moduleHelper.DeleteViewField(views, id, module.Fields);
            if (fields.Count > 0)
            {
                foreach (var field in fields)
                {
                    await _viewRepository.DeleteViewField(field.ViewId, field.Field);
                }
            }

            //var viewStates = await _viewRepository.GetAllViewStates(id);
            //if (viewStates.Count > 0)
            //{
            //    foreach (var viewState in viewStates)
            //    {
            //        await _viewRepository.DeleteHardViewState(viewState);
            //    }
            //}

            _moduleHelper.AfterUpdate(AppUser, moduleEntity);

            return Ok();
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Delete))
                return StatusCode(403);

            var moduleEntity = await _moduleRepository.GetById(id);

            if (moduleEntity == null)
                return NotFound();

            var result = await _moduleRepository.DeleteSoft(moduleEntity);

            if (result > 0)
            {
                _moduleHelper.AfterDelete(AppUser, moduleEntity);
                await _menuRepository.DeleteModuleFromMenu(id);
            }

            return Ok();
        }

        [Route("create_relation/{moduleId:int}"), HttpPost]
        public async Task<IActionResult> CreateRelation(int moduleId, [FromBody]RelationBindingModel relation)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetByIdFullModule(moduleId);

            if (moduleEntity == null)
                return NotFound();

            var currentRelations = moduleEntity.Relations.ToList();
            var relationEntity = _moduleHelper.CreateRelationEntity(relation, moduleEntity);
            var resultCreate = await _moduleRepository.CreateRelation(relationEntity);

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            if (relationEntity.RelationType == RelationType.ManyToMany && !relation.TwoWay)
            {
                //Create dynamic junction table
                try
                {
                    var resultCreateJunctionTable = await _moduleRepository.CreateJunctionTable(moduleEntity, relationEntity, currentRelations);

                    if (resultCreateJunctionTable != -1)
                    {
                        await _moduleRepository.DeleteRelationHard(relationEntity);

                        throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

                        //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                    }
                }
                catch (Exception)
                {
                    await _moduleRepository.DeleteRelationHard(relationEntity);
                    throw;
                }
            }

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
        }

        [Route("update_relation/{moduleId:int}/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateRelation(int moduleId, int id, [FromBody]RelationBindingModel relation)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetByIdFullModule(moduleId);

            if (moduleEntity == null)
                return NotFound();

            var relationEntity = await _moduleRepository.GetRelation(id);

            if (relationEntity == null)
                return NotFound();

            _moduleHelper.UpdateRelationEntity(relation, relationEntity, moduleEntity);
            await _moduleRepository.UpdateRelation(relationEntity);

            return Ok();
        }

        [Route("delete_relation/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteRelation(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Delete))
                return StatusCode(403);

            var relationEntity = await _moduleRepository.GetRelation(id);

            if (relationEntity == null)
                return NotFound();

            await _moduleRepository.DeleteRelationSoft(relationEntity);

            return Ok();
        }

        [Route("create_dependency/{moduleId:int}"), HttpPost]
        public async Task<IActionResult> CreateDependency(int moduleId, [FromBody]DependencyBindingModel dependency)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetByIdFullModule(moduleId);

            if (moduleEntity == null)
                return NotFound();

            var dependencyEntity = _moduleHelper.CreateDependencyEntity(dependency, moduleEntity);
            var resultCreate = await _moduleRepository.CreateDependency(dependencyEntity);

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
        }

        [Route("update_dependency/{moduleId:int}/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateDependency(int moduleId, int id, [FromBody]DependencyBindingModel dependency)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetByIdFullModule(moduleId);

            if (moduleEntity == null)
                return NotFound();

            var dependencyEntity = await _moduleRepository.GetDependency(id);

            if (dependencyEntity == null)
                return NotFound();

            _moduleHelper.UpdateDependencyEntity(dependency, dependencyEntity, moduleEntity);
            await _moduleRepository.UpdateDependency(dependencyEntity);

            return Ok();
        }

        [Route("update_field/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateField(int id, [FromBody]FieldBindingModel field)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Update))
                return StatusCode(403);

            var fieldEntity = await _moduleRepository.GetField(id);

            if (fieldEntity == null)
                return NotFound();

            fieldEntity.InlineEdit = field.InlineEdit;

            await _moduleRepository.UpdateField(fieldEntity);

            return Ok();
        }
        [Route("delete_dependency/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteDependency(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.Delete))
                return StatusCode(403);

            var dependencyEntity = await _moduleRepository.GetDependency(id);

            if (dependencyEntity == null)
                return NotFound();

            await _moduleRepository.DeleteDependencySoft(dependencyEntity);

            return Ok();
        }

        [Route("get_module_settings"), HttpGet]
        public async Task<IActionResult> GetModuleSettings()
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.View))
                return StatusCode(403);

            var moduleSettings = await _settingRepository.GetAsync(SettingType.Module);

            return Ok(moduleSettings);
        }

        [Route("get_module_fields"), HttpGet]
        public async Task<IActionResult> GetModuleFieldByName([FromUri]string moduleName)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "module", RequestTypeEnum.View))
                return StatusCode(403);

            var fields = await _moduleRepository.GetModuleFieldByName(moduleName);

            return Ok(fields);
        }
    }
}