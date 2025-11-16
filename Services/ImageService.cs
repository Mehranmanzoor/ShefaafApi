using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ShefaafAPI.Services;

public class ImageService : IImageService
{
    private readonly Cloudinary cloudinary;

    public ImageService(IConfiguration config)
    {
        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
        );
        
        cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImage(IFormFile file)
    {
        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "shefaaf-products",
                Transformation = new Transformation()
                    .Width(800)
                    .Height(800)
                    .Crop("limit")
                    .Quality("auto")
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            
            return uploadResult.SecureUrl.ToString();
        }

        throw new Exception("File is empty");
    }

    public async Task<bool> DeleteImage(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await cloudinary.DestroyAsync(deleteParams);
        
        return result.Result == "ok";
    }
}
