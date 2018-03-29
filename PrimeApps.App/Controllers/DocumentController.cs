using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.App.Helpers;
using PrimeApps.App.Results;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Document = PrimeApps.Model.Entities.Application.Document;
using RecordHelper = PrimeApps.App.Helpers.RecordHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Helpers.QueryTranslation;
using Aspose.Words;
using MimeMapping;
using Aspose.Words.MailMerging;

namespace PrimeApps.App.Controllers
{
    [Route("api/Document"), Authorize]
    public class DocumentController : BaseController
    {
        private IDocumentRepository _documentRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private ITemplateRepostory _templateRepository;
        private INoteRepository _noteRepository;
        private IPicklistRepository _picklistRepository;
        private ISettingRepository _settingRepository;

        public DocumentController(IDocumentRepository documentRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, ITemplateRepostory templateRepository, INoteRepository noteRepository, IPicklistRepository picklistRepository, ISettingRepository settingRepository)
        {
            _documentRepository = documentRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _templateRepository = templateRepository;
            _noteRepository = noteRepository;
            _picklistRepository = picklistRepository;
            _settingRepository = settingRepository;
        }

        /// <summary>
        /// Uploads files by the chucks as a stream.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <returns>System.String.</returns>
        [Route("Upload"), HttpPost]
        public async Task<IActionResult> Upload()
        {
            ;
            DocumentUploadResult result;
            var isUploaded = DocumentHelper.Upload(Request.Body, out result);

            if (!isUploaded && result == null)
                return NotFound();

            if (!isUploaded)
                return BadRequest();

            return Ok(result);
        }
        /// <summary>
        /// Using at email attachments and on module 
        /// </summary>
        /// <returns></returns>
        [Route("upload_attachment"), HttpPost]
        public async Task<IActionResult> UploadAttachment()
        {
            //Parse stream and get file properties.
            HttpMultipartParser parser = new HttpMultipartParser(Request.Body, "file");
            String blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");
            //if it is successfully parsed continue.
            if (parser.Success)
            {

                if (parser.FileContents.Length <= 0)
                {
                    //check the file size if it is 0 bytes then return client with that error code.
                    return BadRequest();
                }

                //declare chunk variables
                int chunk = 0; //current chunk
                int chunks = 1; //total chunk count

                string uniqueName = string.Empty;
                string container = string.Empty;

                //if parser has more then 1 parameters, it means that request is chunked.
                if (parser.Parameters.Count > 1)
                {
                    //calculate chunk variables.
                    chunk = int.Parse(parser.Parameters["chunk"]);
                    chunks = int.Parse(parser.Parameters["chunks"]);

                    //get the file name from parser
                    if (parser.Parameters.ContainsKey("name"))
                        uniqueName = parser.Parameters["name"];

                    if (parser.Parameters.ContainsKey("container"))
                        container = parser.Parameters["container"];
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;
                }

                //send stream and parameters to storage upload helper method for temporary upload.
                Storage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType);

                var result = new DocumentUploadResult();
                result.ContentType = parser.ContentType;
                result.UniqueName = uniqueName;

                if (chunk == chunks - 1)
                {
                    CloudBlockBlob blob = Storage.CommitFile(uniqueName, $"{container}/{uniqueName}", parser.ContentType, "pub", chunks);
                    result.PublicURL = $"{blobUrl}{blob.Uri.AbsolutePath}";
                }

                //return content type of the file to the client
                return Ok(result);
            }

