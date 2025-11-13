using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookApi.Binders
{
	public class DateTimeBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext context)
		{
			var value = context.ValueProvider.GetValue("date").FirstValue;
			if (DateTime.TryParseExact(value, "dd-MM-yyyy", null, DateTimeStyles.None, out var date))
			{
				context.Result = ModelBindingResult.Success(date);
			}
			else
			{
				context.ModelState.AddModelError("date", "Invalid format. Use dd-MM-yyyy");
			}
			return Task.CompletedTask;
		}
	}
}
