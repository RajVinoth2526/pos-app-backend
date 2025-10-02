using ClientAppPOSWebAPI.Data;
using ClientAppPOSWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ClientAppPOSWebAPI.Services
{
    public class InvoiceService
    {
        private readonly POSDbContext _context;

        public InvoiceService(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice?> GetInvoiceByOrderIdAsync(int orderId)
        {
            return await _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.OrderId == orderId);
        }

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Order)
                .Include(i => i.InvoiceItems)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<Invoice> GenerateInvoiceFromOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new InvalidOperationException($"Order with ID {orderId} not found");

            // Check if invoice already exists for this order
            var existingInvoice = await GetInvoiceByOrderIdAsync(orderId);
            if (existingInvoice != null)
                return existingInvoice;

            var invoice = new Invoice
            {
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                OrderId = orderId,
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30), // 30 days payment terms
                Status = "Generated",
                SubTotal = order.SubTotal,
                TaxAmount = order.TaxAmount,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Notes = $"Invoice for Order {order.OrderNumber}",
                CreatedAt = DateTime.Now
            };

            // Create invoice items from order items
            foreach (var orderItem in order.OrderItems)
            {
                var invoiceItem = new InvoiceItem
                {
                    ProductId = orderItem.ProductId,
                    ProductName = orderItem.ProductName,
                    ProductDescription = $"Product ID: {orderItem.ProductId}",
                    UnitPrice = orderItem.UnitPrice,
                    Quantity = orderItem.Quantity,
                    SubTotal = orderItem.SubTotal,
                    TaxAmount = orderItem.TaxAmount,
                    DiscountAmount = orderItem.DiscountAmount,
                    TotalAmount = orderItem.TotalAmount,
                    Notes = orderItem.Notes
                };

                invoice.InvoiceItems.Add(invoiceItem);
            }

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return await GetInvoiceByIdAsync(invoice.Id);
        }

        public async Task<string> GenerateInvoiceHtmlAsync(int invoiceId, InvoiceTemplate? template = null)
        {
            var invoice = await GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
                throw new InvalidOperationException($"Invoice with ID {invoiceId} not found");

            template ??= GetDefaultTemplate();

            var html = new StringBuilder();
            
            // HTML Document Structure
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='en'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>Invoice " + invoice.InvoiceNumber + "</title>");
            html.AppendLine("    <style>");
            html.AppendLine(GetInvoiceCss());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Invoice Header
            html.AppendLine("    <div class='invoice-header'>");
            html.AppendLine("        <div class='company-info'>");
            html.AppendLine($"            <h1>{template.CompanyName}</h1>");
            html.AppendLine($"            <p>{template.CompanyAddress}</p>");
            html.AppendLine($"            <p>Phone: {template.CompanyPhone} | Email: {template.CompanyEmail}</p>");
            if (!string.IsNullOrEmpty(template.TaxNumber))
                html.AppendLine($"            <p>Tax Number: {template.TaxNumber}</p>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='invoice-info'>");
            html.AppendLine($"            <h2>INVOICE #{invoice.InvoiceNumber}</h2>");
            html.AppendLine($"            <p><strong>Date:</strong> {invoice.InvoiceDate:MMMM dd, yyyy}</p>");
            html.AppendLine($"            <p><strong>Due Date:</strong> {invoice.DueDate?.ToString("MMMM dd, yyyy")}</p>");
            html.AppendLine($"            <p><strong>Order #:</strong> {invoice.Order.OrderNumber}</p>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");

            // Customer Information
            html.AppendLine("    <div class='customer-info'>");
            html.AppendLine("        <h3>Bill To:</h3>");
            html.AppendLine($"        <p><strong>{invoice.Order.CustomerName ?? "Customer"}</strong></p>");
            if (!string.IsNullOrEmpty(invoice.Order.CustomerPhone))
                html.AppendLine($"        <p>Phone: {invoice.Order.CustomerPhone}</p>");
            if (!string.IsNullOrEmpty(invoice.Order.CustomerEmail))
                html.AppendLine($"        <p>Email: {invoice.Order.CustomerEmail}</p>");
            html.AppendLine("    </div>");

            // Invoice Items Table
            html.AppendLine("    <table class='invoice-items'>");
            html.AppendLine("        <thead>");
            html.AppendLine("            <tr>");
            html.AppendLine("                <th>Description</th>");
            html.AppendLine("                <th>Quantity</th>");
            html.AppendLine("                <th>Unit Price</th>");
            html.AppendLine("                <th>Amount</th>");
            html.AppendLine("            </tr>");
            html.AppendLine("        </thead>");
            html.AppendLine("        <tbody>");

            foreach (var item in invoice.InvoiceItems)
            {
                html.AppendLine("            <tr>");
                html.AppendLine($"                <td>{item.ProductName}</td>");
                html.AppendLine($"                <td>{item.Quantity}</td>");
                html.AppendLine($"                <td>{item.UnitPrice:C}</td>");
                html.AppendLine($"                <td>{item.TotalAmount:C}</td>");
                html.AppendLine("            </tr>");
            }

            html.AppendLine("        </tbody>");
            html.AppendLine("    </table>");

            // Invoice Totals
            html.AppendLine("    <div class='invoice-totals'>");
            html.AppendLine("        <div class='totals-section'>");
            html.AppendLine($"            <p>Subtotal: <span>{invoice.SubTotal:C}</span></p>");
            if (invoice.TaxAmount > 0)
                html.AppendLine($"            <p>Tax: <span>{invoice.TaxAmount:C}</span></p>");
            if (invoice.DiscountAmount > 0)
                html.AppendLine($"            <p>Discount: <span>-{invoice.DiscountAmount:C}</span></p>");
            html.AppendLine($"            <p class='total'><strong>Total: <span>{invoice.TotalAmount:C}</span></strong></p>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");

            // Footer
            html.AppendLine("    <div class='invoice-footer'>");
            if (!string.IsNullOrEmpty(template.TermsAndConditions))
            {
                html.AppendLine("        <h4>Terms and Conditions:</h4>");
                html.AppendLine($"        <p>{template.TermsAndConditions}</p>");
            }
            if (!string.IsNullOrEmpty(template.FooterText))
            {
                html.AppendLine($"        <p>{template.FooterText}</p>");
            }
            html.AppendLine("        <p class='generated-date'>Generated on: " + DateTime.Now.ToString("MMMM dd, yyyy 'at' h:mm tt") + "</p>");
            html.AppendLine("    </div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"INV-{today:yyyyMMdd}";
            
            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            if (lastInvoice == null)
            {
                return $"{prefix}-001";
            }

            var lastNumber = lastInvoice.InvoiceNumber.Split('-').Last();
            if (int.TryParse(lastNumber, out int number))
            {
                return $"{prefix}-{(number + 1):D3}";
            }

            return $"{prefix}-001";
        }

        private InvoiceTemplate GetDefaultTemplate()
        {
            return new InvoiceTemplate
            {
                CompanyName = "Your POS Store",
                CompanyAddress = "123 Main Street, City, State 12345",
                CompanyPhone = "(555) 123-4567",
                CompanyEmail = "info@yourposstore.com",
                TaxNumber = "TAX-123456789",
                Website = "www.yourposstore.com",
                Currency = "USD",
                FooterText = "Thank you for your business!",
                TermsAndConditions = "Payment is due within 30 days. Late payments may incur additional fees."
            };
        }

        private string GetInvoiceCss()
        {
            return @"
                body {
                    font-family: 'Arial', sans-serif;
                    margin: 0;
                    padding: 20px;
                    background-color: #f5f5f5;
                    color: #333;
                }
                .invoice-header {
                    background-color: white;
                    padding: 30px;
                    border-radius: 8px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                    margin-bottom: 20px;
                    display: flex;
                    justify-content: space-between;
                    align-items: flex-start;
                }
                .company-info h1 {
                    color: #2c3e50;
                    margin: 0 0 10px 0;
                    font-size: 28px;
                }
                .company-info p {
                    margin: 5px 0;
                    color: #666;
                }
                .invoice-info h2 {
                    color: #e74c3c;
                    margin: 0 0 15px 0;
                    font-size: 24px;
                }
                .invoice-info p {
                    margin: 5px 0;
                    text-align: right;
                }
                .customer-info {
                    background-color: white;
                    padding: 20px;
                    border-radius: 8px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                    margin-bottom: 20px;
                }
                .customer-info h3 {
                    color: #2c3e50;
                    margin: 0 0 10px 0;
                }
                .invoice-items {
                    width: 100%;
                    border-collapse: collapse;
                    background-color: white;
                    border-radius: 8px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                    margin-bottom: 20px;
                    overflow: hidden;
                }
                .invoice-items th {
                    background-color: #34495e;
                    color: white;
                    padding: 15px;
                    text-align: left;
                    font-weight: bold;
                }
                .invoice-items td {
                    padding: 15px;
                    border-bottom: 1px solid #ecf0f1;
                }
                .invoice-items tr:nth-child(even) {
                    background-color: #f8f9fa;
                }
                .invoice-totals {
                    background-color: white;
                    padding: 20px;
                    border-radius: 8px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                    margin-bottom: 20px;
                }
                .totals-section {
                    text-align: right;
                }
                .totals-section p {
                    margin: 10px 0;
                    font-size: 16px;
                }
                .totals-section span {
                    display: inline-block;
                    min-width: 100px;
                    text-align: right;
                }
                .total {
                    font-size: 18px !important;
                    border-top: 2px solid #e74c3c;
                    padding-top: 10px;
                    margin-top: 15px;
                }
                .invoice-footer {
                    background-color: white;
                    padding: 20px;
                    border-radius: 8px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                    font-size: 14px;
                    color: #666;
                }
                .invoice-footer h4 {
                    color: #2c3e50;
                    margin: 0 0 10px 0;
                }
                .generated-date {
                    text-align: center;
                    font-style: italic;
                    margin-top: 20px;
                    padding-top: 20px;
                    border-top: 1px solid #ecf0f1;
                }
                @media print {
                    body { background-color: white; }
                    .invoice-header, .customer-info, .invoice-items, .invoice-totals, .invoice-footer {
                        box-shadow: none;
                        border: 1px solid #ddd;
                    }
                }
            ";
        }

        public async Task<bool> UpdateInvoiceStatusAsync(int invoiceId, string status)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
                return false;

            invoice.Status = status;
            invoice.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
                return false;

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

