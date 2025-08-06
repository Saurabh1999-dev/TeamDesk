namespace TeamDesk.Services.Interfaces
{
    public interface ICaptchaService
    {
        string GenerateCaptchaCode(int length = 6);
        byte[] GenerateCaptchaImage(string code);
        bool ValidateCaptcha(string input, string? storedCode);
    }

}
