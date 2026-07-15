namespace TubieTools_PublicAPI.Services
{
    using TubieTools_PublicAPI.Models;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of care provider service with JSON file persistence
    /// </summary>
    public class CareProviderService : ICareProviderService
    {
        private readonly ILogger<CareProviderService> _logger;
        private readonly string _configPath;
        private readonly string _demoAccountPath;
        private List<CareProvider> _allProviders;

        public CareProviderService(ILogger<CareProviderService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configPath = Path.Combine(Directory.GetCurrentDirectory(), "Config");
            _demoAccountPath = Path.Combine(_configPath, "demo_account.json");

            // Ensure config directory exists
            if (!Directory.Exists(_configPath))
            {
                Directory.CreateDirectory(_configPath);
            }

            _allProviders = new List<CareProvider>();
        }

        /// <summary>
        /// Load all providers from JSON files for the specified tier
        /// </summary>
        private async Task LoadProvidersAsync()
        {
            _allProviders.Clear();

            try
            {
                // Load DayCare providers
                var dayCareProviders = await LoadProvidersFromFileAsync("daycare_providers.json");
                _allProviders.AddRange(dayCareProviders);

                // Load ElderlyHome providers
                var elderlyProviders = await LoadProvidersFromFileAsync("elderly_home_providers.json");
                _allProviders.AddRange(elderlyProviders);

                // Load HealthcareProvider providers
                var healthcareProviders = await LoadProvidersFromFileAsync("healthcare_provider_providers.json");
                _allProviders.AddRange(healthcareProviders);

                _logger.LogInformation($"Loaded {_allProviders.Count} providers from configuration files");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading providers: {ex.Message}");
            }
        }

        /// <summary>
        /// Load providers from a specific JSON file
        /// </summary>
        private async Task<List<CareProvider>> LoadProvidersFromFileAsync(string fileName)
        {
            var filePath = Path.Combine(_configPath, fileName);
            var providers = new List<CareProvider>();

            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"Provider file not found: {filePath}");
                return providers;
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<ProviderFileData>(json, options);

                if (data?.CareProviders != null)
                {
                    providers.AddRange(data.CareProviders);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading providers from {fileName}: {ex.Message}");
            }

            return providers;
        }

        /// <summary>
        /// Save providers to JSON file by tier
        /// </summary>
        private async Task SaveProvidersAsync()
        {
            try
            {
                // Group providers by tier
                var groupedByTier = _allProviders.GroupBy(p => p.Tier);

                foreach (var tierGroup in groupedByTier)
                {
                    var tierFileName = GetFileNameForTier(tierGroup.Key);
                    var filePath = Path.Combine(_configPath, tierFileName);

                    var data = new ProviderFileData
                    {
                        CareProviders = tierGroup.ToList()
                    };

                    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(filePath, json);

                    _logger.LogInformation($"Saved {tierGroup.Count()} providers to {tierFileName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving providers: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the JSON file name for a provider tier
        /// </summary>
        private string GetFileNameForTier(CareProviderTier tier)
        {
            return tier switch
            {
                CareProviderTier.DayCare => "daycare_providers.json",
                CareProviderTier.ElderlyHome => "elderly_home_providers.json",
                CareProviderTier.HealthcareProvider => "healthcare_provider_providers.json",
                _ => "providers.json"
            };
        }

        public async Task<List<CareProvider>> GetAllProvidersAsync(CareProviderTier? tier = null, string status = null)
        {
            await LoadProvidersAsync();

            var result = _allProviders.AsEnumerable();

            if (tier.HasValue)
            {
                result = result.Where(p => p.Tier == tier.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                result = result.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return result.ToList();
        }

        public async Task<CareProvider> GetProviderByIdAsync(string providerId)
        {
            await LoadProvidersAsync();
            return _allProviders.FirstOrDefault(p => p.ProviderId == providerId);
        }

        public async Task<List<CareProvider>> GetProvidersByTierAsync(CareProviderTier tier)
        {
            return await GetAllProvidersAsync(tier);
        }

        public async Task<CareProvider> CreateProviderAsync(Models.Requests.CreateCareProviderRequest request)
        {
            await LoadProvidersAsync();

            // Validate provider doesn't already exist
            var generatedId = GenerateProviderId(request.ProviderName, request.Tier);
            if (_allProviders.Any(p => p.ProviderId == generatedId))
            {
                throw new InvalidOperationException($"Provider with ID {generatedId} already exists");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.ProviderName))
                throw new ArgumentException("Provider name is required");

            if (request.PrimaryContact == null)
                throw new ArgumentException("Primary contact is required");

            if (request.BillingAddress == null)
                throw new ArgumentException("Billing address is required");

            var provider = new CareProvider
            {
                ProviderId = generatedId,
                ProviderName = request.ProviderName,
                Tier = request.Tier,
                TaxId = request.TaxId,
                Status = "Active",
                AnnualOrderVolume = request.AnnualOrderVolume,
                PrimaryContact = request.PrimaryContact,
                SecondaryContact = request.SecondaryContact,
                BillingAddress = request.BillingAddress,
                ShippingAddress = request.ShippingAddress ?? request.BillingAddress,
                PaymentConfiguration = request.PaymentConfiguration ?? new PaymentConfiguration
                {
                    PaymentFrequency = PaymentFrequency.Monthly,
                    DiscountPercentage = 0,
                    AutoRenew = true,
                    NextBillingDate = DateTime.UtcNow.AddMonths(1)
                },
                InternalNotes = request.InternalNotes,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _allProviders.Add(provider);
            await SaveProvidersAsync();

            _logger.LogInformation($"Created new provider: {provider.ProviderId}");
            return provider;
        }

        public async Task<CareProvider> UpdateProviderAsync(string providerId, Models.Requests.UpdateCareProviderRequest request)
        {
            await LoadProvidersAsync();

            var provider = _allProviders.FirstOrDefault(p => p.ProviderId == providerId);
            if (provider == null)
                throw new KeyNotFoundException($"Provider {providerId} not found");

            // Update fields
            if (!string.IsNullOrWhiteSpace(request.ProviderName))
                provider.ProviderName = request.ProviderName;

            if (request.AnnualOrderVolume > 0)
                provider.AnnualOrderVolume = request.AnnualOrderVolume;

            if (request.PrimaryContact != null)
                provider.PrimaryContact = request.PrimaryContact;

            if (request.SecondaryContact != null)
                provider.SecondaryContact = request.SecondaryContact;

            if (request.BillingAddress != null)
                provider.BillingAddress = request.BillingAddress;

            if (request.ShippingAddress != null)
                provider.ShippingAddress = request.ShippingAddress;

            if (request.PaymentConfiguration != null)
                provider.PaymentConfiguration = request.PaymentConfiguration;

            if (!string.IsNullOrWhiteSpace(request.InternalNotes))
                provider.InternalNotes = request.InternalNotes;

            if (!string.IsNullOrWhiteSpace(request.Status))
                provider.Status = request.Status;

            provider.ModifiedDate = DateTime.UtcNow;

            await SaveProvidersAsync();

            _logger.LogInformation($"Updated provider: {provider.ProviderId}");
            return provider;
        }

        public async Task<bool> DeleteProviderAsync(string providerId)
        {
            await LoadProvidersAsync();

            var provider = _allProviders.FirstOrDefault(p => p.ProviderId == providerId);
            if (provider == null)
                throw new KeyNotFoundException($"Provider {providerId} not found");

            // Soft delete: change status to Inactive
            provider.Status = "Inactive";
            provider.ModifiedDate = DateTime.UtcNow;

            await SaveProvidersAsync();

            _logger.LogInformation($"Deleted provider: {providerId}");
            return true;
        }

        public async Task<List<CareProvider>> GetActiveProvidersAsync()
        {
            return await GetAllProvidersAsync(null, "Active");
        }

        public async Task<List<CareProvider>> SearchProvidersByNameAsync(string searchTerm)
        {
            await LoadProvidersAsync();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return _allProviders;

            return _allProviders
                .Where(p => p.ProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Generate a provider ID based on name and tier
        /// </summary>
        private string GenerateProviderId(string providerName, CareProviderTier tier)
        {
            var tierPrefix = tier switch
            {
                CareProviderTier.DayCare => "daycare",
                CareProviderTier.ElderlyHome => "elderly",
                CareProviderTier.HealthcareProvider => "healthcare",
                _ => "provider"
            };

            var nameSlug = string.Concat(providerName.Where(c => !char.IsWhiteSpace(c))).ToLower();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return $"{tierPrefix}-{nameSlug}-{timestamp}";
        }
    }

    /// <summary>
    /// Helper class for JSON serialization/deserialization
    /// </summary>
    public class ProviderFileData
    {
        public List<CareProvider> CareProviders { get; set; }
    }
}
