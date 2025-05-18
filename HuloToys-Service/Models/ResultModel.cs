namespace API_CORE.Controllers.Models
{
    public class ResultModel<T>
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }
    }

}
