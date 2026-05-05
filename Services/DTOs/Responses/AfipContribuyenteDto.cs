namespace Services.DTOs.Responses;

public class AfipContribuyenteDto
{
    public string Documento { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty; // CUIT, CUIL, DNI
    public string RazonSocial { get; set; } = string.Empty;
    public string CondicionIVA { get; set; } = string.Empty; // Responsable Inscripto, Monotributista, etc.
    public int? CondicionIVACode { get; set; } // 1=RI, 2=Monotributista, etc.
    public string? DomicilioFiscal { get; set; }
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }
    public string? CodigoPostal { get; set; }
    public string Estado { get; set; } = string.Empty; // ACTIVO, INACTIVO
    public List<string>? Actividades { get; set; }
    public DateTime? FechaInscripcion { get; set; }
}
