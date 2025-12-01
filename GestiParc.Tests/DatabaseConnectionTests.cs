using Microsoft.Extensions.Configuration;
using GestiParc.Infrastructure;

namespace GestiParc.Tests
{
    public class DatabaseConnectionTests
    {
        private static string GetConnectionString()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            return configuration.GetConnectionString("MySqlConnection")
                   ?? throw new InvalidOperationException("MySqlConnection non trouvée dans appsettings.json");
        }

        [Fact]
        public void TestDatabaseConnection_ShouldConnect_Successfully()
        {
            // Arrange
            DbFactory.ConnectionString = GetConnectionString();

            // Act
            using var connection = DbFactory.Create();
            connection.Open();

            // Assert
            Assert.NotNull(connection);
            Assert.Equal(System.Data.ConnectionState.Open, connection.State);
        }

        [Fact]
        public void TestDatabaseConnection_WithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            DbFactory.ConnectionString = "Server=localhost;Port=3306;Database=gestiparc;User ID=wrong_user;Password=wrong_password;SslMode=None;ConnectionTimeout=2;";

            // Act & Assert
            using var connection = DbFactory.Create();
            Assert.Throws<MySqlConnector.MySqlException>(() => connection.Open());
        }

        [Fact]
        public void TestDatabaseConnection_WithInvalidServer_ShouldThrowException()
        {
            // Arrange
            DbFactory.ConnectionString = "Server=999.999.999.999;Port=3306;Database=gestiparc;User ID=test_user;Password=test_password;SslMode=None;ConnectionTimeout=2;";

            // Act & Assert
            using var connection = DbFactory.Create();
            Assert.Throws<MySqlConnector.MySqlException>(() => connection.Open());
        }
    }
}
