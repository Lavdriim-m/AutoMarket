using AutoMarket.Core.DTOs.Vehicles;

namespace AutoMarket.Core.Interfaces;

public interface INhtsaService
{
    /// <summary>Decodes a VIN using the NHTSA vPIC API and returns decoded vehicle info.</summary>
    Task<VinDecodeResultDto> DecodeVinAsync(string vin);
}
