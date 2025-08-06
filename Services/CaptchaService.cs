using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using TeamDesk.Services.Interfaces;

public class CaptchaService : ICaptchaService
{
    private static readonly char[] chars =
        "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789".ToCharArray();

    public string GenerateCaptchaCode(int length = 6)
    {
        var rand = new Random();
        var codeChars = new char[length];
        for (int i = 0; i < length; i++)
        {
            codeChars[i] = chars[rand.Next(chars.Length)];
        }
        return new string(codeChars);
    }

    public byte[] GenerateCaptchaImage(string code)
    {
        using var bmp = new Bitmap(160, 60);
        using var gfx = Graphics.FromImage(bmp);
        gfx.SmoothingMode = SmoothingMode.AntiAlias;
        gfx.Clear(Color.AliceBlue);

        var rand = new Random();

        using (var pen = new Pen(Color.LightGray, 2))
        {
            for (int i = 0; i < 7; i++)
            {
                int x1 = rand.Next(bmp.Width);
                int y1 = rand.Next(bmp.Height);
                int x2 = rand.Next(bmp.Width);
                int y2 = rand.Next(bmp.Height);
                gfx.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        for (int i = 0; i < code.Length; i++)
        {
            using var font = new Font("Arial", 28, FontStyle.Bold);
            using var brush = new SolidBrush(RandomColor(rand));
            float angle = rand.Next(-20, 20);
            gfx.TranslateTransform(25 + i * 22, 30);
            gfx.RotateTransform(angle);
            gfx.DrawString(code[i].ToString(), font, brush, -10, -20);
            gfx.ResetTransform();
        }

        for (int i = 0; i < 100; i++)
        {
            int x = rand.Next(bmp.Width);
            int y = rand.Next(bmp.Height);
            bmp.SetPixel(x, y, RandomColor(rand));
        }

        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    public bool ValidateCaptcha(string input, string? storedCode)
    {
        return storedCode != null && string.Equals(storedCode, input, StringComparison.OrdinalIgnoreCase);
    }

    private Color RandomColor(Random rand)
    {
        return Color.FromArgb(rand.Next(120, 200), rand.Next(50, 160), rand.Next(50, 160));
    }
}