            //this request invalid because there is no file, return fail code to the client.
            return NotFound();
        }
        /// <summary>
        /// Using at record document type upload for single file
        /// </summary>
        /// <returns></returns>
        [Route("upload_document_file"), HttpPost]
        public async Task<IActionResult> UploadDocumentFile()
        {
            //Parse stream and get file properties.
            HttpMultipartParser parser = new HttpMultipartParser(Request.Body, "file");
            String blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");
            //if it is successfully parsed continue.
            if (parser.Success)
            {

                if (parser.FileContents.Length <= 0)
                {
                    //check the file size if it is 0 bytes then return client with that error code.
                    return BadRequest();
                }

                if (Utils.BytesToMegabytes(parser.FileContents.Length) > 5) // 5 MB maximum
                {
                    return BadRequest();
                }


                string uniqueName = string.Empty;
                string fileName = string.Empty;
                string container = string.Empty;
                string moduleName = string.Empty;
                string fullFileName = string.Empty;
                string recordId = string.Empty;
                string documentSearch = string.Empty;
                bool documentSearchFlag = false;
                int uniqueRecordId = 0;

                //if parser has more then 1 parameters, it means that request is full filled by request maker object - uploader.
                if (parser.Parameters.Count > 1)
                {

                    //get the file name from parser
                    if (parser.Parameters.ContainsKey("name"))
                        uniqueName = parser.Parameters["name"];

                    if (parser.Parameters.ContainsKey("name"))
                        uniqueName = parser.Parameters["name"];

                    if (parser.Parameters.ContainsKey("filename"))
                        fileName = parser.Parameters["filename"];

                    if (parser.Parameters.ContainsKey("container"))
                        container = parser.Parameters["container"];

                    if (parser.Parameters.ContainsKey("modulename"))
                    {
                        moduleName = parser.Parameters["modulename"];
                        moduleName = moduleName.Replace("_", "-");
                    }




                    if (parser.Parameters.ContainsKey("documentsearch"))
                    {
                        documentSearch = parser.Parameters["documentsearch"];
                        bool.TryParse(documentSearch, out documentSearchFlag);
                    }
                    if (parser.Parameters.ContainsKey("recordid"))
                    {
                        recordId = parser.Parameters["recordid"];
                        int.TryParse(recordId, out uniqueRecordId);
                    }


                }

                if (string.IsNullOrEmpty(uniqueName) || string.IsNullOrEmpty(container) || string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(fileName) || uniqueRecordId == 0)
                {
                    return BadRequest();
                }

                var ext = Path.GetExtension(parser.Filename);
                fullFileName = uniqueRecordId + "_" + uniqueName + ext;

                if (ext != ".pdf" && ext != ".txt" && ext != ".doc" && ext != ".docx")
                {
                    return BadRequest();
                }


                var chunk = 0;
                var chunks = 1; //one part chunk

                //send stream and parameters to storage upload helper method for temporary upload.
                Storage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", fullFileName, parser.ContentType);

                var result = new DocumentUploadResult();
                result.ContentType = parser.ContentType;
                result.UniqueName = fullFileName;


                CloudBlockBlob blob = Storage.CommitFile(fullFileName, $"{container}/{moduleName}/{fullFileName}", parser.ContentType, "module-documents", chunks, BlobContainerPublicAccessType.Blob, "temp", uniqueRecordId.ToString(), moduleName, fileName, fullFileName);
                result.PublicURL = $"{blobUrl}{blob.Uri.AbsolutePath}";


                if (documentSearchFlag == true)
                {
                    var documentSearchHelper = new DocumentSearch();

                    documentSearchHelper.CreateOrUpdateIndexOnDocumentBlobStorage(AppUser.TenantGuid.ToString(), moduleName, false);//False because! 5 min auto index incremental change detection policy check which azure provided

                }

                //return content type of the file to the client
                return Ok(result);
            }

            //this request invalid because there is no file, return fail code to the client.
            return NotFound();
        }
        [Route("remove_document"), HttpPost]
        public async Task<IActionResult> RemoveModuleDocument([FromBody]JObject data)
        {
            string tenantId,
                module,
                recordId,
                fieldName,
                fileNameExt;
            module = data["module"]?.ToString();
            recordId = data["recordId"]?.ToString();
            fieldName = data["fieldName"]?.ToString();
            fileNameExt = data["fileNameExt"]?.ToString();
            tenantId = data["tenantId"]?.ToString();

            bool isOperationAllowed = await Cache.Tenant.CheckPermission(PermissionEnum.Write, null, EntityType.Document, int.Parse(tenantId), AppUser.Id);

            if (int.Parse(tenantId) != AppUser.TenantId || !isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return Forbid();
            }

            string moduleDashesName = module.Replace("_", "-");

            var containerName = "module-documents";
            //var ext = Path.GetExtension(fileName);

            string uniqueFileName = $"{AppUser.TenantGuid}/{moduleDashesName}/{recordId}_{fieldName}.{fileNameExt}";

            //remove document
            Storage.RemoveFile(containerName, uniqueFileName);



            var moduleData = await _moduleRepository.GetByNameBasic(module);
            var field = moduleData.Fields.FirstOrDefault(x => x.Name == fieldName);
            if (field.DocumentSearch)
            {
                DocumentSearch documentSearch = new DocumentSearch();
                documentSearch.CreateOrUpdateIndexOnDocumentBlobStorage(tenantId, module, false);
            }

            return Ok();
        }

        [Route("upload_large"), HttpPost]
        public async Task<IActionResult> UploadLarge()
        {
            var parser = new HttpMultipartParser(Request.Body, "file");

            //if it is successfully parsed continue.
            if (parser.Success)
            {
                if (parser.FileContents.Length <= 0)
                {
                    //check the file size if it is 0 bytes then return client with that error code.
                    return BadRequest();
                }

                var chunks = parser.FileContents.Split(2048 * 1024).ToList(); //split to 2 mb.
                var ext = Path.GetExtension(parser.Filename);
                var uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;

                for (var i = 0; i < chunks.Count; i++)
                {
                    //send stream and parameters to storage upload helper method for temporary upload.
                    Storage.UploadFile(i, new MemoryStream(chunks[i]), "temp", uniqueName, parser.ContentType);
                }

                var result = new DocumentUploadResult
                {
                    ContentType = parser.ContentType,
                    UniqueName = uniqueName,
                    Chunks = chunks.Count
                };

                //return content type of the file to the client
                return Ok(result);
            }

            //this request invalid because there is no file, return fail code to the client.
            return NotFound();
        }

