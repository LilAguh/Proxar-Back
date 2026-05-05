using Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Services.Interfaces;

namespace Services.Implementations;

public class PdfService : IPdfService
{

    public byte[] GenerateBudgetPdf(Budget budget, string companyName)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(Header);
                page.Content().Element(Content);
                page.Footer().Element(Footer);
            });

            // Header
            void Header(IContainer container)
            {
                container.Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(companyName).FontSize(20).Bold();
                        column.Item().Text($"Presupuesto N° {budget.Number:D10}").FontSize(14);
                        column.Item().Text($"Fecha: {budget.CreatedAt:dd/MM/yyyy}").FontSize(10);
                    });

                    row.RelativeItem().AlignRight().Column(column =>
                    {
                        column.Item().Text("PRESUPUESTO").FontSize(16).Bold();
                        column.Item().Text($"Válido hasta: {budget.ValidUntil:dd/MM/yyyy}").FontSize(10);
                    });
                });
            }

            // Content
            void Content(IContainer container)
            {
                container.PaddingVertical(20).Column(column =>
                {
                    column.Spacing(10);

                    // Datos del cliente
                    column.Item().Element(ClientInfo);

                    // Tabla de items
                    column.Item().Element(ItemsTable);

                    // Totales
                    column.Item().Element(Totals);

                    // Notas
                    column.Item().Element(Notes);
                });
            }

            void ClientInfo(IContainer container)
            {
                container.Column(column =>
                {
                    column.Item().Text("CLIENTE").FontSize(12).Bold();
                    column.Item().BorderBottom(1).PaddingBottom(5);
                    column.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"Nombre: {budget.ClientName}");
                            col.Item().Text($"Teléfono: {budget.ClientPhone}");
                        });
                        row.RelativeItem().Column(col =>
                        {
                            if (!string.IsNullOrEmpty(budget.ClientCUIT))
                                col.Item().Text($"CUIT: {budget.ClientCUIT}");
                            if (!string.IsNullOrEmpty(budget.ClientEmail))
                                col.Item().Text($"Email: {budget.ClientEmail}");
                        });
                    });
                    if (!string.IsNullOrEmpty(budget.ClientAddress))
                    {
                        column.Item().Text($"Dirección: {budget.ClientAddress}");
                    }
                });
            }

            void ItemsTable(IContainer container)
            {
                container.Table(table =>
                {
                    // Columnas: Cant | Descripción | %IVA | P.Unit. | Total s/imp | Total
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // Cantidad
                        columns.RelativeColumn(3);    // Descripción
                        columns.ConstantColumn(50);  // %IVA
                        columns.ConstantColumn(80);  // P.Unit
                        columns.ConstantColumn(80);  // Total s/imp
                        columns.ConstantColumn(80);  // Total
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cant.").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Descripción").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("%IVA").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("P.Unit.").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total s/imp").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total").Bold();
                    });

                    // Items
                    foreach (var item in budget.Items)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(item.Quantity.ToString());
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(item.Description);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignCenter().Text($"{item.IVAPercentage:F0}%");
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text($"${item.UnitPrice:N2}");
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text($"${item.Subtotal:N2}");
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text($"${item.Total:N2}");
                    }
                });
            }

            void Totals(IContainer container)
            {
                container.AlignRight().Column(column =>
                {
                    column.Spacing(5);

                    if (budget.Discount > 0)
                    {
                        column.Item().Row(row =>
                        {
                            row.ConstantItem(120).Text("Descuento:");
                            row.ConstantItem(100).AlignRight().Text($"- ${budget.Discount:N2}");
                        });
                    }

                    column.Item().Row(row =>
                    {
                        row.ConstantItem(120).Text("Subtotal:");
                        row.ConstantItem(100).AlignRight().Text($"${budget.Subtotal:N2}");
                    });

                    column.Item().Row(row =>
                    {
                        row.ConstantItem(120).Text("IVA:");
                        row.ConstantItem(100).AlignRight().Text($"${budget.IVAAmount:N2}");
                    });

                    column.Item().BorderTop(2).PaddingTop(5).Row(row =>
                    {
                        row.ConstantItem(120).Text("TOTAL:").FontSize(12).Bold();
                        row.ConstantItem(100).AlignRight().Text($"${budget.Total:N2}").FontSize(12).Bold();
                    });
                });
            }

            void Notes(IContainer container)
            {
                container.PaddingTop(20).Column(column =>
                {
                    column.Item().Text($"Presupuesto válido por {budget.ValidDays} días").Italic().FontSize(9);
                    column.Item().PaddingTop(10).Text("DOCUMENTO NO VÁLIDO COMO FACTURA").Bold().FontSize(9);
                });
            }

            // Footer
            void Footer(IContainer container)
            {
                container.AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            }
        });

        return document.GeneratePdf();
    }
}
