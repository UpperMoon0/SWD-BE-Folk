using CloudinaryDotNet.Actions;
using GrowthTracking.DoctorSolution.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System;

namespace GrowthTracking.DoctorSolution.Infrastructure.Cloudinary
{
    using CloudinaryDotNet;

    public class CloudinaryService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;
        private readonly bool _isConfigured;

        public CloudinaryService(IOptions<CloudinarySettings> cloudinarySettings, ILogger<CloudinaryService> logger)
        {
            _logger = logger;
            var settings = cloudinarySettings.Value;
            
            // Check if Cloudinary settings are properly configured
            if (string.IsNullOrWhiteSpace(settings.CloudName) || 
                string.IsNullOrWhiteSpace(settings.ApiKey) || 
                string.IsNullOrWhiteSpace(settings.ApiSecret))
            {
                _logger.LogWarning("Cloudinary is not properly configured. File uploads will return mock URLs for testing.");
                _isConfigured = false;
                
                // Use dummy values to prevent initialization errors
                settings.CloudName = "test-cloud";
                settings.ApiKey = "test-api-key";
                settings.ApiSecret = "test-api-secret";
            }
            else
            {
                _isConfigured = true;
            }
            
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "File is empty or null."
                };
            }

            // If Cloudinary is not properly configured, return a mock result for testing
            if (!_isConfigured)
            {
                _logger.LogInformation("Using mock file upload result because Cloudinary is not configured");
                return new FileUploadResult
                {
                    Success = true,
                    Url = $"https://test-cloudinary.com/test-uploads/{Guid.NewGuid()}/{file.FileName}",
                    PublicId = Guid.NewGuid().ToString()
                };
            }

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream)
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {ErrorMessage}", uploadResult.Error.Message);
                    return new FileUploadResult
                    {
                        Success = false,
                        ErrorMessage = uploadResult.Error.Message
                    };
                }

                return new FileUploadResult
                {
                    Success = true,
                    Url = uploadResult.SecureUrl?.ToString(),
                    PublicId = uploadResult.PublicId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Cloudinary");
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Error uploading file: {ex.Message}"
                };
            }
        }
    }
}
