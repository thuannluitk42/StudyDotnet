using BookApi.Models.Dto;
using FluentValidation;

namespace BookApi.Validators
{
	public class BookForCreationDtoValidator : AbstractValidator<BookForCreationDto>
	{
		public BookForCreationDtoValidator()
		{
			RuleFor(x => x.Title)
				.NotEmpty()
				.MaximumLength(200);

			RuleFor(x => x.Author)
				.NotEmpty()
				.MaximumLength(100);

			RuleFor(x => x.PublishedYear)
				.InclusiveBetween(1900, 2025)
				.WithMessage("Year must be between 1900 and 2025");
		}
	}
}
