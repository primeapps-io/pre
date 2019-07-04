using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Storage;

namespace PrimeApps.Studio.Controllers
{
    [Route("storage")]
    public class StorageController : DraftBaseController
    {
        private IDocumentRepository _documentRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private ITemplateRepository _templateRepository;
        private INoteRepository _noteRepository;
        private ISettingRepository _settingRepository;
        private IImportRepository _importRepository;
        private IUnifiedStorage _storage;
        private IConfiguration _configuration;

        public StorageController(IDocumentRepository documentRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, ITemplateRepository templateRepository, INoteRepository noteRepository, IPicklistRepository picklistRepository, ISettingRepository settingRepository, IImportRepository importRepository, IUnifiedStorage storage, IConfiguration configuration)
        {
            _documentRepository = documentRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _templateRepository = templateRepository;
            _noteRepository = noteRepository;
            _settingRepository = settingRepository;
            _importRepository = importRepository;
            _storage = storage;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_documentRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_recordRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_moduleRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_templateRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_noteRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_importRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Uploads files by the chucks as a stream.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <returns>System.String.</returns>
        //[HttpPost("upload")]
        //[DisableRequestSizeLimit]
        //public async Task<IActionResult> Upload(IFormCollection form)
        //{
        //    IFormFile file = form.Files.First();
        //    StringValues bucketName = $"tenant{AppUser.TenantId}",
        //        chunksStr,
        //        uploadId,
        //        chunkStr,
        //        fileName,
        //        responseList,
        //        type,
        //        container;
        //    form.TryGetValue("chunks", out chunksStr);
        //    form.TryGetValue("chunk", out chunkStr);
        //    form.TryGetValue("name", out fileName);
        //    form.TryGetValue("upload_id", out uploadId);
        //    form.TryGetValue("response_list", out responseList);
        //    form.TryGetValue("type", out type);
        //    form.TryGetValue("container", out container);
        //    ObjectType objectType = UnifiedStorage.GetType(type);


        //    if (!string.IsNullOrWhiteSpace(container))
        //    {
        //        bucketName = GetPath(type, AppUser.TenantId, container);
        //    }
        //    else
        //    {
        //        bucketName = GetPath(type, AppUser.TenantId);
        //    }

        //    int chunk = 0,
        //        chunks = 1;

        //    int.TryParse(chunksStr, out chunks);
        //    int.TryParse(chunkStr, out chunk);
        //    chunk++; // increase it since s3 chunk indexes starting from 1 instead of 0

        //    MultipartResponse response = new MultipartResponse();
        //    CompleteMultipartUploadResponse uploadResult;
        //    try
        //    {
        //        if (chunk == 1)
        //        {
        //            uploadId = await _storage.InitiateMultipartUpload(bucketName, fileName);
        //            response.Status = MultipartStatusEnum.Initiated;
        //            response.UploadId = uploadId;
        //        }

        //        response.ETag = await _storage.UploadPart(bucketName, fileName, chunk, chunks, uploadId, file.OpenReadStream());
        //        if (response.Status == MultipartStatusEnum.NotSet)
        //            response.Status = MultipartStatusEnum.ChunkUpload;

        //        if (chunk == chunks)
        //        {
        //            uploadResult = await _storage.CompleteMultipartUpload(bucketName, fileName, responseList, response.ETag, uploadId);

        //            response.Status = MultipartStatusEnum.Completed;

        //            if (objectType == ObjectType.NOTE || objectType == ObjectType.PROFILEPICTURE || objectType == ObjectType.MAIL) // Add here the types where publicURLs are required.
        //            {
        //                var clearRoot = objectType == ObjectType.PROFILEPICTURE;

        //                response.PublicURL = _storage.GetShareLink(bucketName, fileName, DateTime.UtcNow.AddYears(100), Amazon.S3.Protocol.HTTP, clearRoot);
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        await _storage.AbortMultipartUpload(bucketName, fileName, uploadId);
        //        response.Status = MultipartStatusEnum.Aborted;
        //    }

        //    return Json(response);
        //}

        [HttpPost("upload_attachment")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadWhole()
        {
            var parser = new HttpMultipartParser(Request.Body, "file");
            StringValues bucketName = UnifiedStorage.GetPath("attachment", OrganizationId, (int)AppId);

            //if it is successfully parsed continue.
            if (parser.Success)
            {
                if (parser.FileContents.Length <= 0)
                {
                    //check the file size if it is 0 bytes then return client with that error code.
                    return BadRequest();
                }

                var ext = Path.GetExtension(parser.Filename);
                var uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;

                var fileName = string.Format("{0}_{1}", AppUser.Id, uniqueName);

                using (Stream stream = new MemoryStream(parser.FileContents))
                {
                    await _storage.Upload(bucketName, fileName, stream);
                }

                var url = _storage.GetShareLink(bucketName, fileName, DateTime.UtcNow.AddYears(100));

                //return content type.
                return Ok(url);
            }

            //this request invalid because there is no file, return fail code to the client.
            return NotFound();
        }

        [HttpPost("upload_template")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadTemplate()
        {
            var parser = new HttpMultipartParser(Request.Body, "file");
            StringValues bucketName = UnifiedStorage.GetPath("template", OrganizationId, (int)AppId);

            //if it is successfully parsed continue.
            if (parser.Success)
            {
                if (parser.FileContents.Length <= 0)
                {
                    //check the file size if it is 0 bytes then return client with that error code.
                    return BadRequest();
                }

                var ext = Path.GetExtension(parser.Filename);
                var uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;

                using (Stream stream = new MemoryStream(parser.FileContents))
                {
                    await _storage.Upload(bucketName, uniqueName, stream);
                }

                var result = new DocumentUploadResult
                {
                    ContentType = parser.ContentType,
                    UniqueName = uniqueName,
                    Chunks = 0
                };

                //return content type of the file to the client
                return Ok(result);
            }

            //this request invalid because there is no file, return fail code to the client.
            return NotFound();
        }

        //[Route("download")]
        //public async Task<FileStreamResult> Download([FromQuery(Name = "fileId")] int fileId)
        //{
        //    var doc = await _documentRepository.GetById(fileId);
        //    if (doc != null)
        //    {
        //        return await _storage.Download(UnifiedStorage.GetPath("attachment", AppUser.TenantId), doc.UniqueName, doc.Name);
        //    }
        //    else
        //    {
        //        //there is no such file, return
        //        throw new Exception("Document does not exist in the storage!");
        //    }
        //}

        ///// <summary>
        ///// Validates and creates document record permanently after temporary upload process completed.
        ///// </summary>
        ///// <param name="document">The document.</param>
        //[Route("create"), HttpPost]
        //public async Task<IActionResult> Create([FromBody] DocumentDTO document)
        //{
        //    //get entity name if this document is uploading to a specific entity.
        //    string uniqueStandardizedName = document.FileName.Replace(" ", "-");

        //    uniqueStandardizedName = Regex.Replace(uniqueStandardizedName, @"[^\u0000-\u007F]", string.Empty);

        //    Document currentDoc = new Document()
        //    {
        //        FileSize = document.FileSize,
        //        Description = document.Description,
        //        ModuleId = document.ModuleId,
        //        Name = document.FileName,
        //        CreatedAt = DateTime.UtcNow,
        //        Type = document.MimeType,
        //        UniqueName = document.UniqueFileName,
        //        RecordId = document.RecordId,
        //        Deleted = false
        //    };
        //    if (await _documentRepository.CreateAsync(currentDoc) != null)
        //    {
        //        //transfer file to the permanent storage by committing it.
        //        return Ok(currentDoc.Id.ToString());
        //    }

        //    return BadRequest("Couldn't Create Document!");
        //}

        [Route("upload_logo"), HttpPost]
        public async Task<IActionResult> UploadLogo()
        {
            HttpMultipartParser parser = new HttpMultipartParser(Request.Body, "file");
            StringValues bucketName = UnifiedStorage.GetPath("applogo", OrganizationId, (int)AppId);

            if (parser.Success)
            {
                //if succesfully parsed, then continue to thread.
                if (parser.FileContents.Length <= 0)
                {
                    //if file is invalid, then stop thread and return bad request status code.
                    return BadRequest();
                }

                var uniqueName = string.Empty;
                //get the file name from parser
                if (parser.Parameters.ContainsKey("name"))
                {
                    uniqueName = parser.Parameters["name"];
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid() + ext;
                }

                var fileName = string.Format("{0}_{1}", AppUser.Id, uniqueName);

                using (Stream stream = new MemoryStream(parser.FileContents))
                {
                    await _storage.Upload(bucketName, fileName, stream);
                }

                var logo = _storage.GetShareLink(bucketName, fileName, DateTime.UtcNow.AddYears(100));

                //return content type.
                return Ok(logo);
            }

            //this is not a valid request so return fail.
            return Ok("Fail");
        }

        //[Route("upload_import_excel"), HttpPost]
        //public async Task<IActionResult> ImportSaveExcel([FromQuery(Name = "import_id")] int importId)
        //{
        //    var import = await _importRepository.GetById(importId);

        //    if (import == null)
        //        return NotFound();

        //    var parser = new HttpMultipartParser(Request.Body, "file");
        //    StringValues bucketName = UnifiedStorage.GetPath("import", AppUser.TenantId);

        //    //if it is successfully parsed continue.
        //    if (parser.Success)
        //    {
        //        if (parser.FileContents.Length <= 0)
        //        {
        //            //check the file size if it is 0 bytes then return client with that error code.
        //            return BadRequest();
        //        }

        //        var ext = Path.GetExtension(parser.Filename);
        //        var fileName = Guid.NewGuid().ToString().Replace("-", "") + ext;

        //        using (Stream stream = new MemoryStream(parser.FileContents))
        //        {
        //            await _storage.Upload(bucketName, fileName, stream);
        //        }

        //        var excelUrl = _storage.GetShareLink(bucketName, fileName, DateTime.UtcNow.AddYears(100), Amazon.S3.Protocol.HTTP, false);
        //        excelUrl = excelUrl + "--" + parser.Filename;

        //        import.ExcelUrl = excelUrl;
        //        await _importRepository.Update(import);

        //        //return content type of the file to the client
        //        return Ok(parser.Filename);
        //    }

        //    //this request invalid because there is no file, return fail code to the client.
        //    return NotFound();
        //}
    }
}