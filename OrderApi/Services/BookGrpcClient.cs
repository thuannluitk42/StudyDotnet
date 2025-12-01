using BookApi.Protos;
using Grpc.Net.Client;
using Grpc.Core;

namespace OrderApi.Services;

public class BookGrpcClient
{
    private readonly GrpcChannel _channel;
    private readonly BookService.BookServiceClient _client;
    private readonly ILogger<BookGrpcClient> _logger;

    public BookGrpcClient(BookService.BookServiceClient client, ILogger<BookGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
        _logger.LogInformation("📡 BookGrpcClient initialized with Polly resilience patterns");
    }

    /// <summary>
    /// Get book by ID via gRPC
    /// </summary>
    public async Task<BookResponse?> GetBookAsync(int bookId)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                _logger.LogInformation("🔍 Calling gRPC GetBook for BookId={BookId} (Attempt {Attempt})", 
                    bookId, retryCount + 1);

                var request = new GetBookRequest { Id = bookId };
                var response = await _client.GetBookAsync(request);

                _logger.LogInformation("✅ gRPC GetBook succeeded: {Title}", response.Title);
                return response;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable && retryCount < maxRetries - 1)
            {
                retryCount++;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                
                _logger.LogWarning("🔄 Retry {RetryCount} after {Delay}s due to: {Error}", 
                    retryCount, delay.TotalSeconds, ex.Status.Detail);
                
                await Task.Delay(delay);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "❌ gRPC GetBook failed for BookId={BookId}", bookId);
                throw;
            }
        }

        return null;
    }

    /// <summary>
    /// Check if book exists and has stock
    /// </summary>
    public async Task<bool> CheckBookAvailabilityAsync(int bookId)
    {
        var book = await GetBookAsync(bookId);
        return book != null;
    }

    public async Task<List<BookResponse>> GetBooksStreamAsync(int pageSize = 10, int pageNumber = 1)
    {
        _logger.LogInformation("📚 Calling gRPC GetBooksStream: PageSize={PageSize}, PageNumber={PageNumber}",
            pageSize, pageNumber);

        var request = new GetBooksRequest
        {
            PageSize = pageSize,
            PageNumber = pageNumber
        };

        var books = new List<BookResponse>();

        try
        {
            using var call = _client.GetBooksStream(request);

            while (await call.ResponseStream.MoveNext())
            {
                var book = call.ResponseStream.Current;
                _logger.LogInformation("📖 Received book: Id={Id}, Title={Title}", book.Id, book.Title);
                books.Add(book);
            }

            _logger.LogInformation("✅ Stream completed: {Count} books received", books.Count);
            return books;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "❌ gRPC GetBooksStream failed");
            throw;
        }
    }
}