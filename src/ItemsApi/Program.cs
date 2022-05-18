using ItemsApi;
using System.Linq.Expressions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddSingleton<IRepository<int, Item>, ItemRepository>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");



if (false)
{
#pragma warning disable CS0162 // Unreachable code detected
    app.MapPost("/items/{id}", (int id, Item item, ItemRepository items) => items.Create(id, item with { Id = id }));
    app.MapGet("/items/{id}", (int id, ItemRepository items) => items.Read(id));
    app.MapPut("/items/{id}", (int id, Item item, ItemRepository items) => items.Update(id, item with { Id = id }));
#pragma warning restore CS0162 // Unreachable code detected
}

if (false)
{
#pragma warning disable CS0162 // Unreachable code detected
    app.MapREST<Item, int, ItemRepository>("/items",
        (service, id, item) => service.Create(id, item with { Id = id }),
        (service, id) => service.Read(id),
        (service, id, item) => service.Update(id, item with { Id = id }),
        (service, id) => service.Delete(id));
#pragma warning restore CS0162 // Unreachable code detected
}

app.MapREST<ItemRepository, int, Item>("/items")
    .IncludeProperty(x => x.Name) // Adds endpoint /items/{key}/name
    .IncludeProperty(x => x.Description); // addeds /items/{key}/Description

// TODO the RestMapBuilder should also include the map of the top level methods so these can be customized also


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

    public static RestMapBuilder<TRepository, TKey, T> MapREST<TRepository, TKey, T>(this IEndpointRouteBuilder builder, string pattern)
        where TRepository : IRepository<TKey, T>
    {
        var route = $"{pattern}/{{key}}";

        builder.MapPost(route, (TKey key, T item, IRepository<TKey, T> repository) => repository.Create(key, item));
        builder.MapGet(route, (TKey key, IRepository<TKey, T> repository) => repository.Read(key));
        builder.MapPut(route, (TKey key, T item, IRepository<TKey, T> repository) => repository.Update(key, item));
        builder.MapDelete(route, (TKey key, IRepository<TKey, T> repository) => repository.Delete(key));

        return new RestMapBuilder<TRepository, TKey, T>(route, builder);
    }
}

public sealed class RestMapBuilder<TRepository, TKey, T>
{
    private readonly IEndpointRouteBuilder _routeBuilder;
    private readonly string _route;

    public RestMapBuilder(string route, IEndpointRouteBuilder routeBuilder)
    {
        _route = route;
        _routeBuilder = routeBuilder;
    }

    public RestMapBuilder<TRepository, TKey, T> IncludeProperty<TProperty>(Expression<Func<T, TProperty>> selector)
    {
        var property = (PropertyInfo)((MemberExpression)selector.Body).Member; // Some error handling might be nice
        var name = property.Name;

        var route = $"{_route}/{name}";

        // TODO Could be awesome to make without reflection
        // also a lot of error handling could be nice

        _routeBuilder.MapPost(route, (TKey key, [Microsoft.AspNetCore.Mvc.FromBody] TProperty value, IRepository<TKey, T> repository) =>
        {
            var entity = repository.Read(key) ?? throw new InvalidOperationException("Not found");
            var existing = property.GetValue(entity);
            if (existing != null)
            {
                throw new InvalidOperationException("Already exists");
            }

            property.SetValue(entity, value);
            repository.Update(key, entity);
        });

        _routeBuilder.MapGet(route, (TKey key, IRepository<TKey, T> repository) =>
        {
            var entity = repository.Read(key) ?? throw new InvalidOperationException("Not found");
            var value = property.GetValue(entity);

            // TODO cannot return null, if the value is null then it should be a not found?

            return (TProperty)value;
        });

        _routeBuilder.MapPut(route, (TKey key, [Microsoft.AspNetCore.Mvc.FromBody] TProperty value, IRepository<TKey, T> repository, HttpContext context) =>
        {
            var entity = repository.Read(key) ?? throw new InvalidOperationException("Not found");
            
            // So aparrently i can use reflection to modify properties on readonly record types
            property.SetValue(entity, value);
            repository.Update(key, entity);
        });

        _routeBuilder.MapDelete(route, (TKey key, IRepository<TKey, T> repository) =>
        {
            var entity = repository.Read(key) ?? throw new InvalidOperationException("Not found");

            // TODO Yeah - but what if the property is not nullable?
            property.SetValue(entity, null);
            repository.Update(key, entity);
        });

        return this;
    }
}
