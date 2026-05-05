using Services.DTOs.Responses;

namespace Services.Interfaces;

public interface IAfipService
{
    /// <summary>
    /// Obtiene los datos fiscales de un contribuyente desde AFIP por CUIT/CUIL/DNI
    /// </summary>
    Task<AfipContribuyenteDto> GetContribuyenteDataAsync(string documento);
}
