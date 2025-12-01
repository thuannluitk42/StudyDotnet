namespace BookApi.Models.Dto
{
	public class BookForCreationDto
	{
		public string Title { get; set; } = string.Empty;
		public string Author { get; set; } = string.Empty;
		public int PublishedYear { get; set; }
	}
}
