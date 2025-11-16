using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ShefaafAPI.Services;

public interface IImageService
{
    Task<string> UploadImage(IFormFile file);
    Task<bool> DeleteImage(string publicId);
}
