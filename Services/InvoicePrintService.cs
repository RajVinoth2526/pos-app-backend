using ClientAppPOSWebAPI.Models;
using System.Diagnostics;
using System.Text;

namespace ClientAppPOSWebAPI.Services
{
    public class InvoicePrintService
    {
        private readonly InvoiceService _invoiceService;
        private readonly ILogger<InvoicePrintService> _logger;

        public InvoicePrintService(InvoiceService invoiceService, ILogger<InvoicePrintService> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        public async Task<bool> PrintInvoiceAsync(int invoiceId, PrintOptions? options = null)
        {
            try
            {
                options ??= new PrintOptions();

                // Generate HTML content
                var htmlContent = await _invoiceService.GenerateInvoiceHtmlAsync(invoiceId);
                
                // Create print-optimized HTML
                var printHtml = GeneratePrintHtml(htmlContent, options);
                
                // Save HTML to temporary file
                var tempFile = Path.GetTempFileName() + ".html";
                await File.WriteAllTextAsync(tempFile, printHtml, Encoding.UTF8);
                
                // Print using default browser
                var success = await PrintWithBrowserAsync(tempFile, options);
                
                // Clean up temporary file
                try
                {
                    File.Delete(tempFile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to delete temporary file {tempFile}: {ex.Message}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error printing invoice {invoiceId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PrintInvoiceToDefaultPrinterAsync(int invoiceId, PrintOptions? options = null)
        {
            try
            {
                options ??= new PrintOptions();

                // Generate HTML content
                var htmlContent = await _invoiceService.GenerateInvoiceHtmlAsync(invoiceId);
                
                // Create print-optimized HTML
                var printHtml = GeneratePrintHtml(htmlContent, options);
                
                // Save HTML to temporary file
                var tempFile = Path.GetTempFileName() + ".html";
                await File.WriteAllTextAsync(tempFile, printHtml, Encoding.UTF8);
                
                // Print using system default printer
                var success = await PrintWithSystemPrinterAsync(tempFile, options);
                
                // Clean up temporary file
                try
                {
                    File.Delete(tempFile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to delete temporary file {tempFile}: {ex.Message}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error printing invoice {invoiceId} to system printer: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> PrintWithBrowserAsync(string htmlFile, PrintOptions options)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = htmlFile,
                    UseShellExecute = true,
                    Verb = "print"
                };

                var process = Process.Start(processInfo);
                if (process != null)
                {
                    // Wait a moment for the print dialog to appear
                    await Task.Delay(1000);
                    
                    // Don't wait for process to complete as it might stay open
                    _logger.LogInformation($"Print job initiated for {htmlFile}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error printing with browser: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> PrintWithSystemPrinterAsync(string htmlFile, PrintOptions options)
        {
            try
            {
                // Try to use the default browser's print functionality
                var browserPath = GetDefaultBrowserPath();
                if (!string.IsNullOrEmpty(browserPath))
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = browserPath,
                        Arguments = $"--headless --disable-gpu --print-to-pdf=\"{Path.GetTempFileName()}.pdf\" \"{htmlFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        _logger.LogInformation("PDF generated for printing");
                        return process.ExitCode == 0;
                    }
                }

                // Fallback to opening in default browser
                return await PrintWithBrowserAsync(htmlFile, options);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error printing with system printer: {ex.Message}");
                return false;
            }
        }

        private string GetDefaultBrowserPath()
        {
            try
            {
                // Try common browser paths
                var possiblePaths = new[]
                {
                    @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                    @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                    @"C:\Program Files\Mozilla Firefox\firefox.exe",
                    @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe",
                    @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                    @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
                };

                return possiblePaths.FirstOrDefault(File.Exists) ?? "";
            }
            catch
            {
                return "";
            }
        }

        private string GeneratePrintHtml(string invoiceHtml, PrintOptions options)
        {
            var printCss = GetPrintCss(options);
            
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Print Invoice</title>
    <style>
        {printCss}
    </style>
    <script>
        // Auto-print when page loads
        window.onload = function() {{
            setTimeout(function() {{
                window.print();
                if ({options.CloseAfterPrint.ToString().ToLower()}) {{
                    setTimeout(function() {{
                        window.close();
                    }}, 1000);
                }}
            }}, 500);
        }};
        
        // Handle print events
        window.onafterprint = function() {{
            if ({options.CloseAfterPrint.ToString().ToLower()}) {{
                window.close();
            }}
        }};
        
        // Manual print function
        function printInvoice() {{
            window.print();
        }}
        
        // Close window function
        function closeWindow() {{
            window.close();
        }}
    </script>
</head>
<body>
    <div class='print-controls' id='printControls'>
        <button onclick='printInvoice()'>🖨️ Print Invoice</button>
        <button onclick='closeWindow()'>❌ Close</button>
    </div>
    
    {invoiceHtml}
</body>
</html>";
        }

        private string GetPrintCss(PrintOptions options)
        {
            return $@"
        body {{
            font-family: 'Arial', sans-serif;
            margin: 0;
            padding: 20px;
            background-color: white;
            color: #333;
        }}
        
        .print-controls {{
            text-align: center;
            margin: 20px 0;
            padding: 20px;
            background-color: #f0f0f0;
            border-radius: 8px;
        }}
        
        .print-controls button {{
            background-color: #007bff;
            color: white;
            border: none;
            padding: 10px 20px;
            font-size: 16px;
            border-radius: 5px;
            cursor: pointer;
            margin: 0 10px;
        }}
        
        .print-controls button:hover {{
            background-color: #0056b3;
        }}
        
        @media print {{
            body {{ 
                background-color: white; 
                margin: 0;
                padding: 0;
            }}
            
            .print-controls {{ 
                display: none !important; 
            }}
            
            @page {{ 
                margin: {(options.PageMargin ?? "0.5in")}; 
                size: {(options.PageSize ?? "A4")};
            }}
            
            .invoice-header, .customer-info, .invoice-items, .invoice-totals, .invoice-footer {{
                box-shadow: none;
                border: 1px solid #ddd;
                page-break-inside: avoid;
            }}
            
            .invoice-items {{
                page-break-inside: avoid;
            }}
            
            .invoice-items tr {{
                page-break-inside: avoid;
            }}
            
            .invoice-footer {{
                page-break-after: avoid;
            }}
        }}
        
        @media screen {{
            .print-controls {{
                display: block;
            }}
        }}
    ";
        }

        public async Task<string> GenerateInvoicePdfAsync(int invoiceId, PrintOptions? options = null)
        {
            try
            {
                options ??= new PrintOptions();

                // Generate HTML content
                var htmlContent = await _invoiceService.GenerateInvoiceHtmlAsync(invoiceId);
                
                // Create print-optimized HTML
                var printHtml = GeneratePrintHtml(htmlContent, options);
                
                // Save HTML to temporary file
                var tempFile = Path.GetTempFileName() + ".html";
                await File.WriteAllTextAsync(tempFile, printHtml, Encoding.UTF8);
                
                // Generate PDF using browser
                var pdfFile = Path.GetTempFileName() + ".pdf";
                var browserPath = GetDefaultBrowserPath();
                
                if (!string.IsNullOrEmpty(browserPath))
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = browserPath,
                        Arguments = $"--headless --disable-gpu --print-to-pdf=\"{pdfFile}\" \"{tempFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        
                        // Clean up HTML file
                        try { File.Delete(tempFile); } catch { }
                        
                        if (process.ExitCode == 0 && File.Exists(pdfFile))
                        {
                            return pdfFile;
                        }
                    }
                }
                
                // Clean up files
                try { File.Delete(tempFile); } catch { }
                try { File.Delete(pdfFile); } catch { }
                
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating PDF for invoice {invoiceId}: {ex.Message}");
                return "";
            }
        }
    }

    public class PrintOptions
    {
        public string? PageSize { get; set; } = "A4";
        public string? PageMargin { get; set; } = "0.5in";
        public bool CloseAfterPrint { get; set; } = true;
        public bool AutoPrint { get; set; } = true;
        public string? PrinterName { get; set; }
        public int Copies { get; set; } = 1;
    }
}

