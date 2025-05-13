namespace HuloToys_Service.RedisWorker
{
    public interface IRedisRepository
    {
        public void Set(string key, string value);
        public void Remove(string key);
        public string Get(string key);
    }
}
