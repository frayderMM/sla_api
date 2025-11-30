using System.Net;
using System.Net.Mail;

namespace DamslaApi.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void EnviarCorreo(string para, string asunto, string cuerpo)
        {
            try
            {
                var smtp = new SmtpClient(_config["Email:Smtp"])
                {
                    Port = int.Parse(_config["Email:Port"]),
                    Credentials = new NetworkCredential(
                        _config["Email:User"],
                        _config["Email:Pass"]),
                    EnableSsl = true
                };

                var msg = new MailMessage
                {
                    From = new MailAddress(_config["Email:User"]),
                    Subject = asunto,
                    Body = cuerpo,
                    IsBodyHtml = true
                };

                msg.To.Add(para);
                smtp.Send(msg);
            }
            catch (Exception ex)
            {
                // Log del error pero no romper el flujo
                Console.WriteLine($"Error enviando correo: {ex.Message}");
            }
        }
    }
}
