using Azure.Storage.Blobs;
using Azure.Storage.Queues;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<QueueServiceClient>(factory =>
{
    string? connString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
    return new QueueServiceClient(connString);
});
//builder.Services.AddTransient<BlobServiceClient>(factory =>
//{
//    string? connString = builder.Configuration.GetSection("MY_AZURE_STORAGE_CONNECTION_STRING").Value;
//    return new BlobServiceClient(connString);
//});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
