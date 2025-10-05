using Xunit;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orders.Application.Services;
using Orders.Application.Abstractions;
using Orders.Application.Interfaces;
using Orders.Application.DTOs;
using Orders.Domain.Entities;

namespace Orders.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orders;
        private readonly Mock<IUserRepository> _users;
        private readonly Mock<IAuditService> _audit;
        private readonly OrderService _svc;

        public OrderServiceTests()
        {
            _orders = new Mock<IOrderRepository>();
            _users = new Mock<IUserRepository>();
            _audit = new Mock<IAuditService>();

            _svc = new OrderService(_orders.Object, _users.Object, _audit.Object);
        }

        [Fact]
        public async Task CreateForUserAsync_Should_Create_Order_And_Log_Audit()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User("test@example.com", "hashedpw", "John Doe");

            _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(user);

            // Act
            var result = await _svc.CreateForUserAsync(userId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            _orders.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
            _audit.Verify(a => a.LogAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddItemAsync_Should_Add_Item_And_Log_Audit()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var order = new Order(userId);
            // ✅ Use positional constructor (Name, Quantity, UnitPrice)
            var req = new AddItemRequest("Guitar", 2, 1200m);

            _orders.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(order);

            // Act
            var result = await _svc.AddItemAsync(userId, orderId, req, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Items.Should().ContainSingle(i => i.Name == "Guitar" && i.Quantity == 2 && i.UnitPrice == 1200m);
            _orders.Verify(r => r.AddItemAsync(order, It.IsAny<OrderItem>(), It.IsAny<CancellationToken>()), Times.Once);
            _audit.Verify(a => a.LogAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
