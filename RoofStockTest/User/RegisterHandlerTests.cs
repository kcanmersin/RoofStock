using Core.Data.Entity.User;
using Core.Features.User.Register; 
using Core.Service.Email; 
using Core.Service.JWT; 
using FluentValidation;
using Microsoft.AspNetCore.Identity; 
using Microsoft.Extensions.Configuration; 
using Moq; 
using Xunit;
using FluentAssertions; 
public class RegisterHandlerTests
{
    private readonly Mock<UserManager<Core.Data.Entity.User.AppUser>> _userManagerMock;
    private readonly Mock<RoleManager<AppRole>> _roleManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IValidator<RegisterCommand>> _validatorMock;
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<Core.Data.Entity.User.AppUser>>(
            Mock.Of<IUserStore<Core.Data.Entity.User.AppUser>>(), null, null, null, null, null, null, null, null);

        _roleManagerMock = new Mock<RoleManager<AppRole>>(
            Mock.Of<IRoleStore<AppRole>>(), null, null, null, null);

        _emailServiceMock = new Mock<IEmailService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _validatorMock = new Mock<IValidator<RegisterCommand>>();

        _handler = new RegisterHandler(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _emailServiceMock.Object,
            _jwtServiceMock.Object,
            _validatorMock.Object,
            Mock.Of<IConfiguration>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var command = new RegisterCommand(); 
        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult
            {
                Errors = { new FluentValidation.Results.ValidationFailure("Property", "Error") }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("ValidationFailed");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCreationFails()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<Core.Data.Entity.User.AppUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("RegistrationFailed");
    }

}
