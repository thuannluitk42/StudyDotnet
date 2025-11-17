using Microsoft.AspNetCore.Authorization;

namespace BookApi.Authorization
{
	public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
	{
		protected override Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			MinimumAgeRequirement requirement)
		{
			var ageClaim = context.User.FindFirst("age");
			if (ageClaim != null && int.TryParse(ageClaim.Value, out int age) && age >= requirement.MinimumAge)
				context.Succeed(requirement);

			return Task.CompletedTask;
		}
	}
}
