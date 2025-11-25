namespace OrderApi.Models.Dto
{
    public class OrderForCreationDto
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
    }
}