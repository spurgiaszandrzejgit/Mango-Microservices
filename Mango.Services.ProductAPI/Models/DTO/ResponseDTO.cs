namespace Mango.Services.ProductAPI.Models.DTO
{
public class ResponseDTO<T>
{
    public bool IsSuccess { get; set; } = true;
    public T? Result { get; set; }
    public string? DisplayMessage { get; set; }
    public List<string>? ErrorMessages { get; set; }
}
}
