using Microsoft.EntityFrameworkCore;
using apiDocument.Models;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

//Accedemos a la configuracion
var configuration = builder.Configuration;

//Obtenemos la cadena de conexión de la configuración
var connectionString = configuration.GetConnectionString("DefaultConnection");
SqlConnectionStringBuilder sqlConnect = new SqlConnectionStringBuilder(connectionString);

sqlConnect.TrustServerCertificate = true;

string sqlConnectionString = sqlConnect.ConnectionString;


builder.Services.AddControllers();

builder.Services.AddDbContext<EventsContext>(opt => opt.UseSqlServer(sqlConnectionString));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUi();
}

//Configuracion de CORS
app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();