using WebApplication5.Controllers; // MailController namespace'ini ekleyin
using Microsoft.Extensions.Configuration; // IConfiguration için gerekli
using WebApplication5.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // API controller'larý için ekleyin

// MailService'i baðýmlýlýk enjeksiyonuna ekleyin (appsettings.json'dan ayarlarý alarak ve null kontrolü yaparak)
builder.Services.AddTransient<MailService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var smtpServer = configuration.GetSection("Smtp").GetValue<string>("Server");
    var smtpPort = configuration.GetSection("Smtp").GetValue<int>("Port");
    var smtpUsername = configuration.GetSection("Smtp").GetValue<string>("Username");
    var smtpPassword = configuration.GetSection("Smtp").GetValue<string>("Password");

    if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
    {
        throw new Exception("SMTP ayarlarý eksik veya hatalý. Lütfen appsettings.json dosyanýzý kontrol edin."); // Daha spesifik bir istisna mesajý
    }

    if (!int.TryParse(smtpPort.ToString(), out int port)) // Port numarasýnýn geçerli bir integer olduðundan emin olun
    {
        throw new Exception("Geçersiz SMTP port numarasý. Lütfen appsettings.json dosyanýzý kontrol edin.");
    }

    return new MailService(smtpServer, port, smtpUsername, smtpPassword);
});

// MailController'ý baðýmlýlýk enjeksiyonuna ekleyin (MailService'i kullanarak)
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

// Controller route'larýný ekleyin (TEK BÝR YÖNTEM KULLANIN)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// app.MapControllers(); // Bu satýrý silin veya yorum satýrý yapýn. Gerekli deðil.

app.MapRazorPages();

// wwwroot/uploads klasörünü oluþtur
string uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.Run();