using ClientAppPOSWebAPI.Managers;
using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Services;
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

            // Always return 200 status, even if no orders found
            if (pagedResult == null)
            {
                pagedResult = new PagedResult<Order>
                {
                    Items = new List<Order>(),
                    TotalCount = 0,
                    Page = filters.PageNumber,
                    PageSize = filters.PageSize
                };
            }

            return Ok(Result.SuccessResult(pagedResult));
        }

        // PUT: api/orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ReplaceOrder(int id, [FromBody] CreateOrderDto createOrderDto)
        {
            if (createOrderDto == null)
                return BadRequest(Result.FailureResult("Order data is required"));

            if (createOrderDto.OrderItems == null || !createOrderDto.OrderItems.Any())
            {
                return BadRequest(Result.FailureResult("Order must contain at least one item"));
            }

            try
            {
                // Note: The 'id' parameter from URL takes precedence over any 'id' in the request body
                var updatedOrder = await _orderManager.ReplaceOrderAsync(id, createOrderDto);
                
                if (updatedOrder == null)
                    return NotFound(Result.FailureResult("Order not found"));

                return Ok(Result.SuccessResult(updatedOrder));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Result.FailureResult(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, Result.FailureResult("An error occurred while updating the order"));
            }
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

        // GET: api/orders/drafts
        [HttpGet("drafts")]
        public async Task<IActionResult> GetDraftOrders([FromQuery] OrderFilterDto filters)
        {
            // Set IsDraft filter to true
            filters.IsDraft = true;
            var pagedResult = await _orderManager.GetAllOrdersAsync(filters);

            if (pagedResult == null || !pagedResult.Items.Any())
            {
                return Ok(Result.SuccessResult(new PagedResult<Order>
                {
                    Items = new List<Order>(),
                    TotalCount = 0,
                    Page = filters.PageNumber,
                    PageSize = filters.PageSize
                }));
            }

            return Ok(Result.SuccessResult(pagedResult));
        }

        // GET: api/orders/final
        [HttpGet("final")]
        public async Task<IActionResult> GetFinalOrders([FromQuery] OrderFilterDto filters)
        {
            // Set IsDraft filter to false
            filters.IsDraft = false;
            var pagedResult = await _orderManager.GetAllOrdersAsync(filters);

            if (pagedResult == null || !pagedResult.Items.Any())
            {
                return Ok(Result.SuccessResult(new PagedResult<Order>
                {
                    Items = new List<Order>(),
                    TotalCount = 0,
                    Page = filters.PageNumber,
                    PageSize = filters.PageSize
                }));
            }

            return Ok(Result.SuccessResult(pagedResult));
        }

        // POST: api/orders/{id}/convert-to-final
        [HttpPost("{id}/convert-to-final")]
        public async Task<IActionResult> ConvertDraftToFinal(int id)
        {
            var updateDto = new OrderDto
            {
                IsDraft = false,
                OrderStatus = "Pending",
                PaymentStatus = "Pending"
            };

            var updatedOrder = await _orderManager.UpdateOrderAsync(id, updateDto);

            if (updatedOrder == null)
                return NotFound(Result.FailureResult("Order not found"));

            return Ok(Result.SuccessResult(updatedOrder));
        }

        // POST: api/orders/{id}/convert-to-draft
        [HttpPost("{id}/convert-to-draft")]
        public async Task<IActionResult> ConvertFinalToDraft(int id)
        {
            var updateDto = new OrderDto
            {
                IsDraft = true,
                OrderStatus = "Draft",
                PaymentStatus = "Draft"
            };

            var updatedOrder = await _orderManager.UpdateOrderAsync(id, updateDto);

            if (updatedOrder == null)
                return NotFound(Result.FailureResult("Order not found"));

            return Ok(Result.SuccessResult(updatedOrder));
        }

        // POST: api/orders/{id}/generate-invoice
        [HttpPost("{id}/generate-invoice")]
        public async Task<IActionResult> GenerateInvoice(int id)
        {
            try
            {
                // First get the order to check if it exists
                var order = await _orderManager.GetOrderAsync(id);
                if (order == null)
                    return NotFound(Result.FailureResult("Order not found"));

                // Inject InvoiceService to generate invoice
                var invoiceService = HttpContext.RequestServices.GetRequiredService<InvoiceService>();
                var invoice = await invoiceService.GenerateInvoiceFromOrderAsync(id);
                
                return Ok(Result.SuccessResult(invoice));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Result.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result.FailureResult($"An error occurred while generating invoice: {ex.Message}"));
            }
        }
    }
}
