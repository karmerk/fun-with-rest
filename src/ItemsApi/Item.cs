namespace ItemsApi;

public record Item(int Id, string Name, string Description);


public sealed class Items
{
    public readonly Dictionary<int, Item> _items = new Dictionary<int, Item>()
    {
        { 1, new Item(1, "Item 1", "Description 1") },
        { 2, new Item(2, "Item 2", "Description 2") },
        { 3, new Item(3, "Item 3", "Description 3") },
        { 4, new Item(4, "Item 4", "Description 4") },
    };

    public void Create(Item item)
    {
        _items.Add(item.Id, item);
    }

    public Item? Read(int id)
    {
        if (_items.TryGetValue(id, out var item))
        {
            return item;
        }
        return null;
    }

    public void Update(Item item)
    {
        _items[item.Id] = item;
    }

    public void Delete(int id)
    {
        _items.Remove(id);
    }


}