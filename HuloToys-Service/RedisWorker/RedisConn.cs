using HuloToys_Service.Utilities.Lib;
using StackExchange.Redis;
using Telegram.Bot.Types.Payments;
namespace HuloToys_Service.RedisWorker
{
    public class RedisConn
    {
        private readonly string _redisHost;
        private readonly int _redisPort;
        // private readonly int _db_index;        

        private ConnectionMultiplexer _redis;
        public RedisConn(IConfiguration config)
        {
            _redisHost = config["Redis:Host"];
            _redisPort = Convert.ToInt32(config["Redis:Port"]);
            // _db_index = Convert.ToInt32(config["Redis:Database:db_product"]);            
        }

        public void Connect()
        {
            try
            {
                if (_redis == null || !_redis.IsConnected)
                {
                    var configString = $"{_redisHost}:{_redisPort},connectRetry=5,allowAdmin=true";
                    _redis = ConnectionMultiplexer.Connect(configString);
                }
            }
            catch (RedisConnectionException err)
            {
                LogHelper.InsertLogTelegram("Connect RedisConn.Insert: " + err);
            }
        }

        public void Set(string key, string value, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            db.StringSet(key, value);
        }
        public void Set(string key, string value, DateTime expires, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            var expiryTimeSpan = expires.Subtract(DateTime.Now);

            db.StringSet(key, value, expiryTimeSpan);
        }

        public async Task<string> GetAsync(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return await db.StringGetAsync(key);
        }
        public string Get(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return db.StringGet(key);
        }

        public string GetNoAsync(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return db.StringGet(key);
        }

        public async void clear(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            await db.KeyDeleteAsync(key);
        }
        // ==============================
        // LIST (🔥 thêm mới cho notify paging)
        // ==============================
        public void LPush(string key, string value, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            db.ListLeftPush(key, value);
        }
        public void RPush(string key, string value, int dbIndex)
        {
            var db = _redis.GetDatabase(dbIndex);
            db.ListRightPush(key, value);
        }


        public List<string> LRange(string key, long start, long stop, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            var values = db.ListRange(key, start, stop);
            return values.Select(x => x.ToString()).ToList();
        }

        public void LTrim(string key, long start, long stop, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            db.ListTrim(key, start, stop);
        }
        public void LSet(string key, long index, string value, int dbIndex = 0)
        {
            var db = _redis.GetDatabase(dbIndex);
            db.ListSetByIndex(key, index, value);
        }
        public void Del(string key, int dbIndex = 0)
        {
            var db = _redis.GetDatabase(dbIndex);
            db.KeyDelete(key);
        }



        // ==============================
        // SET (🔥 thêm mới cho unseen count)
        // ==============================
        public void SAdd(string key, string value, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            db.SetAdd(key, value);
        }

        public void SRem(string key, string value, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            db.SetRemove(key, value);
        }

        public long SCard(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return db.SetLength(key);
        }
        private IDatabase GetDb(int dbIndex) => _redis.GetDatabase(dbIndex);

        // ==========================
        // Lấy toàn bộ member trong Set
        // ==========================
        public List<string> SMembers(string key, int dbIndex)
        {
            var db = GetDb(dbIndex);
            var members = db.SetMembers(key);
            return members.Select(x => x.ToString()).ToList();
        }

        public bool SIsMember(string key, string value, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return db.SetContains(key, value);
        }
    }
}
