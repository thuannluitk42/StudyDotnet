using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Messages;
using OrderApi.Models;
using OrderApi.Models.Dto;
using OrderApi.Services;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OrdersController> _logger;
		private readonly BookGrpcClient _bookGrpcClient;

        public OrdersController(
            OrderDbContext context,
            IPublishEndpoint publishEndpoint,
            ILogger<OrdersController> logger,
			BookGrpcClient bookGrpcClient)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
			_bookGrpcClient = bookGrpcClient;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _context.Orders.ToListAsync();
            return Ok(orders);
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // POST: api/orders
		[HttpPost]
		public async Task<ActionResult<Order>> CreateOrder(OrderForCreationDto orderDto)
		{
			// ✨ NEW: Validate book exists via gRPC
			_logger.LogInformation("🔍 Validating BookId={BookId} via gRPC", orderDto.BookId);
			
			var bookResponse = await _bookGrpcClient.GetBookAsync(orderDto.BookId);
			
			if (bookResponse == null)
			{
				_logger.LogWarning("❌ Book not found: BookId={BookId}", orderDto.BookId);
				return BadRequest(new { error = $"Book with ID {orderDto.BookId} not found" });
			}
			
			_logger.LogInformation("✅ Book validated via gRPC: {Title}", bookResponse.Title);
			
			// Use book data from gRPC response
			var order = new Order
			{
				BookId = orderDto.BookId,
				BookTitle = bookResponse.Title,  // From gRPC
				BookAuthor = orderDto.BookAuthor,
				Quantity = orderDto.Quantity,
				UnitPrice = orderDto.UnitPrice,
				TotalPrice = orderDto.Quantity * orderDto.UnitPrice,
				CustomerEmail = orderDto.CustomerEmail,
				OrderDate = DateTime.UtcNow,
				Status = "Pending"
			};

			_context.Orders.Add(order);
			await _context.SaveChangesAsync();

			_logger.LogInformation("📦 Order created: OrderId={OrderId}, BookId={BookId}",
				order.Id, order.BookId);

			// Publish OrderCreatedEvent
			await _publishEndpoint.Publish(new OrderCreatedEvent
			{
				OrderId = order.Id,
				BookId = order.BookId,
				BookTitle = order.BookTitle,
				BookAuthor = order.BookAuthor,
				Quantity = order.Quantity,
				TotalPrice = order.TotalPrice,
				CustomerEmail = order.CustomerEmail,
				CreatedAt = order.OrderDate
			});

			_logger.LogInformation("✅ OrderCreatedEvent published for OrderId={OrderId}", order.Id);

			return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
		}

        // PUT: api/orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, OrderForUpdateDto orderDto)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            order.Quantity = orderDto.Quantity;
            order.Status = orderDto.Status;
            order.TotalPrice = order.Quantity * order.UnitPrice;

            await _context.SaveChangesAsync();

            _logger.LogInformation("📝 Order updated: OrderId={OrderId}, Status={Status}", 
                order.Id, order.Status);

            return NoContent();
        }

        // DELETE: api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("🗑️ Order deleted: OrderId={OrderId}", id);

            return NoContent();
        }
        [HttpGet("test-stream")]
        public async Task<IActionResult> TestStream([FromQuery] int pageSize = 5, [FromQuery] int pageNumber = 1)
        {
            try
            {
                var books = await _bookGrpcClient.GetBooksStreamAsync(pageSize, pageNumber);
                return Ok(new { 
                    message = "Stream completed successfully",
                    count = books.Count,
                    books = books.Select(b => new { b.Id, b.Title })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stream books");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("test-retry")]
        public async Task<IActionResult> TestRetry([FromQuery] int bookId = 1)
        {
            try
            {
                var book = await _bookGrpcClient.GetBookAsync(bookId);
                return Ok(new { 
                    message = "GetBook succeeded",
                    book = new { book.Id, book.Title }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get book");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}