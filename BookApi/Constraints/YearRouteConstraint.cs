namespace BookApi.Constraints;

public class YearRouteConstraint : IRouteConstraint
{
	public bool Match(HttpContext httpContext, IRouter route, string routeKey,
					  RouteValueDictionary values, RouteDirection routeDirection)
	{
		if (!values.TryGetValue(routeKey, out var value)) return false;
		if (!int.TryParse(value?.ToString(), out var year)) return false;
		return year >= 1900 && year <= 2025;
	}
}