        [Route("Create"), HttpPost]
        /// <summary>
        /// Validates and creates document record permanently after temporary upload process completed.
        /// </summary>
        /// <param name="document">The document.</param>
        public async Task<IActionResult> Create(DocumentDTO document)
        {
            //validate instance id for the request.

            bool isOperationAllowed = await Cache.Tenant.CheckPermission(PermissionEnum.Write, null, EntityType.Document, document.TenantId, AppUser.Id);

            if (document.TenantId != AppUser.TenantId || !isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return Forbid();
            }
            //get entity name if this document is uploading to a specific entity.
            string uniqueStandardizedName = document.FileName.Replace(" ", "-");

            uniqueStandardizedName = Regex.Replace(uniqueStandardizedName, @"[^\u0000-\u007F]", string.Empty);

            Document currentDoc = new Document()
            {
                FileSize = document.FileSize,
                Description = document.Description,
                ModuleId = document.ModuleId,
                Name = document.FileName,
                CreatedAt = DateTime.UtcNow,
                Type = document.MimeType,
                UniqueName = document.UniqueFileName,
                RecordId = document.RecordId,
                Deleted = false
            };
            if (await _documentRepository.CreateAsync(currentDoc) != null)
            {
                //transfer file to the permanent storage by committing it.
                Storage.CommitFile(document.UniqueFileName, currentDoc.UniqueName, currentDoc.Type, string.Format("inst-{0}", AppUser.TenantGuid), document.ChunkSize);
                return Ok(currentDoc.Id.ToString());
            }

            return BadRequest("Couldn't Create Document!");

        }

        [Route("image_create"), HttpPost]
        /// <summary>
        /// Validates and creates document record permanently after temporary upload process completed.
        /// </summary>
        /// <param name="document">The document.</param>
        public async Task<IActionResult> ImageCreate([FromBody]JObject data)
        {

            string UniqueFileName, MimeType, TenantId;
            UniqueFileName = data["UniqueFileName"]?.ToString();
            MimeType = data["MimeType"]?.ToString();
            TenantId = data["TenantId"]?.ToString();

            int ChunkSize = Int32.Parse(data["ChunkSize"]?.ToString());

            if (UniqueFileName != null)
            {
                Storage.CommitFile(UniqueFileName, UniqueFileName, MimeType, "record-detail-" + TenantId, ChunkSize);
                return Ok(UniqueFileName);
            }

            return BadRequest("Couldn't Create Document!");

        }

        /// <summary>
        /// Gets last five document records for an entity.
        /// </summary>
        /// <param name="EntityType">Type of the entity.</param>
        /// <param name="EntityID">The entity identifier.</param>
        /// <param name="InstanceID">The instance identifier.</param>
        /// <returns>IList{DTO.DocumentResult}.</returns>
        [Route("GetEntityDocuments"), HttpPost]
        public async Task<IActionResult> GetEntityDocuments(DocumentRequest req)
        {
            //validate instance id for the request.
            bool isOperationAllowed = await Cache.Tenant.CheckPermission(PermissionEnum.Read, null, EntityType.Document, req.TenantId, AppUser.Id);
            if (AppUser.TenantId != req.TenantId || !isOperationAllowed)
            {
                //if the requested instance does not belong to the current session, than return Forbidden status message
                return Forbid();
            }

            //Get last 5 entity documents and return it to the client.
            var result = await _documentRepository.GetTopEntityDocuments(new DocumentRequest()
            {
                TenantId = req.TenantId,
                ModuleId = req.ModuleId,
                RecordId = req.RecordId,
                UserId = AppUser.Id,
                ContainerId = AppUser.TenantGuid
            });
            return Ok(result);
        }

        /// <summary>
        /// Gets documents for the requested entity and instance. If entity id is empty, query will be resulted with all documents of instance.
        /// </summary>
        /// <param name="EntityType">Type of the entity.</param>
        /// <param name="EntityID">The entity identifier.</param>
        /// <param name="TenantId">The instance identifier.</param>
        /// <returns>DocumentExplorerResult.</returns>
        [Route("GetDocuments"), HttpPost]
        public async Task<IActionResult> GetDocuments(DocumentRequest req)
        {
            //validate the instance id by the session instances.
            bool isOperationAllowed = await Cache.Tenant.CheckPermission(PermissionEnum.Read, null, EntityType.Document, req.TenantId, AppUser.Id);

            if (req.TenantId != AppUser.TenantId || !isOperationAllowed)
            {
                //if the requested instance does not belong to the current session, than return Forbidden status message
                return Forbid();
            }

            //var docresult = crmDocuments.GetDocuments(req.InstanceID, req.EntityID);

            return Ok(await _documentRepository.GetDocuments(new DocumentRequest()
            {
                TenantId = req.TenantId,
                ModuleId = req.ModuleId,
                RecordId = req.RecordId,
                UserId = AppUser.Id,
                ContainerId = AppUser.TenantGuid
            }));

        }

