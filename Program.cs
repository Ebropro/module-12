using TmsApi.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using TmsApi.Services;
using Tms.Api.Persistence;
using Tms.Api.Filters;

using Asp.Versioning;


var builder = WebApplication.CreateBuilder(args);
// Step 3: Register the DbContext in Program.cs
// Register TmsDbContext scoped for incoming HTTP requests

builder.Services.AddDbContext<TmsDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        //EX-2 Step 1: Enable Console SQL Logging
        .LogTo(Console.WriteLine, LogLevel.Information)  // Log SQL to output window
        .EnableSensitiveDataLogging()  // Show parameters in querylogs (dev only)
);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddControllers(options =>
{
options.Filters.Add<AuditLogFilter>();
});

// Register controller support (MVC pipeline for API controllers)
 builder.Services.AddControllers();
 builder.Services.AddOpenApi();

// AUTHENTICATION SETUP
// attach a custom handler (BasicAuthHandler) that defines how users are authenticated
builder.Services
    .AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", _ => { });


builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();


builder.Services.AddProblemDetails();



// AUTHORIZATION SETUP (ARE YOU ALLOWED?)
// Enables [Authorize] / RequireAuthorization() checks 
builder.Services.AddAuthorization();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// APPLICATION PIPELINE (ORDER MATTERS)
var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


// Minimal API endpoint protected by authorization
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});
//
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
// Placeholder response    
courseCode = "CS-101",
studentId = "S-001",
letterGrade = "A"
}))
.RequireAuthorization(); // Forces authentication before execution

// app.MapOpenApi(); // enables /openapi.json
// app.UseExceptionHandler();

// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler();
// }

app.MapControllers();
app.MapGet("/api/error", () =>
{
    //throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
    throw new InvalidOperationException("Simulated database failure for ProblemDetails testing");
});


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // UI
}



// EXCERSICE - Step 2: Write an Auto-Seeder
//SEDDER CODE

// Seed test data at startup
// using (var scope = app.Services.CreateScope())
//  {
//      var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();

//      context.Database.Migrate(); // // Applies any pending migrations; keeps migration history intact
    
//      if (!context.Students.Any())
//      {
//         var students = new List<Student>
//         {
//             new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", Age= 22, GPA = 3.8m, IsActive = true },
//             new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", Age= 23, GPA = 2.9m, IsActive = true },
//             new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", Age= 25, GPA = 3.4m, IsActive = false },
//             new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince", Age= 27, GPA = 3.9m, IsActive = true },
//             new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright", Age= 24, GPA = 2.5m, IsActive = true }
//         };

//         context.Students.AddRange(students);

//         var courses = new List<Course>
//         {
//             new() { Code = "CS-101", Title = "Intro to CS", MaxCapacity = 30 },
//             new() { Code = "CS-201", Title = "Data Structures", MaxCapacity = 25 },
//             new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity = 40 }
//         };

//         context.Courses.AddRange(courses);

//         context.SaveChanges();

//         var enrollments = new List<Enrollment>
//         {
//             new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
//             new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
//             new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
//             new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
//         };

//         context.Enrollments.AddRange(enrollments);

//         context.SaveChanges();
//     }
// }

if (app.Environment.IsDevelopment())
{
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
await DataSeeder.SeedAsync(context);
}

app.Run();

