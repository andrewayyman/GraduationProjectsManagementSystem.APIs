namespace Graduation_Project_Management.Utilities
{
    public static class DocumentSetting
    {
        public static async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Files", folderName);
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
            string fileName = $"{Guid.NewGuid}-{file.FileName}";
            string FilePath = Path.Combine(FolderPath, fileName);
            using var stream = new FileStream(FilePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }
        public static void DeleteFile(string folderName, string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Files", folderName, fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
