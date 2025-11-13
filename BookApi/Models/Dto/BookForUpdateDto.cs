namespace BookApi.Models.Dto
{
	public class BookForUpdateDto
	{
		public string? Title { get; set; }
		public string? Author { get; set; }
		public int? PublishedYear { get; set; }
	}
}
