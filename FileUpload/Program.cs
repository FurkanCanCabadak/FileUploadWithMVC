using FileUpload.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;  // E�er hesap do�rulama gerekliyse bunu true b�rak
    options.Password.RequireDigit = true;           // �ifrede rakam zorunlulu�u
    options.Password.RequiredLength = 8;            // �ifre minimum uzunluk
    options.Password.RequireUppercase = true;       // �ifrede b�y�k harf zorunlulu�u
    options.Password.RequireLowercase = true;       // �ifrede k���k harf zorunlulu�u
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Kimlik do�rulamay� aktif hale getir
app.UseAuthorization();  // Yetkilendirme i�lemi

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapRazorPages();

app.Run();
