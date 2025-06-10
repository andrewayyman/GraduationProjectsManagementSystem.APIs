namespace Graduation_Project_Management.IServices
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
