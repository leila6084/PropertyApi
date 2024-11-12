using PropertyApi.CsvDataRepository;
using PropertyApi.Models;
using PropertyApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("AppOptions"));
builder.Services.AddSingleton<CsvResultParser>();
builder.Services.AddSingleton<ICsvResultParser>(sp => sp.GetService<CsvResultParser>());
builder.Services.AddSingleton<PropertyService>(); 
builder.Services.AddSingleton<IHostedService>(sp => sp.GetService<PropertyService>()); 
builder.Services.AddSingleton<IPropertyService>(sp => sp.GetService<PropertyService>()); 

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:5152") // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Optional: For cookies or authentication headers
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
