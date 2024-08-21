// -------------------------------------------------------------------------------------------------
// Copyright (c) Integrated Health Information Systems Pte Ltd. All rights reserved.
// -------------------------------------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// Reads the 'FhirEngineSerilog' configuration section to add and configure Serilog for logging audit and application logs
builder.AddFhirEngineSerilogAuditLogging();

// Reads the 'FhirEngine' configuration section to add services
builder.AddFhirEngineServer();

var app = builder.Build();

app.UseHsts()
    .UseStaticFiles() // for serving rapidoc index.html
    .UseRouting()
    .UseFhirEngineMiddlewares()
    .UseAuthentication()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapFhirEngineHealthChecks("/health");
        endpoints.MapFhirEngine();
    });

await app.RunAsync();
