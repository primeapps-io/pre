using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.AuditLog;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Common.Import;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Helpers.QueryTranslation;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.App.Controllers
{
    [Route("api/data"), Authorize]
    public class DataController : ApiBaseController
    {
        private IAuditLogRepository _auditLogRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private IImportRepository _importRepository;
        private ITenantRepository _tenantRepository;
        private Warehouse _warehouse;
        private IConfiguration _configuration;
        private ICalculationHelper _calculationHelper;

        private IDocumentHelper _documentHelper;
        private IRecordHelper _recordHelper;
        public DataController(IAuditLogRepository auditLogRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IImportRepository importRepository, ITenantRepository tenantRepository, IRecordHelper recordHelper, Warehouse warehouse, IConfiguration configuration, IDocumentHelper documentHelper, ICalculationHelper calculationHelper)
        {
            _auditLogRepository = auditLogRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _importRepository = importRepository;
            _tenantRepository = tenantRepository;
            _warehouse = warehouse;
            _configuration = configuration;

            _documentHelper = documentHelper;
            _recordHelper = recordHelper;
            _calculationHelper = calculationHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_auditLogRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_importRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("import/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpPost]
        public async Task<IActionResult> Import(string module, [FromBody]JArray records)
        {
            var moduleEntity = await _moduleRepository.GetByName(module);

            if (moduleEntity == null || records.IsNullOrEmpty() || records.Count < 1)
                return BadRequest();

            var importEntity = new Import
            {
                ModuleId = moduleEntity.Id,
                TotalCount = records.Count
            };

            var result = await _importRepository.Create(importEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            foreach (JObject record in records)
            {
                record["import_id"] = importEntity.Id;
            }

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            int resultCreate;

            try
            {
                resultCreate = await _recordRepository.CreateBulk(records, moduleEntity);
            }
            catch (PostgresException ex)
            {
                await _importRepository.DeleteHard(importEntity);

                if (ex.SqlState == PostgreSqlStateCodes.UniqueViolation)
                    return StatusCode(HttpStatusCode.Status409Conflict, _recordHelper.PrepareConflictError(ex));

                if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.Detail });

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                throw;
            }

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            return Ok(importEntity);
        }

        [Route("import_save_excel"), HttpPost]
        public async Task<IActionResult> ImportSaveExcel([FromQuery(Name = "import_id")]int importId)
        {
            var import = await _importRepository.GetById(importId);

            if (import == null)
                return NotFound();

            var stream = await Request.ReadAsStreamAsync();
            DocumentUploadResult result;
            var isUploaded = _documentHelper.Upload(stream, out result);

            if (!isUploaded)
                return BadRequest();

            var excelUrl = await _documentHelper.Save(result, "import-" + AppUser.TenantId);

            import.ExcelUrl = excelUrl;
            await _importRepository.Update(import);

            return Ok(excelUrl);
        }

        [Route("import_find"), HttpPost]
        public async Task<ICollection<Import>> ImportFind([FromBody]ImportRequest request)
        {
            return await _importRepository.Find(request);
        }

        [Route("import_revert/{importId:int}"), HttpDelete]
        public async Task<IActionResult> RevertImport(int importId)
        {
            var import = await _importRepository.GetById(importId);

            if (import == null)
                return NotFound();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            await _importRepository.Revert(import);
            await _importRepository.DeleteSoft(import);

            return Ok();
        }

        [Route("remove_sample_data"), HttpDelete]
        public async Task<IActionResult> RemoveSampleData()
        {
            var result = await _recordRepository.DeleteSampleData((List<Module>)await _moduleRepository.GetAll());

            //if (result > 0)
            //{
            //    var instanceToUpdate = await _tenantRepository.GetAsync(AppUser.TenantId);
            //    instanceToUpdate.HasSampleData = false;
            //   await _tenantRepository.UpdateAsync(instanceToUpdate);
            //}

            return Ok();
        }

        [Route("find_audit_logs"), HttpPost]
        public async Task<ICollection<AuditLog>> FindAuditLogs([FromBody]AuditLogRequest request)
        {
            return await _auditLogRepository.Find(request);
        }

        [Route("update_leave"), HttpPost]
        public async Task<IActionResult> UpdateLeave([FromQuery(Name = "tenant_id")]int tenantId)
        {
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;
            _recordRepository.TenantId = _moduleRepository.TenantId = tenantId;

            var izinTuru = _recordRepository.Find("izin_turleri", new FindRequest { Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } }, Offset = 0, Limit = 9999 }, false).First;
            var calisanlar = _recordRepository.Find("calisanlar", new FindRequest { Filters = new List<Filter> { new Filter { Field = "calisma_durumu", Operator = Operator.Equals, Value = "Aktif", No = 1 } }, Offset = 0, Limit = 9999 }, false);

            foreach (JObject calisan in calisanlar)
            {
                //Set warehouse database name
                _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;
                await _calculationHelper.YillikIzinHesaplama((int)calisan["id"], (int)izinTuru["id"], _warehouse, tenantId);
            }

            return Ok("Updated all");
        }

        [Route("import_get_mapping"), HttpPost]
        public async Task<ImportMap> ImportGetMapping([FromBody]ImportMappingRequest request)
        {
            if (request == null)
                return null;

            var result = await _importRepository.GetImportMappingByName(request.Name, request.ModuleId);

            return result;
        }

        [Route("import_get_all_mappings/{id:int}"), HttpGet]
        public async Task<ICollection<ImportMap>> ImportGetAllMappings(int id)
        {
            if (id <= 0)
                return null;

            var result = await _importRepository.GetImportMappingByModuleId(id);

            return result;
        }

        [Route("import_mapping_save"), HttpPost]
        public async Task<IActionResult> ImportMappingSave([FromBody]ImportMappingRequest request)
        {
            var control = await _importRepository.GetImportMappingByName(request.Name, request.ModuleId);

            if (control != null)
                return BadRequest();

            var newImportTemplate = new ImportMap()
            {
                ModuleId = request.ModuleId,
                Skip = request.Skip,
                Name = request.Name,
                Mapping = request.Mapping
            };


            var result = await _importRepository.ImportMappingSave(newImportTemplate);

            if (result > 0)
                return Ok(newImportTemplate);
            else
                return null;
        }

        [Route("import_mapping_update/{id:int}"), HttpPost]
        public async Task<IActionResult> ImportMappingUpdate(int id, [FromBody]ImportMappingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id <= 0)
                return BadRequest("Record id not found");

            var mapping = await _importRepository.GetImportMappingById(id);

            if (mapping == null)
                return NotFound();

            mapping.Name = request.Name;
            mapping.Mapping = request.Mapping;
            mapping.Skip = request.Skip;

            var result = await _importRepository.ImportMappingUpdate(mapping);

            return Ok(result);
        }

        [Route("import_mapping_delete"), HttpPost]
        public async Task<IActionResult> ImportDeleteMapping([FromBody]ImportMappingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var mapping = await _importRepository.GetImportMappingById(request.Id);

            if (mapping == null)
                return NotFound();

            var result = await _importRepository.ImportMappingSoftDelete(mapping);

            return Ok(result);
        }
    }
}
