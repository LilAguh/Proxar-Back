using Models;

namespace Services.Interfaces;

public interface IPdfService
{
    Task<string> GenerateBudgetPdfAsync(Budget budget, string companyName);
    byte[] GenerateBudgetPdf(Budget budget, string companyName);
}
