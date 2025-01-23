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
using Microsoft.Extensions.Configuration;
using Sprache;
using UsersMicroservice.src.auth.application.commands.update_credentials.types;
using UsersMicroservice.src.auth.application.commands.update_credentials;
using UsersMicroservice.src.auth.application.commands.recover_password.types;
using UsersMicroservice.src.auth.application.commands.recover_password;
using System.Net;
using System.Xml.Linq;
using UsersMicroservice.core.Infrastructure;

namespace TestUserMicroservice.authTests
{
    public class AuthControllerTests
    {
        private readonly Mock<ICryptoService> _mockCryptoService;
        private readonly Mock<ITokenAuthenticationService> _mockTokenAuthenticationService;
        private readonly Mock<ICredentialsRepository> _mockCredentialsRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockCryptoService = new Mock<ICryptoService>();
            _mockTokenAuthenticationService = new Mock<ITokenAuthenticationService>();
            _mockCredentialsRepository = new Mock<ICredentialsRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockConfiguration = new Mock<IConfiguration>();

            _controller = new AuthController(
                _mockCredentialsRepository.Object,
                _mockUserRepository.Object,
                _mockCryptoService.Object,
                _mockTokenAuthenticationService.Object,
                _mockConfiguration.Object
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
            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>()))
                .ReturnsAsync(_Optional<Credentials>.Of(new Credentials("9fb9be1e-37a6-457b-8719-6a832185b5d3", userId, "test@gamil.com", "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")));
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

            _mockCryptoService.Setup(service => service.Compare(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>()))
                .ReturnsAsync(_Optional<Credentials>.Of(new Credentials("1", "1", "test@example.com", "hashedPassword")));

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

            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>())).
                ReturnsAsync(_Optional<Credentials>.Empty());

            // Act
            var resultAction = await _controller.Login(command);

            // Assert
            Assert.True(resultAction is UnauthorizedObjectResult);
        }

        [Fact(DisplayName = "Test When UpdateCredentials Method Resturns OkObjectResult ")]

        public async Task UpdateCredentials_ValidCommand_ReturnsOkResult()
        {
            // Arrange
            var userId = "2e512140-dfd7-4927-ba48-8986d243f638";
            var user = User.Create(new UserId(userId), new UserName("Test User"), new UserPhone("+584242374999"), new UserRole("admin"), new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"));
            var command = new UpdateCredentialsCommand(userId, "test@gmail.com", "newpassword");

            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByUserId(It.IsAny<string>()))
                .ReturnsAsync(_Optional<Credentials>.Of(new Credentials("9fb9be1e-37a6-457b-8719-6a832185b5d3", userId, "test@gamil.com", "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")));
            _mockCredentialsRepository.Setup(repo => repo.UpdateCredentials(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _mockCryptoService.Setup(service => service.Compare(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(true);

            var service = new UpdateCredentialsCommandHandler(_mockCredentialsRepository.Object, _mockCryptoService.Object);
            _mockCryptoService.Setup(cs => cs.Hash(It.IsAny<string>())).ReturnsAsync("hashedpassword");

            // Act
            var result = await _controller.UpdateCredentials(command);

            // Assert
            Assert.True(result is OkObjectResult);

        }

        [Fact(DisplayName = "Test When UpdateCredentials Method Resturns BadRequestObjectResult ")]

        public async Task UpdateCredentials_InvalidCommand_ReturnsBadRequest()
        {
            // Arrange
            var userId = "2e512140-dfd7-4927-ba48-8986d243f638";
            var user = User.Create(new UserId(userId), new UserName("Test User"), new UserPhone("+584242374999"), new UserRole("admin"), new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"));
            var command = new UpdateCredentialsCommand(userId, "newetest.com", "password");

            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByUserId(It.IsAny<string>()))
                .ReturnsAsync(_Optional<Credentials>.Of(new Credentials("9fb9be1e-37a6-457b-8719-6a832185b5d3", userId, "test@gamil.com", "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")));
            _mockCredentialsRepository.Setup(repo => repo.UpdateCredentials(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _mockCryptoService.Setup(service => service.Compare(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(true);

            var service = new UpdateCredentialsCommandHandler(_mockCredentialsRepository.Object, _mockCryptoService.Object);
            _mockCryptoService.Setup(cs => cs.Hash(It.IsAny<string>())).ReturnsAsync("hashedpassword");

            // Act
            var result = await _controller.UpdateCredentials(command);

            // Assert
            Assert.True(result is BadRequestObjectResult);
        }

        [Fact]
        public async Task RecoverPassword_ReturnsOkResult_ValidCommand()
        {
            // Arrange
            var userId = "2e512140-dfd7-4927-ba48-8986d243f638";
            var command = new RecoverPasswordCommand("test@gmail.com");
            var subject = "Test Subject";
            var body = "Test Body";
            _mockCryptoService.Setup(service => service.Compare(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>()))
                .ReturnsAsync(_Optional<Credentials>.Of(new Credentials("9fb9be1e-37a6-457b-8719-6a832185b5d3", userId, "test@gamil.com", "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")));
     
            _mockCredentialsRepository.Setup(repo => repo.UpdateCredentials(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _mockConfiguration.Setup(config => config["SmtpSettings:SmtpHost"]).Returns("smtp.test.com");
            _mockConfiguration.Setup(config => config["SmtpSettings:SmtpPort"]).Returns("587");
            _mockConfiguration.Setup(config => config["EmailSettings:From"]).Returns("noreply@example.com");
           
           
            var emailContent = new EmailContent(subject, body);
            var service = new RecoverPasswordCommandHandler(_mockCredentialsRepository.Object, _mockCryptoService.Object);


            // Act
            var result = await _controller.RecoverPassword(command);
            var response = await service.Execute(command);


            // Assert

            Assert.True(response.IsSuccessful );

        }

       

        [Fact]
        public async Task RecoverPassword_ReturnsBadRequest_InvalidCommand()
        {
            // Arrange

            var command = new RecoverPasswordCommand("invalidgmail.com");


            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>())).
               ReturnsAsync(_Optional<Credentials>.Empty());

            var service = new RecoverPasswordCommandHandler(_mockCredentialsRepository.Object, _mockCryptoService.Object);


            // Act
            var result = await _controller.RecoverPassword(command);

            // Assert
            Assert.True(result is BadRequestObjectResult);


        }
    }
}
