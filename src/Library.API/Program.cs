using System.Text;
using System.Text.Json.Serialization;
using Library.API.Middleware;
using Library.Application.Common;
using Library.Application.Interfaces;
using Library.Application.Mappings;
using Library.Application.Services;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Repositories;
using Library.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// NLog מתחת ל-ILogger הרגיל: הקוד בכל השכבות לא משתנה. כל ההגדרות ב-nlog.config
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Enums ב-JSON כמילים ("Lost") ולא כמספרים (3)
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// שגיאות בלי גוף (למשל 401/403 משכבת ההרשאות, או כתובת שלא קיימת) יקבלו גוף ProblemDetails,
// באותו פורמט JSON כמו שאר השגיאות במערכת
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();

// Swagger עם כפתור Authorize: מדביקים token ומנסים endpoints מוגנים
builder.Services.AddSwaggerGen(options =>
{
    var bearerScheme = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "הדביקי כאן את ה-token מ-/api/auth/login (בלי המילה Bearer)",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [bearerScheme] = Array.Empty<string>() });
});

// JWT: ההגדרות נבדקות בעליית השרת. מפתח חסר או קצר מ-32 תווים מונע מהשרת לעלות
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

// פרמטרי הבדיקה של ה-token נלקחים מאותן JwtOptions שכבר עברו ולידציה
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

builder.Services.AddAuthorization();

// CORS: רק כתובת הלקוח רשאית לפנות לשרת מהדפדפן. הכתובות מהקונפיגורציה:
// בפיתוח appsettings.Development.json, בענן משתנה סביבה
const string ClientCorsPolicy = "Client";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options => options.AddPolicy(ClientCorsPolicy, policy =>
    policy.WithOrigins(allowedOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          // כדי שהלקוח יוכל לקרוא את הכותרות האלה מהתשובה
          .WithExposedHeaders(CorrelationIdMiddleware.HeaderName, "Location")));

// חוקי ההשאלה מ-appsettings, נבדקים בעליית השרת
builder.Services.AddOptions<LoanRulesOptions>()
    .Bind(builder.Configuration.GetSection(LoanRulesOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ה-connection string נטען מהקונפיגורציה (בפיתוח: User Secrets), אף פעם לא מהקוד
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is missing. Set it with: dotnet user-secrets set \"ConnectionStrings:Default\" \"...\"");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// AutoMapper: טוען את כל ה-Profiles מפרויקט Application
builder.Services.AddAutoMapper(typeof(BookProfile).Assembly);

// Repositories ו-Services: Scoped, כי הם תלויים ב-AppDbContext (Scoped).
// אם היו Singleton, הם היו מחזיקים DbContext אחד לכל חיי השרת ומשתפים אותו בין בקשות. זה באג.
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBookCopyRepository, BookCopyRepository>();

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICopyService, CopyService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ILoanService, LoanService>();

// Singleton: המחלקות לא מחזיקות שום מצב ולא תלויות בשירות Scoped
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// Scoped: תלוי ב-AppDbContext, שהוא Scoped
builder.Services.AddScoped<DatabaseInitializer>();

var app = builder.Build();

// Migrate + Seed אוטומטיים בעליית השרת
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync(app.Lifetime.ApplicationStopping);
}

// סדר ה-pipeline: טיפול בשגיאות ראשון, כדי לעטוף את כל השאר
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseStatusCodePages();

// Swagger פעיל גם בענן: זה פרויקט הדגמה, וכך אפשר לבדוק את כל ה-endpoints גם ב-Render
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(ClientCorsPolicy);

// Authentication (מי את?) חייב לבוא לפני Authorization (מה מותר לך?)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
