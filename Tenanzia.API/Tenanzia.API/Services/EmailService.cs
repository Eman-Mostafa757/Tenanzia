using System.Net;
using System.Net.Mail;
using SendGrid.Helpers.Mail;
using SendGrid;
namespace Tenanzia.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendInvoiceEmail(
    string toEmail, string toName, int invoiceId,
    int orderId, decimal amount, string status,
    string companyName,
    List<(string ProductName, int Quantity, decimal UnitPrice, decimal TotalPrice)> items)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(_config["SendGrid:FromEmail"], companyName);
            var to = new EmailAddress(toEmail, toName);
            var subject = $"Invoice #{invoiceId} from {companyName} — ${amount:N0}";

            var htmlContent = $@"
    <!DOCTYPE html>
    <html>
    <head>
      <style>
        body {{ font-family: Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; }}
        .header {{ background: #D4537E; padding: 30px; text-align: center; }}
        .header h1 {{ color: white; margin: 0; font-size: 24px; }}
        .header p {{ color: #FBEAF0; margin: 5px 0 0; font-size: 14px; }}
        .body {{ padding: 30px; }}
        .invoice-box {{ background: #f9f9f9; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .invoice-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }}
        .invoice-row:last-child {{ border-bottom: none; }}
        .label {{ color: #666; font-size: 14px; }}
        .value {{ color: #333; font-size: 14px; font-weight: 500; }}
        .items-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
        .items-table th {{ background: #f0f0f0; padding: 10px; text-align: left; font-size: 12px; color: #666; text-transform: uppercase; }}
        .items-table td {{ padding: 10px; border-bottom: 1px solid #eee; font-size: 14px; color: #333; }}
        .total-row {{ background: #FEF2F6; }}
        .total-row td {{ font-weight: bold; color: #D4537E; font-size: 16px; }}
        .status-paid {{ background: #D1FAE5; color: #065F46; padding: 4px 12px; border-radius: 20px; font-size: 12px; }}
        .status-unpaid {{ background: #FEF3C7; color: #92400E; padding: 4px 12px; border-radius: 20px; font-size: 12px; }}
        .footer {{ background: #D4537E; padding: 20px; text-align: center; }}
        .footer p {{ color: white; margin: 0; font-size: 13px; }}
      </style>
    </head>
    <body>
      <div class='container'>
        <div class='header'>
          <h1>Tenanzia</h1>
          <p>Invoice from {companyName}</p>
        </div>
        <div class='body'>
          <p style='font-size: 16px; color: #333;'>Dear {toName},</p>
          <p style='color: #666; font-size: 14px;'>
            You have received an invoice from <strong>{companyName}</strong>.
          </p>

          <!-- Invoice Info -->
          <div class='invoice-box'>
            <div class='invoice-row'>
              <span class='label'>Invoice #</span>
              <span class='value'>#{invoiceId}</span>
            </div>
            <div class='invoice-row'>
              <span class='label'>Order #</span>
              <span class='value'>#{orderId}</span>
            </div>
            <div class='invoice-row'>
              <span class='label'>Status</span>
              <span class='value'>
                <span class='{(status == "Paid" ? "status-paid" : "status-unpaid")}'>
                  {status}
                </span>
              </span>
            </div>
          </div>

          <!-- Items Table -->
          <p style='font-weight: 500; color: #333; margin-bottom: 8px;'>Order Items</p>
          <table class='items-table'>
            <thead>
              <tr>
                <th>Product</th>
                <th>Qty</th>
                <th>Unit Price</th>
                <th>Total</th>
              </tr>
            </thead>
            <tbody>
              {string.Join("", items.Select(i => $@"
              <tr>
                <td>{i.ProductName}</td>
                <td>{i.Quantity}</td>
                <td>${i.UnitPrice:N0}</td>
                <td>${i.TotalPrice:N0}</td>
              </tr>"))}
              <tr class='total-row'>
                <td colspan='3'>Total</td>
                <td>${amount:N0}</td>
              </tr>
            </tbody>
          </table>

          <p style='color: #666; font-size: 13px; text-align: center; margin-top: 20px;'>
            If you have any questions, please contact {companyName}.
          </p>
        </div>
        <div class='footer'>
          <p>Thank you for your business!</p>
          <p style='margin-top: 5px; font-size: 12px; color: #FBEAF0;'>
            Powered by Tenanzia
          </p>
        </div>
      </div>
    </body>
    </html>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
            await client.SendEmailAsync(msg);
        }
    }
}
