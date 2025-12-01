using BookApi.Models.Dto;
using FluentValidation;

namespace BookApi.Validators
{
	public class BookForUpdateDtoValidator : AbstractValidator<BookForUpdateDto>
	{
		public BookForUpdateDtoValidator()
		{
			RuleFor(x => x.Title)
				.MaximumLength(200).When(x => x.Title != null);

			RuleFor(x => x.Author)
				.MaximumLength(100).When(x => x.Author != null);

			RuleFor(x => x.PublishedYear)
				.InclusiveBetween(1900, 2025).When(x => x.PublishedYear.HasValue);
		}
	}
}
