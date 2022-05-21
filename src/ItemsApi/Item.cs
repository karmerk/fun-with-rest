namespace ItemsApi;

public record SubItem(int Count, string Name);

public record Item(int Id, string Name, string Description, SubItem SubItem);




public sealed class ItemRepository : IRepository<int, Item>
{
    public readonly Dictionary<int, Item> _items = new Dictionary<int, Item>()
    {
        { 1, new Item(1, "Item 1", "Description 1", new SubItem(3, "Sub 1") )},
        { 2, new Item(2, "Item 2", "Description 2", new SubItem(3, "Sub 1") )},
        { 3, new Item(3, "Item 3", "Description 3", new SubItem(3, "Sub 1") )},
        { 4, new Item(4, "Item 4", "Description 4", new SubItem(3, "sub 1") )},
    };

    public void Create(int key, Item item)
    {
        _items.Add(key, item with { Id = key });
    }

    public Item? Read(int id)
    {
        if (_items.TryGetValue(id, out var item))
        {
            return item with { }; // Return a copy
        }
        return null;
    }

    public void Update(int key, Item item)
    {
        _items[item.Id] = item with { Id = key};
    }

    public void Delete(int id)
    {
        _items.Remove(id);
    }


}