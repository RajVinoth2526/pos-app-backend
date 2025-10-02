using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Services;
using ClientAppPOSWebAPI.Success;
using Microsoft.AspNetCore.Mvc;

namespace ClientAppPOSWebAPI.Controllers
{
    [Route("api/invoices")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;
        private readonly InvoicePrintService _invoicePrintService;

        public InvoiceController(InvoiceService invoiceService, InvoicePrintService invoicePrintService)
        {
            _invoiceService = invoiceService;
            _invoicePrintService = invoicePrintService;
        }

        // POST: api/invoices/generate/{orderId}
        [HttpPost("generate/{orderId}")]
        public async Task<IActionResult> GenerateInvoice(int orderId)
        {
            try
            {
                var invoice = await _invoiceService.GenerateInvoiceFromOrderAsync(orderId);
                return Ok(Result.SuccessResult(invoice));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(Result.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result.FailureResult($"An error occurred while generating invoice: {ex.Message}"));
            }
        }

        // GET: api/invoices/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);

            if (invoice == null)
            {
                return NotFound(Result.FailureResult("Invoice not found"));
            }

            return Ok(Result.SuccessResult(invoice));
        }

        // GET: api/invoices/order/{orderId}
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetInvoiceByOrder(int orderId)
        {
            var invoice = await _invoiceService.GetInvoiceByOrderIdAsync(orderId);

            if (invoice == null)
            {
                return NotFound(Result.FailureResult("Invoice not found for this order"));
            }

            return Ok(Result.SuccessResult(invoice));
        }

        // GET: api/invoices
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            return Ok(Result.SuccessResult(invoices));
        }

        // GET: api/invoices/{id}/html
        [HttpGet("{id}/html")]
        public async Task<IActionResult> GetInvoiceHtml(int id)
        {
            try
            {
                var html = await _invoiceService.GenerateInvoiceHtmlAsync(id);
                return Content(html, "text/html");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(Result.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result.FailureResult($"An error occurred while generating HTML: {ex.Message}"));
            }
        }

        // POST: api/invoices/{id}/print
        [HttpPost("{id}/print")]
        public async Task<IActionResult> PrintInvoice(int id, [FromBody] InvoicePrintRequest? printRequest = null)
        {
            try
            {
                printRequest ??= new InvoicePrintRequest { InvoiceId = id };

                if (printRequest.Format.ToLower() == "html")
                {
                    var html = await _invoiceService.GenerateInvoiceHtmlAsync(id);
                    return Content(html, "text/html");
                }
                else
                {
                    // For PDF generation, you would integrate with a PDF library like iTextSharp or PuppeteerSharp
                    // For now, return HTML that can be printed
                    var html = await _invoiceService.GenerateInvoiceHtmlAsync(id);
                    var printHtml = GeneratePrintHtml(html);
                    return Content(printHtml, "text/html");
                }
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(Result.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result.FailureResult($"An error occurred while printing invoice: {ex.Message}"));
            }
        }

        // POST: api/invoices/{id}/print-to-printer
        [HttpPost("{id}/print-to-printer")]
        public async Task<IActionResult> PrintInvoiceToPrinter(int id, [FromBody] PrintOptions? options = null)
        {
            try
            {
                var success = await _invoicePrintService.PrintInvoiceAsync(id, options);
                
                if (success)
                {
                    return Ok(Result.SuccessResult("Invoice sent to printer successfully"));
                }
                else
                {
                    return StatusCode(500, Result.FailureResult("Failed to print invoice"));
                }
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(Result.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result.FailureResult($"An error occurred while printing invoice: {ex.Message}"));
            }
        }

        // POST: api/invoices/{id}/print-to-system-printer
        [HttpPost("{id}/print-to-system-printer")]
        public async Task<IActionResult> PrintInvoiceToSystemPrinter(int id, [FromBody] PrintOptions? options = null)
        {
            try
            {
                var success = await _invoicePrintService.PrintInvoiceToDefaultPrinterAsync(id, options);
                
                if (success)
                {
                    return Ok(Result.SuccessResult("Invoice sent to system printer successfully"));
                }
                else
                {
                    return StatusCode(500, Result.FailureResult("Failed to print invoice to system printer"));
                }
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(Result.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result.FailureResult($"An error occurred while printing invoice: {ex.Message}"));
            }
        }

        // POST: api/invoices/{id}/generate-pdf
        [HttpPost("{id}/generate-pdf")]
        public async Task<IActionResult> GenerateInvoicePdf(int id, [FromBody] PrintOptions? options = null)
        {
            try
            {
                var pdfPath = await _invoicePrintService.GenerateInvoicePdfAsync(id, options);
                
                if (!string.IsNullOrEmpty(pdfPath) && System.IO.File.Exists(pdfPath))
                {
                    var pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
                    
                    // Clean up temporary file
                    try { System.IO.File.Delete(pdfPath); } catch { }
                    
                    return base.File(pdfBytes, "application/pdf", $"Invoice_{id}.pdf");
                }
                else
                {
                    return StatusCode(500, Result.FailureResult("Failed to generate PDF"));
                }
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(Result.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result.FailureResult($"An error occurred while generating PDF: {ex.Message}"));
            }
        }

        // PUT: api/invoices/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateInvoiceStatus(int id, [FromBody] string status)
        {
            var success = await _invoiceService.UpdateInvoiceStatusAsync(id, status);

            if (!success)
            {
                return NotFound(Result.FailureResult("Invoice not found"));
            }

            return Ok(Result.SuccessResult("Invoice status updated successfully"));
        }

        // DELETE: api/invoices/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var success = await _invoiceService.DeleteInvoiceAsync(id);

            if (!success)
            {
                return NotFound(Result.FailureResult("Invoice not found"));
            }

            return Ok(Result.SuccessResult("Invoice deleted successfully"));
        }

        private string GeneratePrintHtml(string invoiceHtml)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Print Invoice</title>
    <style>
        @media print {{
            body {{ margin: 0; }}
            .no-print {{ display: none; }}
            @page {{ margin: 0.5in; }}
        }}
        .print-controls {{
            text-align: center;
            margin: 20px 0;
            padding: 20px;
            background-color: #f0f0f0;
        }}
        .print-button {{
            background-color: #007bff;
            color: white;
            border: none;
            padding: 10px 20px;
            font-size: 16px;
            border-radius: 5px;
            cursor: pointer;
            margin: 0 10px;
        }}
        .print-button:hover {{
            background-color: #0056b3;
        }}
    </style>
    <script>
        function printInvoice() {{
            window.print();
        }}
        function downloadAsPDF() {{
            // This would integrate with a PDF generation service
            alert('PDF download functionality would be implemented here');
        }}
    </script>
</head>
<body>
    <div class='print-controls no-print'>
        <button class='print-button' onclick='printInvoice()'>🖨️ Print Invoice</button>
        <button class='print-button' onclick='downloadAsPDF()'>📄 Download PDF</button>
        <button class='print-button' onclick='window.close()'>❌ Close</button>
    </div>
    
    {invoiceHtml}
</body>
</html>";
        }
    }
}
