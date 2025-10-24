using Application.Common;
using FluentAssertions;

namespace Application.Tests.Common;

public class ResponseBuilderTests
{
    [Fact]
    public void Ok_WithData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var data = "test data";
        var message = "Operation completed successfully";

        // Act
        var response = ResponseBuilder.Ok(data, message);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be(message);
        response.Error.Should().BeNull();
    }

    [Fact]
    public void Ok_WithoutMessage_ShouldUseDefaultMessage()
    {
        // Arrange
        var data = "test data";

        // Act
        var response = ResponseBuilder.Ok(data);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be("Operation completed successfully");
        response.Error.Should().BeNull();
    }

    [Fact]
    public void Ok_WithNullData_ShouldWork()
    {
        // Arrange & Act
        var response = ResponseBuilder.Ok<string>(null);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
        response.Message.Should().Be("Operation completed successfully");
        response.Error.Should().BeNull();
    }

    [Fact]
    public void Error_WithMessage_ShouldReturnErrorResponse()
    {
        // Arrange
        var errorMessage = "An error occurred";

        // Act
        var response = ResponseBuilder.Error<string>(errorMessage);

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be("An error occurred");
        response.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void Error_WithException_ShouldReturnErrorResponse()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var response = ResponseBuilder.Error<string>(exception);

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Error.Should().Be("Test exception");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Error_WithEmptyMessage_ShouldStillWork(string errorMessage)
    {
        // Arrange & Act
        var response = ResponseBuilder.Error<string>(errorMessage);

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void Ok_WithComplexObject_ShouldWork()
    {
        // Arrange
        var complexData = new 
        {
            Id = Guid.NewGuid(),
            Name = "Test Object",
            Value = 123.45m,
            IsActive = true
        };

        // Act
        var response = ResponseBuilder.Ok(complexData);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(complexData);
        response.Message.Should().Be("Operation completed successfully");
        response.Error.Should().BeNull();
    }

    [Fact]
    public void Error_WithComplexErrorObject_ShouldWork()
    {
        // Arrange
        var errorDetails = new 
        {
            Code = "FRAUD_001",
            Description = "Transaction exceeds daily limit",
            Severity = "High"
        };

        // Act
        var response = ResponseBuilder.Error<string>(errorDetails.ToString());

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Error.Should().Contain("FRAUD_001");
    }

    [Fact]
    public void MultipleOkResponses_ShouldWork()
    {
        // Arrange & Act
        var response1 = ResponseBuilder.Ok("test1");
        var response2 = ResponseBuilder.Ok("test2");

        // Assert
        response1.Success.Should().BeTrue();
        response2.Success.Should().BeTrue();
        response1.Data.Should().Be("test1");
        response2.Data.Should().Be("test2");
    }

    [Fact]
    public void Ok_AndError_ShouldHaveConsistentStructure()
    {
        // Arrange & Act
        var successResponse = ResponseBuilder.Ok("data");
        var errorResponse = ResponseBuilder.Error<string>("error");

        // Assert
        // Both should have all required properties
        successResponse.Should().NotBeNull();
        successResponse.Success.Should().BeTrue();

        errorResponse.Should().NotBeNull();
        errorResponse.Success.Should().BeFalse();
    }
}