using ClientAppPOSWebAPI.Managers;
using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Success;
using Microsoft.AspNetCore.Mvc;

namespace ClientAppPOSWebAPI.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderManager _orderManager;

        public OrdersController(OrderManager orderManager)
        {
            _orderManager = orderManager;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (createOrderDto == null)
            {
                return BadRequest(Result.FailureResult("Order data is required"));
            }

            if (createOrderDto.OrderItems == null || !createOrderDto.OrderItems.Any())
            {
                return BadRequest(Result.FailureResult("Order must contain at least one item"));
            }

            try
            {
                var createdOrder = await _orderManager.CreateOrderAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, Result.SuccessResult(createdOrder));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Result.FailureResult(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, Result.FailureResult("An error occurred while creating the order"));
            }
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderManager.GetOrderAsync(id);

            if (order == null)
            {
                return NotFound(Result.FailureResult("Order not found"));
            }

            return Ok(Result.SuccessResult(order));
        }

        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderFilterDto filters)
        {
            var pagedResult = await _orderManager.GetAllOrdersAsync(filters);

            if (pagedResult == null)
            {
                return NotFound(Result.FailureResult("No orders found"));
            }

            return Ok(Result.SuccessResult(pagedResult));
        }

        // PATCH: api/orders/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderDto dto)
        {
            if (dto == null)
                return BadRequest(Result.FailureResult("No data provided"));

            var updatedOrder = await _orderManager.UpdateOrderAsync(id, dto);

            if (updatedOrder == null)
                return NotFound(Result.FailureResult("Order not found"));

            return Ok(Result.SuccessResult(updatedOrder));
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var success = await _orderManager.DeleteOrderAsync(id);

            if (!success)
            {
                return NotFound(Result.FailureResult("Order not found"));
            }

            return Ok(Result.SuccessResult("Order deleted successfully"));
        }

        // GET: api/orders/status/{status}
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            var filters = new OrderFilterDto { OrderStatus = status };
            var pagedResult = await _orderManager.GetAllOrdersAsync(filters);

            if (pagedResult == null || !pagedResult.Items.Any())
            {
                return NotFound(Result.FailureResult($"No orders found with status: {status}"));
            }

            return Ok(Result.SuccessResult(pagedResult));
        }

        // GET: api/orders/customer/{customerId}
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomer(int customerId)
        {
            // This would need to be implemented in the service layer
            // For now, we'll use the general filter approach
            var filters = new OrderFilterDto();
            var pagedResult = await _orderManager.GetAllOrdersAsync(filters);

            // Filter by customer ID in memory (not ideal for large datasets)
            var customerOrders = pagedResult.Items.Where(o => o.CustomerId == customerId).ToList();

            if (!customerOrders.Any())
            {
                return NotFound(Result.FailureResult($"No orders found for customer ID: {customerId}"));
            }

            return Ok(Result.SuccessResult(customerOrders));
        }
    }
}
