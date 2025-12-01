using BookApi.data;
using BookApi.Protos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using static BookApi.Protos.BookService;

namespace BookApi.Services;

public class BookGrpcService : BookServiceBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookGrpcService> _logger;

    public BookGrpcService(AppDbContext context, ILogger<BookGrpcService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Unary RPC: Get single book by ID
    /// </summary>
    public override async Task<BookResponse> GetBook(GetBookRequest request, ServerCallContext context)
    {
        _logger.LogInformation("📖 gRPC GetBook called for BookId={BookId}", request.Id);

        var book = await _context.Books.FindAsync(request.Id);

        if (book == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Book with ID {request.Id} not found"));
        }

        return new BookResponse
        {
            Id = book.Id,
            Title = book.Title
        };
    }

    public override async Task GetBooksStream(
        GetBooksRequest request,
        IServerStreamWriter<BookResponse> responseStream,
        ServerCallContext context)
    {
        _logger.LogInformation("📚 gRPC GetBooksStream called: PageSize={PageSize}, PageNumber={PageNumber}",
            request.PageSize, request.PageNumber);

        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;

        var books = await _context.Books
            .OrderBy(b => b.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(context.CancellationToken);

        foreach (var book in books)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("⚠️ Stream cancelled by client");
                break;
            }

            var response = new BookResponse
            {
                Id = book.Id,
                Title = book.Title
            };

            await responseStream.WriteAsync(response);
            
            _logger.LogInformation("📖 Streamed book: Id={Id}, Title={Title}", book.Id, book.Title);
            
            // Simulate delay for demo
            await Task.Delay(100, context.CancellationToken);
        }

        _logger.LogInformation("✅ Stream completed: {Count} books sent", books.Count);
    }
}