        /// <summary>
        /// Get document info with id.
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        [Route("GetDocument"), HttpPost]
        public async Task<IActionResult> GetDocumentById([FromBody] int fileID)
        {

            bool isOperationAllowed = await Cache.Tenant.CheckPermission(PermissionEnum.Read, null, EntityType.Document, AppUser.TenantId, AppUser.Id);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return Forbid();
            }

            //get the document record from database
            var doc = await _documentRepository.GetById(fileID);
            if (doc != null)
            {
                return Ok(doc);
            }
            return Ok();
        }

        /// <summary>
        /// Downloads the file from cloud to client as a stream.
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        [Route("Download"), HttpGet]
        public async Task<IActionResult> Download([FromRoute] int fileID)
        {
            //get the document record from database
            var doc = await _documentRepository.GetById(fileID);
            string publicName = "";

            /* disabled temporarily - https://ofisim.visualstudio.com/Ofisim/_workitems/edit/757 must be solved
            bool isOperationAllowed = await Cache.Workgroup.CheckPermission(PermissionEnum.Read, null, EntityType.Document, AppUser.InstanceId, AppUser.LocalId);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return new  ForbiddenResult(Request.HttpContext.GetHttpRequestMessage());
            }
            */

            if (doc != null)
            {
                //if there is a document with this id, try to get it from blob storage.
                var blob = Storage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), doc.UniqueName);
                try
                {
                    //try to get the attributes of blob.
                    await blob.FetchAttributesAsync();
                }
                catch (Exception)
                {
                    //if there is an exception, it means there is no such file.
                    return NotFound();
                }

                //Is bandwidth enough to download this file?
                //Bandwidth is enough, send the Storage.
                publicName = doc.Name;


                return new FileDownloadResult()
                {
                    Blob = blob,
                    PublicName = doc.Name
                };
            }
            else
            {
                //there is no such file, return
                return NotFound();
            }
        }

        /// <summary>
        /// Downloads the file from cloud to client as a stream.
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        [Route("download_module_document"), HttpGet]
        public async Task<IActionResult> DownloadModuleDocument([FromRoute] string module, [FromRoute] string fileName, [FromRoute] string fileNameExt, [FromRoute] string fieldName, [FromRoute] string recordId)
        {

            var publicName = "";

            /* disabled temporarily - https://ofisim.visualstudio.com/Ofisim/_workitems/edit/757 must be solved
            bool isOperationAllowed = await Cache.Workgroup.CheckPermission(PermissionEnum.Read, null, EntityType.Document, AppUser.InstanceId, AppUser.LocalId);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return new  ForbiddenResult(Request.HttpContext.GetHttpRequestMessage());
            }
            */

            if (!string.IsNullOrEmpty(module) && !string.IsNullOrEmpty(fileNameExt) && !string.IsNullOrEmpty(fieldName))
            {

                var containerName = "module-documents";
                //var ext = Path.GetExtension(fileName);

                module = module.Replace("_", "-");

                string uniqueFileName = $"{AppUser.TenantGuid}/{module}/{recordId}_{fieldName}.{fileNameExt}";
                var docName = fileName + "." + fileNameExt;

                //if there is a document with this id, try to get it from blob storage.
                var blob = Storage.GetBlob(containerName, uniqueFileName);
                try
                {
                    //try to get the attributes of blob.
                    await blob.FetchAttributesAsync();
                }
                catch (Exception)
                {
                    //if there is an exception, it means there is no such file.
                    return NotFound();
                }
                publicName = docName;

                return new FileDownloadResult()
                {
                    Blob = blob,
                    PublicName = docName
                };
            }
            else
            {
                //there is no such file, return
                return BadRequest();
            }
        }

        /// <summary>
        /// Removes the document from database and blob storage.
        /// </summary>
        /// <param name="DocID">The document identifier.</param>
        [Route("Remove"), HttpPost]
        public async Task<IActionResult> Remove(DocumentDTO doc)
        {

            bool isOperationAllowed = await Cache.Tenant.CheckPermission(PermissionEnum.Remove, null, EntityType.Document, doc.TenantId, AppUser.Id);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return Forbid();
            }


            //if the document is not null open a new session and transaction
            var result = await _documentRepository.RemoveAsync(doc.ID);
            if (result != null)
            {
                //Update storage license for instance.
                return Ok();
            }


            return NotFound();
        }

        /// <summary>
        /// Modifies the file record in database.
        /// </summary>
        /// <param name="document">The document.</param>
        [Route("Modify"), HttpPost]
        public async Task<IActionResult> Modify(DocumentDTO document)
        {
            bool isOperationAllowed = await Cache.Tenant.CheckPermission(PermissionEnum.Modify, null, EntityType.Document, document.TenantId, AppUser.Id);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return Forbid();
            }

            var updatedDoc = await _documentRepository.UpdateAsync(new Document()
            {
                Id = document.ID,
                Description = document.Description,
                ModuleId = document.ModuleId,
                RecordId = document.RecordId,
                Name = document.FileName?.ToString()
            });

            if (updatedDoc != null)
                return Ok(updatedDoc);

            return NotFound();

        }


        [Route("export"), HttpGet]
        public async Task<IActionResult> Export([FromRoute]string module, [FromRoute]int id, [FromRoute]int templateId, [FromRoute]string format, [FromRoute]string locale, [FromRoute]int timezoneOffset = 180, [FromRoute] bool save = false)
        {
            JObject record;
            var relatedModuleRecords = new Dictionary<string, JArray>();
            var notes = new List<Note>();
            var moduleEntity = await _moduleRepository.GetByName(module);
            var currentCulture = locale == "en" ? "en-US" : "tr-TR";


            if (moduleEntity == null)
                return BadRequest();

            var templateEntity = await _templateRepository.GetById(templateId);

            if (templateEntity == null)
                return BadRequest();

            //if there is a template with this id, try to get it from blob storage.
            var templateBlob = Storage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), $"templates/{templateEntity.Content}");

            try
            {
                //try to get the attributes of blob.
                await templateBlob.FetchAttributesAsync();
            }
            catch (Exception)
            {
                //if there is an exception, it means there is no such file.
                return NotFound();
            }

            if (module == "users")
                moduleEntity = Model.Helpers.ModuleHelper.GetFakeUserModule();

            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository);

            try
            {
                record = _recordRepository.GetById(moduleEntity, id, !AppUser.HasAdminProfile, lookupModules);

                if (record == null)
                    return NotFound();

                foreach (var field in moduleEntity.Fields)
                {
                    if (field.Permissions.Count > 0)
                    {
                        foreach (var permission in field.Permissions)
                        {
                            if (AppUser.ProfileId == permission.ProfileId && permission.Type == FieldPermissionType.None && !record[field.Name].IsNullOrEmpty())
                            {
                                record[field.Name] = null;
                            }
                        }
                    }
                }

                record = await Model.Helpers.RecordHelper.FormatRecordValues(moduleEntity, record, _moduleRepository, _picklistRepository, AppUser.TenantLanguage, currentCulture, timezoneOffset, lookupModules);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.UndefinedTable)
                    return NotFound();

                throw;
            }

            Aspose.Words.Document doc;

            // Open a template document.
            using (var template = new MemoryStream())
            {
                await templateBlob.DownloadToStreamAsync(template);
                doc = new Aspose.Words.Document(template);
            }

            // Add related module records.
            await AddRelatedModuleRecords(relatedModuleRecords, notes, moduleEntity, lookupModules, doc, record, module, id, currentCulture, timezoneOffset);

            doc.MailMerge.UseNonMergeFields = true;
            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedRegions | MailMergeCleanupOptions.RemoveUnusedFields;
            doc.MailMerge.FieldMergingCallback = new FieldMergingCallback(AppUser.TenantGuid);

            var mds = new MailMergeDataSource(record, module, moduleEntity, relatedModuleRecords, notes: notes);

            try
            {
                doc.MailMerge.ExecuteWithRegions(mds);
            }
            catch (Exception ex)
            {
                return BadRequest(AppUser.TenantLanguage == "tr" ? "Geçersiz şablon. Lütfen şablon içerisindeki etiketleri kontrol ediniz. Hata Mesajı: " + ex.Message : "Invalid template. Please check tags in templates. Error Message: " + ex.Message);
            }

            var rMessage = new HttpResponseMessage();
            Stream outputStream = new MemoryStream();

            SaveFormat sf;
            var localModuleName = AppUser.TenantLanguage.Contains("tr") ? moduleEntity.LabelTrSingular : moduleEntity.LabelEnSingular;
            var fileName = $"{localModuleName}";
            switch (format)
            {
                case "pdf":
                    sf = SaveFormat.Pdf;
                    fileName = $"{fileName}.pdf";
                    break;
                case "docx":
                    sf = SaveFormat.Docx;
                    fileName = $"{fileName}.docx";
                    break;
                default:
                    sf = SaveFormat.Docx;
                    fileName = $"{fileName}.docx";
                    break;
            }

            Aspose.Words.Saving.SaveOptions saveOptions = Aspose.Words.Saving.SaveOptions.CreateSaveOptions(sf);

            doc.Save(outputStream, saveOptions);
            outputStream.Position = 0;
            var mimeType = MimeUtility.GetMimeMapping(fileName);
            if (save)
            {

                Storage.UploadFile(0, outputStream, "temp", fileName, mimeType);
                var blob = Storage.CommitFile(fileName, Guid.NewGuid().ToString().Replace("-", "") + "." + format, mimeType, "pub", 1);

                outputStream.Position = 0;
                var blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");
                var result = new { filename = fileName, fileurl = $"{blobUrl}{blob.Uri.AbsolutePath}" };

                return Ok(result);
            }
            rMessage.Content = new StreamContent(outputStream);
            rMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            rMessage.StatusCode = HttpStatusCode.OK;
            rMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = fileName
            };

            var response = ResponseMessage(rMessage);

            return response;
        }

        [Route("download_template"), HttpGet]
        public async Task<IActionResult> DownloadTemplate([FromRoute]int templateId)
        {
            //get the document record from database
            var template = await _templateRepository.GetById(templateId);
            string publicName = "";

            bool isOperationAllowed = await Cache.Tenant.CheckPermission(PermissionEnum.Read, null, EntityType.Document, AppUser.TenantId, AppUser.Id);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return Forbid();
            }

            if (template != null)
            {
                //if there is a document with this id, try to get it from blob storage.
                var blob = Storage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), $"templates/{template.Content}");
                try
                {
                    //try to get the attributes of blob.
                    await blob.FetchAttributesAsync();
                }
                catch (Exception)
                {
                    //if there is an exception, it means there is no such file.
                    return NotFound();
                }

                //Bandwidth is enough, send the Storage.
                publicName = template.Name;

                string[] splittedFileName = template.Content.Split('.');
                string extension = splittedFileName.Length > 1 ? splittedFileName[1] : "docx";

                return new FileDownloadResult()
                {
                    Blob = blob,
                    PublicName = $"{template.Name}.{extension}"
                };
            }
            else
            {
                //there is no such file, return
                return NotFound();
            }
        }
        [Route("document_search"), HttpPost]
        public async Task<IActionResult> SearchDocument(DocumentFilterRequest filterRequest)
        {
            if (filterRequest != null && filterRequest.Filters.Count > 0)
            {
                DocumentSearch search = new DocumentSearch();

                var searchIndexName = AppUser.TenantGuid + "-" + filterRequest.Module;

                if (filterRequest.Module == null || filterRequest.Top > 50)
                {
                    return BadRequest();
                }

                var results = search.AdvancedSearchDocuments(searchIndexName, filterRequest.Filters, filterRequest.Top, filterRequest.Skip);

                return Ok(results);
            }
            return BadRequest();
        }

        private async Task<bool> AddRelatedModuleRecords(Dictionary<string, JArray> relatedModuleRecords, List<Note> notes, Module moduleEntity, ICollection<Module> lookupModules, Aspose.Words.Document doc, JObject record, string module, int recordId, string currentCulture, int timezoneOffset)
        {
            // Get second level relations
            var secondLevelRelationSetting = _settingRepository.Get(SettingType.Template);
            var secondLevels = new List<SecondLevel>();

            if (secondLevelRelationSetting != null && secondLevelRelationSetting.Count > 0)
            {
                var secondLevelRelations = secondLevelRelationSetting.Where(x => x.Key == "second_level_relation_" + module).ToList();

                foreach (var secondLevelRelation in secondLevelRelations)
                {
                    var secondLevelRelationParts = secondLevelRelation.Value.Split('>');
                    var secondLevelModuleRelationId = int.Parse(secondLevelRelationParts[0]);
                    var secondLevelSubModuleRelationId = int.Parse(secondLevelRelationParts[1]);
                    var secondLevelModuleRelation = moduleEntity.Relations.FirstOrDefault(x => !x.Deleted && x.Id == secondLevelModuleRelationId);

                    if (secondLevelModuleRelation != null)
                    {
                        var secondLevelModuleEntity = await _moduleRepository.GetByName(secondLevelModuleRelation.RelatedModule);
                        var secondLevelSubModuleRelation = secondLevelModuleEntity.Relations.FirstOrDefault(x => !x.Deleted && x.Id == secondLevelSubModuleRelationId);
                        Module secondLevelSubModuleEntity = null;

                        if (secondLevelSubModuleRelation != null)
                            secondLevelSubModuleEntity = await _moduleRepository.GetByName(secondLevelSubModuleRelation.RelatedModule);
                        else
                            secondLevelModuleEntity = null;

                        if (secondLevelModuleEntity != null)
                        {
                            secondLevels.Add(new SecondLevel
                            {
                                RelationId = secondLevelModuleRelationId,
                                Module = secondLevelModuleEntity,
                                SubModule = secondLevelSubModuleEntity,
                                SubRelation = secondLevelSubModuleRelation
                            });
                        }
                    }
                }
            }

            // Get related module records.
            foreach (Relation relation in moduleEntity.Relations.Where(x => !x.Deleted && x.RelationType != RelationType.ManyToMany).ToList())
            {
                if (!doc.Range.Text.Contains("{{#foreach " + relation.RelatedModule + "}}"))
                    continue;

                var fields = await Helpers.RecordHelper.GetAllFieldsForFindRequest(relation.RelatedModule, _moduleRepository);

                var findRequest = new FindRequest
                {
                    Fields = fields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = relation.RelationField,
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        }
                    },
                    SortField = "created_at",
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                };

                if (relation.RelatedModule == "activities")
                {
                    findRequest.Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = "related_to",
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        },
                        new Filter
                        {
                            Field = "related_module",
                            Operator = Operator.Is,
                            No = 2,
                            Value = AppUser.TenantLanguage.Contains("tr") ? moduleEntity.LabelTrSingular : moduleEntity.LabelEnSingular
                        }
                    };
                }

                var records = _recordRepository.Find(relation.RelatedModule, findRequest);
                Module relatedModuleEntity;
                ICollection<Module> relatedLookupModules;

                if (moduleEntity.Name == relation.RelatedModule)
                {
                    relatedModuleEntity = moduleEntity;
                    relatedLookupModules = lookupModules;
                }
                else
                {
                    relatedModuleEntity = await _moduleRepository.GetByNameBasic(relation.RelatedModule);
                    relatedLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(relatedModuleEntity, _moduleRepository);
                }

                var recordsFormatted = new JArray();
                var secondLevel = secondLevels.SingleOrDefault(x => x.RelationId == relation.Id);

                foreach (JObject recordItem in records)
                {
                    var recordFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(relatedModuleEntity, recordItem, _moduleRepository, _picklistRepository, AppUser.TenantLanguage, currentCulture, timezoneOffset, relatedLookupModules);

                    if (secondLevel != null)
                        await AddSecondLevelRecords(recordFormatted, secondLevel.Module, secondLevel.SubRelation, (int)recordItem["id"], secondLevel.SubModule, currentCulture, timezoneOffset);

                    recordsFormatted.Add(recordFormatted);
                }

                relatedModuleRecords.Add(relation.RelatedModule, recordsFormatted);
            }

            // Many to many related module records
            foreach (Relation relation in moduleEntity.Relations.Where(x => !x.Deleted && x.RelationType == RelationType.ManyToMany).ToList())
            {
                if (!doc.Range.Text.Contains("{{#foreach " + relation.RelatedModule + "}}"))
                    continue;

                var fields = await Helpers.RecordHelper.GetAllFieldsForFindRequest(relation.RelatedModule, _moduleRepository, false);
                var fieldsManyToMany = new List<string>();

                foreach (var field in fields)
                {
                    fieldsManyToMany.Add(relation.RelatedModule + "_id." + relation.RelatedModule + "." + field);
                }

                var records = _recordRepository.Find(relation.RelatedModule, new FindRequest
                {
                    Fields = fieldsManyToMany,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = moduleEntity.Name + "_id",
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        }
                    },
                    ManyToMany = moduleEntity.Name,
                    SortField = relation.RelatedModule + "_id." + relation.RelatedModule + ".created_at",
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                });

                Module relatedModuleEntity;
                ICollection<Module> relatedLookupModules;

                if (moduleEntity.Name == relation.RelatedModule)
                {
                    relatedModuleEntity = moduleEntity;
                    relatedLookupModules = new List<Module> { moduleEntity };
                }
                else
                {
                    relatedModuleEntity = await _moduleRepository.GetByNameBasic(relation.RelatedModule);
                    relatedLookupModules = new List<Module> { relatedModuleEntity };
                }

                var recordsFormatted = new JArray();
                var secondLevel = secondLevels.SingleOrDefault(x => x.RelationId == relation.Id);

                foreach (JObject recordItem in records)
                {
                    var recordFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(relatedModuleEntity, recordItem, _moduleRepository, _picklistRepository, AppUser.TenantLanguage, currentCulture, timezoneOffset, relatedLookupModules);

                    if (secondLevel != null)
                        await AddSecondLevelRecords(recordFormatted, secondLevel.Module, secondLevel.SubRelation, (int)recordItem[relation.RelatedModule + "_id"], secondLevel.SubModule, currentCulture, timezoneOffset);

                    recordsFormatted.Add(recordFormatted);
                }

                relatedModuleRecords.Add(relation.RelatedModule, recordsFormatted);
            }

            // Get notes of the record.
            if (doc.Range.Text.Contains("{{#foreach notes}}"))
            {
                var noteList = await _noteRepository.Find(new NoteRequest()
                {
                    Limit = 1000,
                    ModuleId = moduleEntity.Id,
                    Offset = 0,
                    RecordId = recordId
                });

                notes.AddRange(noteList);
            }

            if (module == "quotes" && doc.Range.Text.Contains("{{#foreach quote_products}}"))
            {
                var quoteFields = await Helpers.RecordHelper.GetAllFieldsForFindRequest("quote_products", _moduleRepository);

                var products = _recordRepository.Find("quote_products", new FindRequest()
                {
                    Fields = quoteFields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = "quote",
                            Operator = Operator.Equals,
                            No = 0,
                            Value = recordId.ToString()
                        }
                    },
                    SortField = "order",
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                });

                var quoteProductsModuleEntity = await _moduleRepository.GetByNameBasic("quote_products");
                var quoteProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(quoteProductsModuleEntity, _moduleRepository);
                var productsFormatted = new JArray();

                foreach (var product in products)
                {
                    if (!record["currency"].IsNullOrEmpty())
                        product["currency"] = (string)record["currency"];

                    if (!product["product.products.currency"].IsNullOrEmpty())
                        product["currency"] = (string)product["product.products.currency"];

                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(quoteProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, AppUser.TenantLanguage, currentCulture, timezoneOffset, quoteProductsLookupModules);
                    if (!productFormatted["separator"].IsNullOrEmpty())
                    {
                        productFormatted["product.products.name"] = productFormatted["separator"] + "-product_separator_separator";
                    }

                    productsFormatted.Add(productFormatted);
                }

                relatedModuleRecords.Add("quote_products", productsFormatted);
            }

            if (module == "sales_orders" && doc.Range.Text.Contains("{{#foreach order_products}}"))
            {
                var orderFields = await Helpers.RecordHelper.GetAllFieldsForFindRequest("order_products", _moduleRepository);

                var products = _recordRepository.Find("order_products", new FindRequest()
                {
                    Fields = orderFields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = "sales_order",
                            Operator = Operator.Equals,
                            No = 0,
                            Value = recordId.ToString()
                        }
                    },
                    SortField = "order",
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                });

                var orderProductsModuleEntity = await _moduleRepository.GetByNameBasic("order_products");
                var orderProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(orderProductsModuleEntity, _moduleRepository);
                var productsFormatted = new JArray();

                foreach (var product in products)
                {
                    if (!record["currency"].IsNullOrEmpty())
                        product["currency"] = (string)record["currency"];

                    if (!product["product.products.currency"].IsNullOrEmpty())
                        product["currency"] = (string)product["product.products.currency"];

                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(orderProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, AppUser.TenantLanguage, currentCulture, timezoneOffset, orderProductsLookupModules);
                    productsFormatted.Add(productFormatted);
                }

                relatedModuleRecords.Add("order_products", productsFormatted);
            }

            return true;
        }

        private async Task<bool> AddSecondLevelRecords(JObject record, Module secondLevelModuleEntity, Relation relation, int recordId, Module secondLevelSubModuleEntity, string currentCulture, int timezoneOffset)
        {
            record[secondLevelSubModuleEntity.Name] = "";
            var primaryField = secondLevelSubModuleEntity.Fields.Single(x => x.Primary);
            JArray records;

            if (relation.RelationType != RelationType.ManyToMany)
            {
                var fields = await Helpers.RecordHelper.GetAllFieldsForFindRequest(relation.RelatedModule, _moduleRepository);

                var secondLevelFindRequest = new FindRequest
                {
                    Fields = fields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = relation.RelationField,
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        }
                    },
                    SortField = primaryField.Name,
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                };

                records = _recordRepository.Find(secondLevelSubModuleEntity.Name, secondLevelFindRequest);
                var recordsFormatted = new JArray();

                foreach (JObject recordItem in records)
                {
                    var recordItemFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(secondLevelSubModuleEntity, recordItem, _moduleRepository, _picklistRepository, AppUser.TenantLanguage, currentCulture, timezoneOffset);
                    record[secondLevelSubModuleEntity.Name] += (string)recordItemFormatted[primaryField.Name] + ControlChar.LineBreak;
                    recordsFormatted.Add(recordItemFormatted);
                }

                record[secondLevelSubModuleEntity.Name + "_records"] = recordsFormatted;
            }
            else
            {
                var fields = await Helpers.RecordHelper.GetAllFieldsForFindRequest(relation.RelatedModule, _moduleRepository, false);
                var fieldsManyToMany = new List<string>();

                foreach (var field in fields)
                {
                    fieldsManyToMany.Add(relation.RelatedModule + "_id." + relation.RelatedModule + "." + field);
                }

                var secondLevelFindRequestManyToMany = new FindRequest
                {
                    Fields = fieldsManyToMany,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = secondLevelModuleEntity.Name + "_id",
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        }
                    },
                    ManyToMany = secondLevelModuleEntity.Name,
                    SortField = relation.RelatedModule + "_id." + relation.RelatedModule + "." + primaryField.Name,
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                };

                records = _recordRepository.Find(relation.RelatedModule, secondLevelFindRequestManyToMany);
                var recordsFormatted = new JArray();

                foreach (JObject recordItem in records)
                {
                    var recordItemFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(secondLevelSubModuleEntity, recordItem, _moduleRepository, _picklistRepository, AppUser.TenantLanguage, currentCulture, timezoneOffset);
                    record[secondLevelSubModuleEntity.Name] += (string)recordItemFormatted[relation.RelatedModule + "_id." + relation.RelatedModule + "." + primaryField.Name] + ControlChar.LineBreak;
                    recordsFormatted.Add(recordItemFormatted);
                }

                record[secondLevelSubModuleEntity.Name + "_records"] = recordsFormatted;
            }

            return true;
        }

        [Route("find"), HttpPost]
        public async Task<ICollection<DocumentResult>> Find(DocumentFindRequest request)
        {
            var documents = await _documentRepository.GetDocumentsByLimit(request);

            return documents;
        }

        [Route("count"), HttpPost]
        public async Task<int> Count(DocumentFindRequest request)
        {
            return await _documentRepository.Count(request);
        }

        private class SecondLevel
        {
            public int RelationId { get; set; }
            public Module Module { get; set; }
            public Module SubModule { get; set; }
            public Relation SubRelation { get; set; }

        }
    }
}
