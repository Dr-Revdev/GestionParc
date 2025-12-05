using GestiParc.Core.Interfaces.Repositories;
using GestiParc.Infrastructure;
using GestiParc.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurer la connection string pour DbFactory
var cs = builder.Configuration.GetConnectionString("GestiParcDb");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("Connection string 'GestiParcDb' manquante dans appsettings.json.");

DbFactory.ConnectionString = cs;

// 2. Controllers
builder.Services.AddControllers();

// 3. DI : repository équipements
builder.Services.AddScoped<IEquipmentRepository, EquipmentMySqlRepository>();
builder.Services.AddScoped<IEquipmentTypeRepository, EquipmentTypeMySqlRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();