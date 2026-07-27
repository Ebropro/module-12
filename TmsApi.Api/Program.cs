using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Api.Auth;
using TmsApi.Api.Filters;
using TmsApi.Api.Middlewares;
using TmsApi.Api.Options;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using Asp.Versioning;
using TmsApi.Application.Enrollments.Commands;
using MediatR;
using TmsApi.Application.Behaviors;
using TmsApi.Api.ExceptionHandlers;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Api.RateLimiting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);

// LoggingBehavior FIRST — it must wrap ValidationBehavior
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

// builder.Services.AddDbContext<TmsDbContext>(options =>
//     options
//         .UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
//         .LogTo(Console.WriteLine, LogLevel.Information)  // Log SQL to output window
//         .EnableSensitiveDataLogging()  // Show parameters in query logs (dev only)
// );

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});



// M7 - Exercise 1: API Versioning
// Step 1 Configure versioning in Program.cs
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddOpenApi();

// AUTHENTICATION SETUP
// Attach a custom handler (BasicAuthHandler) that defines how users are authenticated.
builder.Services
    .AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", _ => { });

// M7 Session 2 — Exercise 3, Step 1: register HybridCache
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

// Production-only — leave commented in lab. Register a Redis-backed
// IDistributedCache BEFORE AddHybridCache and HybridCache will pick it up
// automatically as its L2 layer.
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("Redis");
//     options.InstanceName = "tms:";
// });

builder.Services.AddSingleton<EnrollmentWorker>();

// M7 Session 2 — Exercise 4, Step 2: tier-aware token bucket as the global policy
builder.Services.AddRateLimiter(options =>
{
    // options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    // {
    //     var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

    //     return tier switch
    //     {
    //         ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
    //             partitionKey: $"paid:{partitionKey}",
    //             factory: _ => new TokenBucketRateLimiterOptions
    //             {
    //                 TokenLimit = 200,
    //                 TokensPerPeriod = 100,
    //                 ReplenishmentPeriod = TimeSpan.FromSeconds(10),
    //                 QueueLimit = 0,
    //                 AutoReplenishment = true
    //             }),
    //         ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
    //             partitionKey: $"free:{partitionKey}",
    //             factory: _ => new TokenBucketRateLimiterOptions
    //             {
    //                 TokenLimit = 30,
    //                 TokensPerPeriod = 10,
    //                 ReplenishmentPeriod = TimeSpan.FromSeconds(10),
    //                 QueueLimit = 0,
    //                 AutoReplenishment = true
    //             }),
    //         _ => RateLimitPartition.GetTokenBucketLimiter(
    //             partitionKey: $"anon:{partitionKey}",
    //             factory: _ => new TokenBucketRateLimiterOptions
    //             {
    //                 TokenLimit = 10,
    //                 TokensPerPeriod = 5,
    //                 ReplenishmentPeriod = TimeSpan.FromSeconds(10),
    //                 QueueLimit = 0,
    //                 AutoReplenishment = true
    //             })
    //     };
    // });


// ==========================================================================
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
{
    if (httpContext.Request.Path.StartsWithSegments("/api/v2/transcripts"))
    {
        return RateLimitPartition.GetNoLimiter("transcripts");
    }

    var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

    return tier switch
    {
        ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
            $"paid:{partitionKey}",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 200,
                TokensPerPeriod = 100,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }),

        ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
            $"free:{partitionKey}",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 30,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }),

        _ => RateLimitPartition.GetTokenBucketLimiter(
            $"anon:{partitionKey}",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 10,
                TokensPerPeriod = 5,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
                AutoReplenishment = true
            })
    };
});
// ==========================================================================



    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    

    
    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts))
            retryAfter = ((int)ts.TotalSeconds).ToString();

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType = "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Rate limit exceeded",
            Detail = $"Too many requests. Retry after {retryAfter} seconds.",
            Status = StatusCodes.Status429TooManyRequests,
            Type = "https://tms.local/errors/rate_limit_exceeded"
        }, ct);
    };

    // M7 Session 2 — Exercise 4, Step 3: separate concurrency limiter for the
    // transcript endpoint. Token bucket limits how OFTEN you can call; this
    // limits how MANY can run at once — the thing that actually kills the DB pool.
    options.AddConcurrencyLimiter("transcripts", opt =>
    {
        opt.PermitLimit = 5;      // 5 in-flight transcripts maximum
        opt.QueueLimit = 20;      // queue up to 20 more
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddAuthorization();// AUTHORIZATION SETUP (ARE YOU ALLOWED?)
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseMiddleware<V1DeprecationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
// Minimal API endpoint protected by authorization
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    // Placeholder response
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}))
.RequireAuthorization(); // Forces authentication before execution



app.MapControllers();
app.MapGet("/api/error", () =>
{
    // throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
    throw new InvalidOperationException("Simulated database failure for ProblemDetails testing");
});
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

        // Tell Scalar to pull both documents into its sidebar dropdown
        options
            .AddDocument("v1", "API Version 1.0")
            .AddDocument("v2", "API Version 2.0");
    });
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();

    await DataSeeder.SeedAsync(context);
}

app.Run();

