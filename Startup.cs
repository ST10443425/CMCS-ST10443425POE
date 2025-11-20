// Add services
builder.Services.AddScoped<IClaimAutomationService, ClaimAutomationService>();

// Configure claim settings
builder.Services.Configure<ClaimSettings>(builder.Configuration.GetSection("ClaimSettings"));
