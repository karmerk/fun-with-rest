using ItemsApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Items>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/items/{id}", (int id, Items items) => items.Read(id));
app.MapPost("/items/{id}", (int id, Item item, Items items) => items.Create(item with { Id = id}));
app.MapPut("/items/{id}", (int id, Item item, Items items) => items.Update(item with { Id = id }));

app.Run();
