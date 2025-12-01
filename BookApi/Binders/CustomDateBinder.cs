using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookApi.Binders
{
	public class CustomDateBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var value = bindingContext.ValueProvider.GetValue("date").FirstValue;

			if (DateTime.TryParseExact(value, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
			{
				bindingContext.Result = ModelBindingResult.Success(date);
			}
			else
			{
				bindingContext.ModelState.AddModelError("date", "Date must be in format dd-MM-yyyy");
			}

			return Task.CompletedTask;
		}
	}
}
