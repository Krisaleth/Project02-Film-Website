using Microsoft.AspNetCore.Hosting;

namespace Project02.Services
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(IFormFile file, string folderWithinWwwroot);
        Task DeleteAsync(string releativePathWithWwwroot);

    }

    public class FileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;

        public FileStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(IFormFile file, string folderWithinWwwroot)
        {
            var targetFolder = Path.Combine(_env.WebRootPath, folderWithinWwwroot);
            Directory.CreateDirectory(targetFolder);

            var filename = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var absPath = Path.Combine(targetFolder, filename);

            await using var fs = new FileStream(absPath, FileMode.Create);
            await file.CopyToAsync(fs);

            return "/" + Path.Combine(folderWithinWwwroot, filename).Replace("\\", "/");
        }

        public Task DeleteAsync(string releativePathWithinWwwroot)
        {
            var absPath = Path.Combine(_env.WebRootPath, releativePathWithinWwwroot.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(absPath)) { File.Delete(absPath); }
            return Task.CompletedTask;
        }
    }
}
