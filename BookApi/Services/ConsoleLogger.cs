namespace BookApi.Services
{
	public class ConsoleLogger : ILogger { 
		public void Log(string msg) => Console.WriteLine(msg); 
	}
}
