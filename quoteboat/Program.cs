using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using quoteboat.Interfaces;
using quoteboat.Repositories;
using quoteboat.Data;

var builder = WebApplication.CreateBuilder(args);

// database connection
var connection = String.Empty;
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.Development.json");
    connection = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
}
else
{
    connection = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");
}
builder.Services.AddDbContext<QuoteBoatContext>(options =>
    options.UseSqlServer(connection, sqlOptions => 
    sqlOptions.EnableRetryOnFailure()));


// Add services to the container.
// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// scopes for repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<IQuoteItemRepository, QuoteItemRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();


var app = builder.Build();

// swagger
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();


app.Run();


