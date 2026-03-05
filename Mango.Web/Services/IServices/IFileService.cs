namespace Mango.Web.Services.IServices
{
    public interface IFileService
    {
        Task<string?> SaveImageAsync(IFormFile? file);
        bool DeleteImage(string? imageUrl);
    }
}
