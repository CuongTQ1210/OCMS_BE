using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OCMS_BOs;
using OCMS_BOs.Helper;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Repositories.Repository;
using OCMS_Services.IService;
using OCMS_Services.Service;
using System.Text;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using StackExchange.Redis;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
var keyVaultEndpoint = new Uri(builder.Configuration["KeyVault:Endpoint"]);
builder.Configuration.AddAzureKeyVault(
   keyVaultEndpoint,
   new DefaultAzureCredential()
);

// Các cấu hình khác có thể lấy từ Key Vault
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<OCMSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorCodesToAdd: null)));

// Add Azure Clients
builder.Services.AddAzureClients(azureBuilder =>
    azureBuilder.AddBlobServiceClient(builder.Configuration.GetValue<string>("AzureBlobStorage")));

// Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:ConnectionString")));

// Add Email Service
builder.Services.AddTransient<IEmailService>(provider =>
{
    var smtpServer = builder.Configuration["Email:SmtpServer"];
    var smtpPort = int.Parse(builder.Configuration["Email:SmtpPort"]);
    var smtpUser = builder.Configuration["Email:SmtpUser"];
    var smtpPass = builder.Configuration["Email:SmtpPass"];
    return new EmailService(smtpServer, smtpPort, smtpUser, smtpPass);
});

builder.Services.AddScoped<JWTTokenHelper>();
builder.Services.AddAutoMapper(typeof(MappingHelper));
builder.Services.AddScoped<UnitOfWork>();

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ITrainingPlanRepository, TrainingPlanRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IExternalCertificateRepository, ExternalCertificateRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ICertiTempRepository, CertificateTemplateRepository>();
builder.Services.AddScoped<ITrainingScheduleRepository, TrainingScheduleRepository>();
builder.Services.AddScoped<IInstructorAssignmentRepository, InstructorAssignmentRepository>();
builder.Services.AddScoped<ITraineeAssignRepository, TraineeAssignRepository>();
builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
builder.Services.AddScoped<IDecisionTemplateRepository, DecisionTemplateRepository>();
builder.Services.AddScoped<IDecisionRepository, DecisionRepository>();
// Add services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IExternalCertificateService, ExternalCertificateService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddScoped<ITrainingPlanService, TrainingPlanService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ITrainingScheduleService, TrainingScheduleService>();
builder.Services.AddScoped<IInstructorAssignmentService, InstructorAssignmentService>();
builder.Services.AddScoped<ITraineeAssignService, TraineeAssignService>();
builder.Services.AddScoped<ICertificateTemplateService, CertificateTemplateService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddHttpClient<IHsmAuthService, HsmAuthService>();
builder.Services.AddHttpClient<IPdfSignerService, PdfSignerService>();
builder.Services.AddScoped<IDecisionTemplateService, DecisionTemplateService>();
builder.Services.AddScoped<IDecisionService, DecisionService>();
builder.Services.AddScoped<IReportService, ReportService>();
// Register Lazy<T> factories
builder.Services.AddScoped(provider => new Lazy<ITrainingScheduleService>(() => provider.GetRequiredService<ITrainingScheduleService>()));
builder.Services.AddScoped(provider => new Lazy<ITrainingPlanService>(() => provider.GetRequiredService<ITrainingPlanService>()));

builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
    };
});

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add other services
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Certificate Management System API", Version = "v1" });

    // Configure Swagger to use JWT authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Simple error handling
app.UseExceptionHandler(errorApp => {
    errorApp.Run(async context => {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            Message = "An unexpected error occurred. Please try again later."
        });
    });
});

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Add CORS middleware
app.UseCors("AllowAll");

// Home redirect to Swagger
app.Use(async (context, next) =>
{
    if (context.Request.Path.Value == "/")
    {
        context.Response.Redirect("/swagger/index.html");
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
