var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. ACTIVAR EL SERVICIO DE SESIÓN (Antes del Build)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // La sesión dura 20 min
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
}); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

// 2. USAR LA SESIÓN (Importante: debe ir antes de UseRouting o MapControllerRoute)
app.UseSession(); 

app.UseRouting();

app.UseAuthorization();

// 3. RUTA POR DEFECTO CORREGIDA
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Index}/{id?}"); 
    // ^^^ CAMBIO IMPORTANTE: Ahora apunta a "Acceso" en lugar de "Login"

app.Run();