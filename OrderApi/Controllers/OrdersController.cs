using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Messages;
using OrderApi.Models;
using OrderApi.Models.Dto;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            OrderDbContext context,
            IPublishEndpoint publishEndpoint,
            ILogger<OrdersController> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
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
            var order = new Order
            {
                BookId = orderDto.BookId,
                BookTitle = orderDto.BookTitle,
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
    }
}