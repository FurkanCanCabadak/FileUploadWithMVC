using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using FileUpload.Data;
using FileUpload.Models;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file, string targetPath)
    {
        if (file != null)
        {
            // Ýzin verilen uzantýlarý burada tanýmlayýn
            string[] allowedExtensions = { ".txt", ".csv", ".md", ".log" };

            if (!IsValidFileName(file.FileName) || !IsValidFileExtension(file.FileName, allowedExtensions))
            {
                ViewBag.Message = "Geçersiz dosya adý veya uzantýsý.";
                return View("Index");
            }

            if (!IsValidDirectory(targetPath))
            {
                ViewBag.Message = "Geçersiz hedef dizin.";
                return View("Index");
            }

            var uniqueFileName = GenerateUniqueFileName(file.FileName, User.Identity.Name);
            var tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uniqueFileName);

            try
            {
                // Dosyayý geçici bir konuma kaydet
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Dosyayý hedef dizine taþý
                var destinationPath = Path.Combine(targetPath, uniqueFileName);
                System.IO.File.Move(tempFilePath, destinationPath);

                // Veritabanýna dosya yükleme bilgilerini kaydet
                var log = new FileUploadLog
                {
                    UserId = User.Identity.Name,
                    FileName = uniqueFileName,
                    FilePath = destinationPath,
                    UploadTime = DateTime.Now
                };

                _context.FileUploadLogs.Add(log);
                await _context.SaveChangesAsync();

                ViewBag.Message = "Dosya baþarýyla yüklendi ve taþýndý.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Dosya iþlenirken bir hata oluþtu: {ex.Message}";
            }
        }
        else
        {
            ViewBag.Message = "Geçerli bir dosya yükleyin.";
        }

        return View("Index");
    }


    private string GenerateUniqueFileName(string originalFileName, string userId)
    {
        var fileExtension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var uniqueId = Guid.NewGuid().ToString();

        var uniqueFileName = $"{userId}_{fileNameWithoutExtension}_{uniqueId}{fileExtension}";
        return uniqueFileName;
    }

    private bool IsValidFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return fileName.All(c => !invalidChars.Contains(c));
    }
    private bool IsValidFileExtension(string fileName, string[] allowedExtensions)
    {
        var fileExtension = Path.GetExtension(fileName).ToLower();
        return allowedExtensions.Contains(fileExtension);
    }

    private bool IsValidDirectory(string path)
    {
        try
        {
            // Mutlak yolu al
            var fullPath = Path.GetFullPath(path);

            // Kök dizin ile baþlamalý
            var rootPath = Path.GetFullPath(Directory.GetCurrentDirectory());

            // Yolu doðrula ve dizinin var olup olmadýðýný kontrol et
            if (Directory.Exists(fullPath))
            {
              
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            // Bir hata oluþursa geçersiz olarak iþaretle
            return false;
        }
    }


}
