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
            // �zin verilen uzant�lar� burada tan�mlay�n
            string[] allowedExtensions = { ".txt", ".csv", ".md", ".log" };

            if (!IsValidFileName(file.FileName) || !IsValidFileExtension(file.FileName, allowedExtensions))
            {
                ViewBag.Message = "Ge�ersiz dosya ad� veya uzant�s�.";
                return View("Index");
            }

            if (!IsValidDirectory(targetPath))
            {
                ViewBag.Message = "Ge�ersiz hedef dizin.";
                return View("Index");
            }

            var uniqueFileName = GenerateUniqueFileName(file.FileName, User.Identity.Name);
            var tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uniqueFileName);

            try
            {
                // Dosyay� ge�ici bir konuma kaydet
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Dosyay� hedef dizine ta��
                var destinationPath = Path.Combine(targetPath, uniqueFileName);
                System.IO.File.Move(tempFilePath, destinationPath);

                // Veritaban�na dosya y�kleme bilgilerini kaydet
                var log = new FileUploadLog
                {
                    UserId = User.Identity.Name,
                    FileName = uniqueFileName,
                    FilePath = destinationPath,
                    UploadTime = DateTime.Now
                };

                _context.FileUploadLogs.Add(log);
                await _context.SaveChangesAsync();

                ViewBag.Message = "Dosya ba�ar�yla y�klendi ve ta��nd�.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Dosya i�lenirken bir hata olu�tu: {ex.Message}";
            }
        }
        else
        {
            ViewBag.Message = "Ge�erli bir dosya y�kleyin.";
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

            // K�k dizin ile ba�lamal�
            var rootPath = Path.GetFullPath(Directory.GetCurrentDirectory());

            // Yolu do�rula ve dizinin var olup olmad���n� kontrol et
            if (Directory.Exists(fullPath))
            {
              
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            // Bir hata olu�ursa ge�ersiz olarak i�aretle
            return false;
        }
    }


}
