using Models;

namespace Services.Interfaces;

public interface IPdfService
{
    byte[] GenerateBudgetPdf(Budget budget, string companyName);
}
