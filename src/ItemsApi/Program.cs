using ItemsApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepository<int, Item>, Items>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//app.MapPost("/items/{id}", (int id, Item item, Items items) => items.Create(item with { Id = id }));
//app.MapGet("/items/{id}", (int id, Items items) => items.Read(id));
//app.MapPut("/items/{id}", (int id, Item item, Items items) => items.Update(item with { Id = id }));

//app.MapREST<Item, int, Items>("/items",
//    (service, id, item) => service.Create(id, item with { Id = id }),
//    (service, id) => service.Read(id),  
//    (service, id, item) => service.Update(id, item with { Id = id }),
//    (service, id) => service.Delete(id));

app.MapREST<Items, int, Item>("/items");

app.Run();



public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapREST<T, TKey, TService>(this IEndpointRouteBuilder endpoints, 
        string pattern, 
        Action<TService, TKey, T> create, 
        Func<TService, TKey, T> read, 
        Action<TService, TKey, T> update, 
        Action<TService, TKey> delete)
    {
        var route = $"{pattern}/{{key}}";

        endpoints.MapPost(route, (TKey key, T item, TService service) => create(service, key, item));
        endpoints.MapGet(route, (TKey key, TService service) => read(service, key));
        endpoints.MapPut(route, (TKey key, T item, TService service) => update(service, key, item));
        endpoints.MapDelete(route, (TKey key, TService service) => delete(service, key));

        return endpoints;
    }

    public static IEndpointRouteBuilder MapREST<TRepository, TKey, T>(this IEndpointRouteBuilder endpoints, string pattern)
        where TRepository : IRepository<TKey, T>
    {
        var route = $"{pattern}/{{key}}";

        endpoints.MapPost(route, (TKey key, T item, IRepository<TKey, T> repository) => repository.Create(key, item));
        endpoints.MapGet(route, (TKey key, IRepository<TKey, T> repository) => repository.Read(key));
        endpoints.MapPut(route, (TKey key, T item, IRepository<TKey, T> repository) => repository.Update(key, item));
        endpoints.MapDelete(route, (TKey key, IRepository<TKey, T> repository) => repository.Delete(key));

        return endpoints;
    }
}

