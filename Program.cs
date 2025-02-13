using WebApplication5.Controllers; // MailController namespace'ini ekleyin
using Microsoft.Extensions.Configuration; // IConfiguration i�in gerekli
using WebApplication5.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // API controller'lar� i�in ekleyin

// MailService'i ba��ml�l�k enjeksiyonuna ekleyin (appsettings.json'dan ayarlar� alarak ve null kontrol� yaparak)
builder.Services.AddTransient<MailService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var smtpServer = configuration.GetSection("Smtp").GetValue<string>("Server");
    var smtpPort = configuration.GetSection("Smtp").GetValue<int>("Port");
    var smtpUsername = configuration.GetSection("Smtp").GetValue<string>("Username");
    var smtpPassword = configuration.GetSection("Smtp").GetValue<string>("Password");

    if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
    {
        throw new Exception("SMTP ayarlar� eksik veya hatal�. L�tfen appsettings.json dosyan�z� kontrol edin."); // Daha spesifik bir istisna mesaj�
    }

    if (!int.TryParse(smtpPort.ToString(), out int port)) // Port numaras�n�n ge�erli bir integer oldu�undan emin olun
    {
        throw new Exception("Ge�ersiz SMTP port numaras�. L�tfen appsettings.json dosyan�z� kontrol edin.");
    }

    return new MailService(smtpServer, port, smtpUsername, smtpPassword);
});

// MailController'� ba��ml�l�k enjeksiyonuna ekleyin (MailService'i kullanarak)
builder.Services.AddTransient<MailController>(sp => new MailController(sp.GetRequiredService<MailService>()));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Controller route'lar�n� ekleyin (TEK B�R Y�NTEM KULLANIN)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// app.MapControllers(); // Bu sat�r� silin veya yorum sat�r� yap�n. Gerekli de�il.

app.MapRazorPages();

// wwwroot/uploads klas�r�n� olu�tur
string uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.Run();