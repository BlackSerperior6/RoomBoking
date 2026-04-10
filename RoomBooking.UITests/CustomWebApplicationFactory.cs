using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using RoomBooking.Interfaces;

namespace RoomBooking.UITests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Mock<IDatabaseConnectionFactory> _mockConnectionFactory = new();
    private readonly Mock<IUserContextWrapper> _mockUserContext = new();
    private readonly Mock<IDbConnectionWrapper> _mockConnection = new();
    private readonly Mock<IDbCommandWrapper> _mockCommand = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var dbFactoryDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDatabaseConnectionFactory));

            if (dbFactoryDescriptor != null)
                services.Remove(dbFactoryDescriptor);
            
            var userContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IUserContextWrapper));

            if (userContextDescriptor != null)
                services.Remove(userContextDescriptor);
            
            services.AddScoped(_ => _mockConnectionFactory.Object);
            services.AddScoped(_ => _mockUserContext.Object);
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
            });
        });
        
        builder.UseEnvironment("Development");
    }

    public void ResetMocks()
    {
        _mockConnectionFactory.Reset();
        _mockUserContext.Reset();
        _mockConnection.Reset();
        _mockCommand.Reset();
    }

    public void SetupMoq(bool setupFailure = false)
    {
        _mockConnectionFactory.Setup(x => x.CreateConnection())
            .Returns(_mockConnection.Object);
        
        _mockConnection.Setup(x => x.CreateCommand(It.IsAny<string>()))
            .Returns(_mockCommand.Object);
        
        if (setupFailure)
            _mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Npgsql.NpgsqlException("Connection failed"));
        else
            _mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        
        _mockCommand.Setup(x => x.AddParameter(
            It.IsAny<string>(),
            It.IsAny<NpgsqlTypes.NpgsqlDbType>(),
            It.IsAny<object>()))
            .Verifiable();
    }
}