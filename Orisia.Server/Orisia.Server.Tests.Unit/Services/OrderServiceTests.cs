using Moq;
using Microsoft.Extensions.Options;
using Orisia.Server.Common.Options;
using Orisia.Server.Common.Requests.Order;
using Orisia.Server.Common.Responses.Order;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Data.Entities;
using Orisia.Server.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orisia.Server.Common.Requests.OrderItem;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Pages;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Data.PaginationAndFiltering;
using Orisia.Server.Domain.Interfaces;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> orderRepositoryMock;
        private readonly Mock<IProductRepository> productRepositoryMock;
        private readonly Mock<IAuthService> authServiceMock;
        private readonly Mock<IOrderItemRepository> orderItemRepositoryMock;
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<IEmailNotificationService> emailNotificationServiceMock;
        private readonly OrderService orderService;

        public OrderServiceTests()
        {
            orderRepositoryMock = new();
            productRepositoryMock = new();
            authServiceMock = new();
            orderItemRepositoryMock = new();
            userRepositoryMock = new();
            emailNotificationServiceMock = new();
            orderService = new(
                orderRepositoryMock.Object,
                productRepositoryMock.Object,
                authServiceMock.Object,
                orderItemRepositoryMock.Object,
                userRepositoryMock.Object,
                emailNotificationServiceMock.Object,
                Options.Create(new PaymentOptions()));
        }

        [Fact]
        public async Task ChangeStatusAsync_ShouldThrowAppException_WhenOrderNotFound()
        {
            orderRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

            AppException exception = await Assert.ThrowsAsync<AppException>(async () => 
                await orderService.ChangeStatusAsync(new() { OrderId = Guid.NewGuid(), OrderStatus = OrderStatus.Cancelled }));

            Assert.Equal("Order not found", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task ChangeStatusAsync_ShouldReturnTrue_WhenOrderStatusChanged()
        {
            Order order = new() { Id = Guid.NewGuid(), Status = OrderStatus.Created };
            orderRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(order);
            orderRepositoryMock.Setup(x => x.ChangeStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>())).ReturnsAsync((Order?)null);

            bool result = await orderService.ChangeStatusAsync(new() { OrderId = order.Id, OrderStatus = OrderStatus.Delivered });

            Assert.True(result);
        }

        [Fact]
        public async Task GetAsync_ShouldThrowAppException_WhenOrderNotFound()
        {
            authServiceMock.Setup(x => x.GetCurrentUserId()).ReturnsAsync(Guid.NewGuid().ToString());
            orderRepositoryMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

            AppException exception = await Assert.ThrowsAsync<AppException>(async () =>
                await orderService.GetAsync());

            Assert.Equal("Order not found", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnOrderResponse()
        {
            Order order = new() { Id = Guid.NewGuid(), OrderTotalPrice = 100, Status = OrderStatus.Created };
            authServiceMock.Setup(x => x.GetCurrentUserId()).ReturnsAsync(order.UserId.ToString());
            orderRepositoryMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(order);

            OrderResponse result = await orderService.GetAsync();

            Assert.NotNull(result);
            Assert.Equal(order.Id, result.Id);
        }

        [Fact]
        public async Task AddProductAsync_ShouldThrowAppException_WhenProductNotFound()
        {
            AddOrderItemRequest request = new() { ProductId = Guid.NewGuid(), Quantity = 1 };
            authServiceMock.Setup(x => x.GetCurrentUserId()).ReturnsAsync(Guid.NewGuid().ToString());
            productRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

            AppException exception = await Assert.ThrowsAsync<AppException>(async () =>
                await orderService.AddProductAsync(request));

            Assert.Equal("Product not found", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task RemoveProductAsync_ShouldThrowAppException_WhenOrderNotFound()
        {
            RemoveOrderItemRequest request = new()
            {
                ProductId = Guid.NewGuid(),
                Quantity = 0
            };
            authServiceMock.Setup(x => x.GetCurrentUserId()).ReturnsAsync(Guid.NewGuid().ToString());
            orderRepositoryMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

            AppException exception = await Assert.ThrowsAsync<AppException>(async () =>
                await orderService.RemoveProductAsync(request));

            Assert.Equal("Order not found", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task SendCurrentAsync_ShouldThrowAppException_WhenOrderNotFound()
        {
            SendOrderRequest request = new()
            {
                Names = "John Doe",
                PostalCode = null,
                Country = null,
                City = null,
                Address = null,
                Phone = null
            };
            authServiceMock.Setup(x => x.GetCurrentUserId()).ReturnsAsync(Guid.NewGuid().ToString());
            orderRepositoryMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

            AppException exception = await Assert.ThrowsAsync<AppException>(async () =>
                await orderService.SendCurrentAsync(request));

            Assert.Equal("Order not found", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }
    }
}
