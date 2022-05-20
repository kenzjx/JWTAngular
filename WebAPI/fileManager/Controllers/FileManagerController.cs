using fileManager.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace fileManager.Controllers
{
    [ApiController]
    [EnableCors("MyPolicy")]
    [Route("[controller]")]
    public class FileManagerController : ControllerBase
    {
        private readonly string AppDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private static List<FileRecord> fileDB = new List<FileRecord>();
        OfficeDBContext dBContext = new OfficeDBContext();

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<HttpResponseMessage> PostAsync([FromForm] FileModel model)
        {
            try
            {
                FileRecord file = await SaveFileAsync(model.MyFile);

                if (!string.IsNullOrEmpty(file.FilePath))
                {
                    file.AltText = model.AltText;
                    file.Description = model.Description;
                    //Save to Inmemory object
                    //fileDB.Add(file);
                    //Save to SQL Server DB
                    SaveToDB(file);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                };
            }
        }

        private async Task<FileRecord> SaveFileAsync(IFormFile myFile)
        {
            FileRecord file = new FileRecord();
            if (myFile != null)
            {
                if (!Directory.Exists(AppDirectory))
                    Directory.CreateDirectory(AppDirectory);

                var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(myFile.FileName);
                var path = Path.Combine(AppDirectory, fileName);

                file.Id = fileDB.Count() + 1;
                file.FilePath = path;
                file.FileName = fileName;
                file.FileFormat = Path.GetExtension(myFile.FileName);
                file.ContentType = myFile.ContentType;

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await myFile.CopyToAsync(stream);
                }

                return file;
            }
            return file;
        }
        
        private void SaveToDB(FileRecord record)
        {
            if (record == null)
                throw new ArgumentNullException($"{nameof(record)}");

            FileData fileData = new FileData();
            fileData.FilePath = record.FilePath;
            fileData.FileName = record.FileName;
            fileData.FileExtension = record.FileFormat;
            fileData.MimeType = record.ContentType;

            dBContext.FileData.Add(fileData);
            dBContext.SaveChanges();
        }

        [HttpGet]
        public List<FileRecord> GetAllFiles()
        {
            //getting data from inmemory obj
            //return fileDB;
            //getting data from SQL DB
            return dBContext.FileData.Select(n => new FileRecord { 
                Id = n.Id, 
                ContentType = n.MimeType, 
                FileFormat = n.FileExtension, 
                FileName = n.FileName, 
                FilePath = n.FilePath 
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            if (!Directory.Exists(AppDirectory))
                Directory.CreateDirectory(AppDirectory);

            //getting file from inmemory obj
            //var file = fileDB?.Where(n => n.Id == id).FirstOrDefault();
            //getting file from DB
            var file = dBContext.FileData.Where(n => n.Id == id).FirstOrDefault();

            var path = Path.Combine(AppDirectory, file?.FilePath);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var contentType = "APPLICATION/octet-stream";
            var fileName = Path.GetFileName(path);

            return File(memory, contentType, fileName);
        }
          public (string fileType, byte[] archiveData, string archiveName) DownloadFiles(string subDirectory)  
        {  
            var zipName = $"archive-{DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss")}.zip";  
  
            var files = Directory.GetFiles(Path.Combine(_hostingEnvironment.ContentRootPath, subDirectory)).ToList();  
  
            using (var memoryStream = new MemoryStream())  
            {  
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))  
                {  
                    files.ForEach(file =>  
                    {  
                        var theFile = archive.CreateEntry(file);  
                        using (var streamWriter = new StreamWriter(theFile.Open()))  
                        {  
                            streamWriter.Write(File.ReadAllText(file));  
                        }  
  
                    });  
                }  
  
                return ("application/zip", memoryStream.ToArray(), zipName);  
            }  
  
        }
        #region Size Converter  
        public string SizeConverter(long bytes)  
        {  
            var fileSize = new decimal(bytes);  
            var kilobyte = new decimal(1024);  
            var megabyte = new decimal(1024 * 1024);  
            var gigabyte = new decimal(1024 * 1024 * 1024);  
  
            switch (fileSize)  
            {  
                case var _ when fileSize < kilobyte:  
                    return $"Less then 1KB";  
                case var _ when fileSize < megabyte:  
                    return $"{Math.Round(fileSize / kilobyte, 0, MidpointRounding.AwayFromZero):##,###.##}KB";  
                case var _ when fileSize < gigabyte:  
                    return $"{Math.Round(fileSize / megabyte, 2, MidpointRounding.AwayFromZero):##,###.##}MB";  
                case var _ when fileSize >= gigabyte:  
                    return $"{Math.Round(fileSize / gigabyte, 2, MidpointRounding.AwayFromZero):##,###.##}GB";  
                default:  
                    return "n/a";  
            }  
        }  
        #endregion  
  
    }  
    }

}
