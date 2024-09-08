namespace FileUpload.Models
{ }
public class FileUploadLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadTime { get; set; }
}
