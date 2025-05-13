namespace Entities.ConfigModels
{
    public class DataBaseConfig
    {
        public DBConfig SqlServer { get; set; }
        public KeyApi KeyApi { get; set; }
        public IPConfig Redis { get; set; }
        public IPConfig Elastic { get; set; }
        public MongoConfig MongoServer { get; set; }
    }

    public class DBConfig
    {
        public string ConnectionString { get; set; } // chuoi ket noi db toi Travel. Tuc la db: adavigo
        public string ConnectionStringPQ { get; set; }// chuoi ket noi db toi Phu Quoc
        public string ConnectionStringTravel { get; set; }// chuoi ket noi db toi travel
        public string ConnectionStringUser { get; set; } // db user manager

    }
    public class KeyApi
    {
        public string api_manual { get; set; }
        public string api_cms { get; set; }
    }

    public class IPConfig
    {
        public string Host { get; set; }
    }

    public class DomainConfig
    {
        public string ImageStatic { get; set; }
    }
    public class MongoConfig
    {
        public string connection_string { get; set; }
        public string catalog_log { get; set; }
        public string catalog_core { get; set; }
    }
}
