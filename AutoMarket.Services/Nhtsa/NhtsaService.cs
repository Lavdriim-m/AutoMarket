using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMarket.Core.DTOs.Vehicles;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Services.Nhtsa;

public class NhtsaService : INhtsaService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NhtsaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://vpic.nhtsa.dot.gov/api/");
    }

    public async Task<VinDecodeResultDto> DecodeVinAsync(string vin)
    {
        var response = await _httpClient.GetAsync($"vehicles/DecodeVin/{vin}?format=json");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<NhtsaApiResponse>(json, _jsonOptions);

        string Get(string variable) =>
            apiResponse?.Results
                .FirstOrDefault(r => string.Equals(r.Variable, variable, StringComparison.OrdinalIgnoreCase))
                ?.Value ?? string.Empty;

        var make = Get("Make");
        var errorText = Get("Error Text");
        var yearStr = Get("Model Year");

        return new VinDecodeResultDto
        {
            VIN = vin.ToUpperInvariant(),
            Make = make,
            Model = Get("Model"),
            Year = int.TryParse(yearStr, out var year) ? year : 0,
            EngineType = Get("Engine Configuration"),
            FuelType = Get("Fuel Type - Primary"),
            BodyClass = Get("Body Class"),
            IsValid = !string.IsNullOrWhiteSpace(make),
            ErrorText = string.IsNullOrWhiteSpace(errorText) ? null : errorText
        };
    }

    // ── private models for JSON parsing ──────────────────────────────────────

    private sealed class NhtsaApiResponse
    {
        [JsonPropertyName("Results")]
        public List<NhtsaResultItem> Results { get; set; } = new();
    }

    private sealed class NhtsaResultItem
    {
        [JsonPropertyName("Variable")]
        public string Variable { get; set; } = string.Empty;

        [JsonPropertyName("Value")]
        public string? Value { get; set; }
    }
}
