namespace HuloToys_Service.Models.Article
{
    public class GenericViewModel<TEntity> where TEntity : class
    {
        public List<TEntity> ListData { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
        public long TotalRecord { get; set; }


    }

    public class Paging
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
        public long TotalRecord { get; set; }
        public string PageAction { get; set; }
        public string RecordName { get; set; }
    }
}
