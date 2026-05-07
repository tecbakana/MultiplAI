using CMSXDAO;
using CMSAPIPublica.Services;
using CMSXData.Models;
using CMSXRepo;
using ICMSX;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "CMSX API Pública", Version = "v1" });
    var swaggerUrl = builder.Configuration["SwaggerServer:Url"] ?? "http://localhost:13230";
    c.AddServer(new Microsoft.OpenApi.Models.OpenApiServer { Url = swaggerUrl });
});

var dbProvider = builder.Configuration["DatabaseProvider"] ?? "PostgreSQL";
builder.Services.AddDbContext<CmsxDbContext>(o =>
{
    if (dbProvider == "SqlServer")
        o.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
    else
        o.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
});

builder.Services.AddCMSXDAL();
builder.Services.AddCMSXRepo();

builder.Services.AddHttpClient<SalematicHttpService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Salematic:BaseUrl"]!);
});

builder.Services.AddSingleton<IEventPublisher>(ps =>
{
    var config = ps.GetRequiredService<IConfiguration>();
    var logger = ps.GetRequiredService<ILogger<PedidoServiceBusPublisher>>();
    return new PedidoServiceBusPublisher(config, logger);
});

var salematicKey = builder.Configuration["SalematicJwt:Key"]!;
builder.Services.AddAuthentication()
    .AddJwtBearer("Salematic", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["SalematicJwt:Issuer"],
            ValidAudience            = builder.Configuration["SalematicJwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(salematicKey))
        };
    });
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Publica", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var permitLimit = builder.Configuration.GetValue<int>("RateLimit:PermitLimit", 30);
var windowSeg   = builder.Configuration.GetValue<int>("RateLimit:WindowSegundos", 60);

builder.Services.AddRateLimiter(rl =>
{
    rl.AddFixedWindowLimiter("api_publica", o =>
    {
        o.PermitLimit          = permitLimit;
        o.Window               = TimeSpan.FromSeconds(windowSeg);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit           = 0;
    });
    rl.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddHealthChecks();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CmsxDbContext>();
    if (dbProvider != "SqlServer")
        db.Database.Migrate();
}

app.UseRouting();
app.UseCors("Publica");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMSX API Pública v1");
    c.RoutePrefix = "swagger";
});

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
