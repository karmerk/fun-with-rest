namespace ItemsApi
{
    public interface IRepository<TKey, T>
    {
        void Create(TKey key, T item);
        void Delete(TKey key);
        T? Read(TKey key);
        void Update(TKey key, T item);
    }
}