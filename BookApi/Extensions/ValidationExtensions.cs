using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace BookApi.Extensions
{
	public static class ValidationExtensions
	{
		public static ValidationProblemDetails ToValidationProblemDetails(this ValidationResult validationResult)
		{
			var errors = validationResult.Errors
				.GroupBy(e => e.PropertyName)
				.ToDictionary(
					g => g.Key,
					g => g.Select(e => e.ErrorMessage).ToArray()
				);

			return new ValidationProblemDetails(errors)
			{
				Title = "One or more validation errors occurred.",
				Status = 400
			};
		}
	}
}
