var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// db context
builder.Services.AddDbContext<QuoteBoatContext>();
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


