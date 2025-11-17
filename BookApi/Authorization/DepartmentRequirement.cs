using Microsoft.AspNetCore.Authorization;

namespace BookApi.Authorization
{
	public class DepartmentRequirement(string department) : IAuthorizationRequirement
	{
		public string Department { get; } = department;
	}
}
