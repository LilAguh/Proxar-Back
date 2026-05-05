using Exceptions;
using Services.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.DTOs.Responses;
using Services.Interfaces;

namespace Services.Implementations;

public class AfipService : IAfipService
{
    private readonly AfipSettings _afipSettings;
    private readonly ILogger<AfipService> _logger;

    public AfipService(IOptions<AfipSettings> afipSettings, ILogger<AfipService> logger)
    {
        _afipSettings = afipSettings.Value;
        _logger = logger;
    }

    public async Task<AfipContribuyenteDto> GetContribuyenteDataAsync(string documento)
    {
        _logger.LogInformation("Consultando datos de AFIP para documento: {Documento}", documento);

        // Validar formato del documento
        var documentoLimpio = documento.Replace("-", "").Replace(" ", "").Trim();

        if (string.IsNullOrEmpty(documentoLimpio) || !long.TryParse(documentoLimpio, out _))
        {
            throw new BusinessRuleException("Documento inválido. Debe ser un CUIT, CUIL o DNI numérico.");
        }

        // Validar longitud (DNI=8 dígitos, CUIT/CUIL=11 dígitos)
        if (documentoLimpio.Length != 8 && documentoLimpio.Length != 11)
        {
            throw new BusinessRuleException("Documento inválido. Debe tener 8 (DNI) u 11 (CUIT/CUIL) dígitos.");
        }

        try
        {
            // TODO: Implementar consulta real a AFIP usando Afip.Net SDK
            // Por ahora retornamos datos de ejemplo para testing

            _logger.LogWarning("⚠️ MODO MOCK: Retornando datos de prueba. Configurar credenciales AFIP en .env");

            return await GetMockDataAsync(documentoLimpio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar datos de AFIP para documento {Documento}", documento);
            throw new BusinessRuleException($"Error al consultar AFIP: {ex.Message}");
        }
    }

    private Task<AfipContribuyenteDto> GetMockDataAsync(string documento)
    {
        // Datos mock para testing mientras no tengamos credenciales
        var tipoDoc = documento.Length == 11 ? "CUIT" : "DNI";

        var mockData = new AfipContribuyenteDto
        {
            Documento = documento.Length == 11
                ? $"{documento[..2]}-{documento.Substring(2, 8)}-{documento[10]}"
                : documento,
            TipoDocumento = tipoDoc,
            RazonSocial = tipoDoc == "CUIT" ? "EMPRESA DE PRUEBA S.R.L." : "JUAN PEREZ",
            CondicionIVA = "Responsable Inscripto",
            CondicionIVACode = 1,
            DomicilioFiscal = "AV. CORRIENTES 1234",
            Localidad = "CORDOBA",
            Provincia = "CORDOBA",
            CodigoPostal = "5000",
            Estado = "ACTIVO",
            Actividades = new List<string> { "Servicios de consultoría" },
            FechaInscripcion = DateTime.UtcNow.AddYears(-5)
        };

        return Task.FromResult(mockData);
    }

    // TODO: Método real de consulta a AFIP
    // private async Task<AfipContribuyenteDto> ConsultarAfipRealAsync(string cuit)
    // {
    //     // Aquí irá la implementación real con Afip.Net
    //     // usando _afipSettings para autenticación
    // }
}
