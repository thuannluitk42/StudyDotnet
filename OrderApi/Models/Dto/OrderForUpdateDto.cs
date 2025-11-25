namespace OrderApi.Models.Dto
{
    public class OrderForUpdateDto
    {
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}