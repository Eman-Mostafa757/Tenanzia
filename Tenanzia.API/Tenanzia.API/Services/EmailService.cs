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
            string toEmail,
            string toName,
            int invoiceId,
            int orderId,
            decimal amount,
            string status,
            string companyName,
            List<(string ProductName, int Quantity, decimal UnitPrice, decimal TotalPrice)> items)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(_config["SendGrid:FromEmail"], companyName);
            var to = new EmailAddress(toEmail, toName);
            var subject = $"Invoice #{invoiceId} from {companyName} — ${amount:N0}";

            var itemsRows = string.Join("", items.Select(i => $@"
            <tr>
              <td style='padding:10px;border-bottom:1px solid #eee;font-size:14px;color:#333'>{i.ProductName}</td>
              <td style='padding:10px;border-bottom:1px solid #eee;font-size:14px;color:#333'>{i.Quantity}</td>
              <td style='padding:10px;border-bottom:1px solid #eee;font-size:14px;color:#333'>${i.UnitPrice:N0}</td>
              <td style='padding:10px;border-bottom:1px solid #eee;font-size:14px;color:#333'>${i.TotalPrice:N0}</td>
            </tr>"));

            var htmlContent = $@"
        <!DOCTYPE html>
        <html>
        <body style='font-family:Arial,sans-serif;background:#f5f5f5;margin:0;padding:20px'>
          <div style='max-width:600px;margin:0 auto;background:white;border-radius:12px;overflow:hidden'>
            <div style='background:#D4537E;padding:30px;text-align:center'>
              <h1 style='color:white;margin:0;font-size:24px'>Tenanzia</h1>
              <p style='color:#FBEAF0;margin:5px 0 0;font-size:14px'>Invoice from {companyName}</p>
            </div>
            <div style='padding:30px'>
              <p style='font-size:16px;color:#333'>Dear {toName},</p>
              <p style='color:#666;font-size:14px'>You have received an invoice from <strong>{companyName}</strong>.</p>

              <table style='width:100%;border-collapse:collapse;margin:15px 0'>
                <tr style='background:#f0f0f0'>
                  <th style='padding:10px;text-align:left;font-size:12px;color:#666'>Invoice #</th>
                  <td style='padding:10px;font-size:14px;color:#333'>#{invoiceId}</td>
                  <th style='padding:10px;text-align:left;font-size:12px;color:#666'>Order #</th>
                  <td style='padding:10px;font-size:14px;color:#333'>#{orderId}</td>
                </tr>
                <tr>
                  <th style='padding:10px;text-align:left;font-size:12px;color:#666'>Status</th>
                  <td style='padding:10px;font-size:14px;color:#333'>{status}</td>
                  <th style='padding:10px;text-align:left;font-size:12px;color:#666'>Amount</th>
                  <td style='padding:10px;font-size:14px;font-weight:bold;color:#D4537E'>${amount:N0}</td>
                </tr>
              </table>

              <p style='font-weight:500;color:#333;margin-bottom:8px'>Order Items</p>
              <table style='width:100%;border-collapse:collapse;margin:15px 0'>
                <thead>
                  <tr style='background:#f0f0f0'>
                    <th style='padding:10px;text-align:left;font-size:12px;color:#666'>Product</th>
                    <th style='padding:10px;text-align:left;font-size:12px;color:#666'>Qty</th>
                    <th style='padding:10px;text-align:left;font-size:12px;color:#666'>Unit Price</th>
                    <th style='padding:10px;text-align:left;font-size:12px;color:#666'>Total</th>
                  </tr>
                </thead>
                <tbody>
                  {itemsRows}
                  <tr style='background:#FEF2F6'>
                    <td colspan='3' style='padding:10px;font-weight:bold;color:#D4537E;font-size:16px'>Total</td>
                    <td style='padding:10px;font-weight:bold;color:#D4537E;font-size:16px'>${amount:N0}</td>
                  </tr>
                </tbody>
              </table>

              <p style='color:#666;font-size:13px;text-align:center'>
                If you have any questions, please contact {companyName}.
              </p>
            </div>
            <div style='background:#D4537E;padding:20px;text-align:center'>
              <p style='color:white;margin:0;font-size:13px'>Thank you for your business!</p>
              <p style='color:#FBEAF0;margin:5px 0 0;font-size:12px'>Powered by Tenanzia</p>
            </div>
          </div>
        </body>
        </html>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid error: {body}");
            }
        }
    }
}
