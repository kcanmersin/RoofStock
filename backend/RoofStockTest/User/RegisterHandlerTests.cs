using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Data.Entity.User;
using Core.Features.User.Register;
using Core.Service.Email;
using Core.Service.JWT;
using Core.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

public class RegisterHandlerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<RoleManager<AppRole>> _roleManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IValidator<RegisterCommand>> _validatorMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
        _roleManagerMock = new Mock<RoleManager<AppRole>>(
            Mock.Of<IRoleStore<AppRole>>(), null, null, null, null);
        _emailServiceMock = new Mock<IEmailService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _validatorMock = new Mock<IValidator<RegisterCommand>>();
        _configurationMock = new Mock<IConfiguration>();

        _handler = new RegisterHandler(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _emailServiceMock.Object,
            _jwtServiceMock.Object,
            _validatorMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCreationFails()
    {
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "1234567890",
            RoleName = "User"
        };

        var validationResult = new FluentValidation.Results.ValidationResult();
        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("RegistrationFailed", result.Error.Code);
        Assert.Equal("User creation failed.", result.Error.Message);
    }
}