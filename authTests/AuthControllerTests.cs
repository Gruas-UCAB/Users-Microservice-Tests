using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersMicroservice.core.Application;
using UsersMicroservice.core.Common;
using UsersMicroservice.Core.Common;
using UsersMicroservice.src.auth.application.commands.login.types;
using UsersMicroservice.src.auth.application.models;
using UsersMicroservice.src.auth.application.repositories;
using UsersMicroservice.src.auth.infrastructure;
using UsersMicroservice.src.department.domain.value_objects;
using UsersMicroservice.src.user.application.repositories;
using UsersMicroservice.src.user.domain.value_objects;
using UsersMicroservice.src.user.domain;
using UsersMicroservice.src.auth.application.commands.login;

namespace TestUserMicroservice.authTests
{
    public class AuthControllerTests
    {
        private readonly Mock<ICryptoService> _mockCryptoService;
        private readonly Mock<ITokenAuthenticationService> _mockTokenAuthenticationService;
        private readonly Mock<ICredentialsRepository> _mockCredentialsRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockCryptoService = new Mock<ICryptoService>();
            _mockTokenAuthenticationService = new Mock<ITokenAuthenticationService>();
            _mockCredentialsRepository = new Mock<ICredentialsRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _controller = new AuthController(
                _mockCredentialsRepository.Object,
                _mockUserRepository.Object,
                _mockCryptoService.Object,
                _mockTokenAuthenticationService.Object
            );
        }

        [Fact(DisplayName = "Test When Login Method Returns OkObjectResult")]
        public async Task Login_ReturnsOkResult_ValidCredentials()
        {
            // Arrange
            var userId = "2e512140-dfd7-4927-ba48-8986d243f638";
            var accessToken = "token";
            var command = new LoginUserCommand("test@gmail.com", "testpassword");
            var user = User.Create(new UserId(userId), new UserName("Test User"), new UserPhone("+584242374999"), new UserRole("admin"), new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"));
            var response = new LoginUserResponse(user, accessToken, 3600);
            var result = Result<LoginUserResponse>.Success(response);

            _mockCryptoService.Setup(service => service.Compare(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>())).ReturnsAsync(_Optional<Credentials>.Of(new Credentials("9fb9be1e-37a6-457b-8719-6a832185b5d3", userId, "test@gamil.com", "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")));
            _mockUserRepository.Setup(repo => repo.GetUserById(It.IsAny<UserId>())).ReturnsAsync(_Optional<User>.Of(user));
            _mockTokenAuthenticationService.Setup(service => service.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(new TokenResponse(accessToken, 3600));
            
            // Act
          
            var resultAction = await _controller.Login(command);

            // Assert
            Assert.True(resultAction is OkObjectResult);
            


        }

        [Fact(DisplayName = "Test When Login Method Resturns UnauthorizedObjectResult")]
        public async Task Login_ReturnsUnauthorizedResult_InvalidCredentials()
        {
            // Arrange
            var command = new LoginUserCommand("test@gmail.com", "wrongpassword");
            var result = Result<LoginUserResponse>.Failure(new Exception("Invalid credentials"));

            _mockCryptoService.Setup(service => service.Compare(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>())).ReturnsAsync(_Optional<Credentials>.Of(new Credentials("1", "1", "test@example.com", "hashedPassword")));

            // Act
            var resultAction = await _controller.Login(command);

            // Assert
            Assert.True(resultAction is UnauthorizedObjectResult);

        }

        [Fact(DisplayName = "Test When Login Method Resturns UnauthorizedObjectResult NoUser")]
        public async Task Login_ReturnsUnauthorizedResult_UserNotFound()
        {
            // Arrange
            var command = new LoginUserCommand("test@example.com", "password");

            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>())).ReturnsAsync(_Optional<Credentials>.Empty());

            // Act
            var resultAction = await _controller.Login(command);

            // Assert
            Assert.True(resultAction is UnauthorizedObjectResult);
        }


    }
}
