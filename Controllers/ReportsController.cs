using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.ViewModels;

namespace Online_Restaurant.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly AppdbContext _context;
        private const int PageSize = 10;

        public ReportsController(AppdbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int? supplierId, int pageNumber = 1)
        {

            var allRecords = await GetFilteredRecords(supplierId);

            int totalCount = allRecords.Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages) pageNumber = totalPages;

            var pageRecords = allRecords
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var viewModel = new SupplierReportViewModel
            {
                Records = pageRecords,
                Suppliers = await _context.Suppliers
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync(),
                SelectedSupplierId = supplierId,
                TotalQuantity = allRecords.Sum(r => r.Quantity),
                TotalCost = allRecords.Sum(r => r.Cost),
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: Reports/ExportExcel?supplierId=3
        [HttpGet]
        public async Task<IActionResult> ExportExcel(int? supplierId)
        {
            var records = await GetFilteredRecords(supplierId);

            string sheetName;
            if (supplierId.HasValue)
            {
                var supplier = await _context.Suppliers.FindAsync(supplierId.Value);
                sheetName = supplier?.CompanyName ?? "Report";
            }
            else
            {
                sheetName = "All Suppliers";
            }

            sheetName = SanitizeSheetName(sheetName);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            // Header row
            worksheet.Cell(1, 1).Value = "Delivery Date";
            worksheet.Cell(1, 2).Value = "Supplier";
            worksheet.Cell(1, 3).Value = "Ingredient";
            worksheet.Cell(1, 4).Value = "Quantity";
            worksheet.Cell(1, 5).Value = "Cost";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#212529");
            headerRow.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var record in records)
            {
                worksheet.Cell(row, 1).Value = record.DeliveryDate;
                worksheet.Cell(row, 1).Style.DateFormat.Format = "dd MMM yyyy";
                worksheet.Cell(row, 2).Value = record.Supplier?.CompanyName ?? "-";
                worksheet.Cell(row, 3).Value = record.Ingredient?.IngredientName ?? "-";
                worksheet.Cell(row, 4).Value = record.Quantity;
                worksheet.Cell(row, 5).Value = record.Cost;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                row++;
            }

            // Totals row
            worksheet.Cell(row, 3).Value = "Total:";
            worksheet.Cell(row, 3).Style.Font.Bold = true;
            worksheet.Cell(row, 4).Value = records.Sum(r => r.Quantity);
            worksheet.Cell(row, 4).Style.Font.Bold = true;
            worksheet.Cell(row, 5).Value = records.Sum(r => r.Cost);
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 5).Style.Font.Bold = true;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"SupplierReport_{sheetName}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task<List<Models.Inventory>> GetFilteredRecords(int? supplierId)
        {
            var query = _context.Inventories
                .Include(i => i.Supplier)
                .Include(i => i.Ingredient)
                .Where(i => i.Quantity > 0)
                .AsQueryable();

            if (supplierId.HasValue)
            {
                query = query.Where(i => i.SupplierId == supplierId.Value);
            }

            return await query
                .OrderByDescending(i => i.DeliveryDate)
                .ToListAsync();
        }

        private static string SanitizeSheetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Report";
            }

            // Excel sheet names can't contain these characters or exceed 31 chars
            foreach (var invalidChar in new[] { '\\', '/', '?', '*', '[', ']', ':' })
            {
                name = name.Replace(invalidChar, '-');
            }

            return name.Length > 31 ? name.Substring(0, 31) : name;
        }
    }
}