using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WITS.Web.Clients;
using WITS.Web.Components.Pages;
using Xunit;

namespace WITS.Web.Tests.Components.Pages;

public class LoginTests : TestContext
{
    private readonly Mock<AuthService> _authServiceMock;
    private readonly Mock<NavigationManager> _navigationManagerMock;

    public LoginTests()
    {
        _authServiceMock = new Mock<AuthService>(MockBehavior.Strict, null, null);
        _navigationManagerMock = new Mock<NavigationManager>();

        Services.AddSingleton(_authServiceMock.Object);
        Services.AddSingleton(_navigationManagerMock.Object);
        Services.AddBootstrap();
    }

    [Fact]
    public async Task Login_WithValidCredentials_NavigatesToHome()
    {
        // Arrange
        _authServiceMock.Setup(x => x.LoginAsync("testuser", "password"))
            .ReturnsAsync(true);
        _navigationManagerMock.Setup(x => x.NavigateTo("/"))
            .Verifiable();

        var cut = RenderComponent<Login>();

        // Act
        cut.Find("#username").Change("NathanK");
        cut.Find("#password").Change("T!mbach2");
        cut.Find("form").Submit();

        // Assert
        _authServiceMock.Verify();
        _navigationManagerMock.Verify();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShowsErrorMessage()
    {
        // Arrange
        _authServiceMock.Setup(x => x.LoginAsync("testuser", "wrongpassword"))
            .ReturnsAsync(false);

        var cut = RenderComponent<Login>();

        // Act
        cut.Find("#username").Change("testuser");
        cut.Find("#password").Change("wrongpassword");
        cut.Find("form").Submit();

        // Assert
        var errorMessage = cut.Find(".alert-danger");
        Assert.Contains("Invalid username or password", errorMessage.TextContent);
    }

    [Fact]
    public async Task Login_WithEmptyFields_ShowsValidationMessages()
    {
        // Arrange
        var cut = RenderComponent<Login>();

        // Act
        cut.Find("form").Submit();

        // Assert
        var validationMessages = cut.FindAll(".validation-message");
        Assert.Equal(2, validationMessages.Count);
        Assert.Contains("Username is required", validationMessages[0].TextContent);
        Assert.Contains("Password is required", validationMessages[1].TextContent);
    }

    [Fact]
    public async Task Login_WithShortUsername_ShowsValidationMessage()
    {
        // Arrange
        var cut = RenderComponent<Login>();

        // Act
        cut.Find("#username").Change("ab");
        cut.Find("form").Submit();

        // Assert
        var validationMessage = cut.Find(".validation-message");
        Assert.Contains("Username must be between 3 and 50 characters", validationMessage.TextContent);
    }

    [Fact]
    public async Task Login_WithShortPassword_ShowsValidationMessage()
    {
        // Arrange
        var cut = RenderComponent<Login>();

        // Act
        cut.Find("#password").Change("12345");
        cut.Find("form").Submit();

        // Assert
        var validationMessage = cut.Find(".validation-message");
        Assert.Contains("Password must be at least 6 characters", validationMessage.TextContent);
    }

    [Fact]
    public async Task Login_WithNetworkError_ShowsErrorMessage()
    {
        // Arrange
        _authServiceMock.Setup(x => x.LoginAsync("testuser", "password"))
            .ThrowsAsync(new Exception("Network error"));

        var cut = RenderComponent<Login>();

        // Act
        cut.Find("#username").Change("testuser");
        cut.Find("#password").Change("password");
        cut.Find("form").Submit();

        // Assert
        var errorMessage = cut.Find(".alert-danger");
        Assert.Contains("An error occurred during login", errorMessage.TextContent);
    }

    [Fact]
    public async Task Login_WithValidInput_ShowsValidState()
    {
        // Arrange
        var cut = RenderComponent<Login>();

        // Act
        cut.Find("#username").Change("validuser");
        cut.Find("#password").Change("validpassword");

        // Assert
        var usernameInput = cut.Find("#username");
        var passwordInput = cut.Find("#password");
        Assert.Contains("is-valid", usernameInput.ClassName);
        Assert.Contains("is-valid", passwordInput.ClassName);
    }
}
