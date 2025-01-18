using Microsoft.AspNetCore.Mvc;
using Moq;
using UsersMicroservice.core.Application;
using UsersMicroservice.src.auth.application.repositories;
using UsersMicroservice.src.department.application.repositories;
using UsersMicroservice.src.department.domain.value_objects;
using UsersMicroservice.src.department.domain;
using UsersMicroservice.src.department.infrastructure.validators;
using UsersMicroservice.src.user.application.repositories;
using UsersMicroservice.src.user.infrastructure.validators;
using UsersMicroservice.src.user.application.commands.create_user.types;
using UsersMicroservice.src.user.domain;
using UsersMicroservice.src.user.domain.value_objects;
using UsersMicroservice.src.user.application.commands.create_user;
using UsersMicroservice.src.user.infrastructure;
using UsersMicroservice.core.Common;
using UsersMicroservice.src.auth.application.models;

using UsersMicroservice.src.user.application.repositories.dto;

using UsersMicroservice.src.user.infrastructure.dto;

using UsersMicroservice.src.user.application.commands.update_user.types;
using UsersMicroservice.Core.Common;
using MongoDB.Driver;
using UsersMicroservice.src.department.application.commands.create_department.types;

namespace TestUserMicroservice.userTests
{
    public class UserControllerTests 
    {
        
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ICredentialsRepository> _mockCredentialsRepository;
        private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
        private readonly Mock<IIdGenerator<string>> _mockIdGenerator;
        private readonly Mock<ICryptoService> _mockCryptoService;
        private readonly UserController _controller;

        public UserControllerTests() { 
        
           _mockUserRepository = new Mock<IUserRepository>();
           _mockIdGenerator = new Mock<IIdGenerator<string>>();
           _mockCredentialsRepository = new Mock<ICredentialsRepository>();
           _mockDepartmentRepository = new Mock<IDepartmentRepository>();
           _mockCryptoService = new Mock<ICryptoService>();
           _controller = new UserController(_mockUserRepository.Object,_mockCredentialsRepository.Object,_mockDepartmentRepository.Object,_mockIdGenerator.Object,_mockCryptoService.Object);


        }



        [Fact(DisplayName = "Test When CreateUser Method Resturns OkCreatedResult")]
        public async Task CreateUser_ReturnsCreatedOkResult_WhenCommandIsValid()
        {
            // Arrange
            var command = new CreateUserCommand("Test User", "+584242374999", "admin", "5fb9be1e-37a6-457b-8719-6a832185b5d3",
                "testUser@gmail.com", 
                "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq");
            var department = new Department(new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"));
            var hashedPassword = "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq";
            var userId = "2e512140-dfd7-4927-ba48-8986d243f638";

            _mockDepartmentRepository.Setup(repo => repo.GetDepartmentById(It.IsAny<DepartmentId>()))
                .ReturnsAsync(_Optional<Department>.Of(department));
            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>()))
                .ReturnsAsync(_Optional<Credentials>.Empty);
            _mockIdGenerator.Setup(gen => gen.GenerateId()).Returns(userId);
            _mockCryptoService.Setup(service => service.Hash(It.IsAny<string>())).ReturnsAsync(hashedPassword);
            _mockUserRepository.Setup(repo => repo.SaveUser(It.IsAny<User>())).ReturnsAsync(new User(new UserId(userId)));
            _mockCredentialsRepository.Setup(repo => repo.AddCredentials(It.IsAny<Credentials>())).ReturnsAsync(userId);
            var service = new CreateUserCommandHandler(_mockIdGenerator.Object, _mockCryptoService.Object,
               _mockUserRepository.Object, _mockDepartmentRepository.Object, _mockCredentialsRepository.Object);

            // Act
            var result =  await service.Execute(command);
            var oKresult = await _controller.CreateUser(command);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(userId, result.Unwrap().Id);
            Assert.True(oKresult is CreatedResult);
        }

        [Fact(DisplayName = "Test When CreateUser Method Resturns CredentialsAlreadyExists")]
        public async Task CreateUser_ReturnsBadRequest_CredentialsAlreadyExists()
        {
            // Arrange
            var command = new CreateUserCommand("Test User", "+584242374999", "admin", "5fb9be1e-37a6-457b-8719-6a832185b5d3",
                "testUser@gmail.com",
                "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq");
            var department = new Department(new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"));
            var hashedPassword = "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq";
            var userId = "2e512140-dfd7-4927-ba48-8986d243f638";
            var credentials = new Credentials("2e512140-dfd7-4927-ba48-8986d243f638", 
                "2e512140-dfd7-4927-ba48-8986d243f638", "testUser@gmail.com", "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq");
            _mockDepartmentRepository.Setup(repo => repo.GetDepartmentById(It.IsAny<DepartmentId>()))
                .ReturnsAsync(_Optional<Department>.Of(department));
            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>()))
                .ReturnsAsync(_Optional<Credentials>.Of(credentials));
            _mockIdGenerator.Setup(gen => gen.GenerateId()).Returns(userId);
            _mockCryptoService.Setup(service => service.Hash(It.IsAny<string>())).ReturnsAsync(hashedPassword);
            _mockUserRepository.Setup(repo => repo.SaveUser(It.IsAny<User>())).ReturnsAsync(new User(new UserId(userId)));
            _mockCredentialsRepository.Setup(repo => repo.AddCredentials(It.IsAny<Credentials>())).ReturnsAsync(userId);
            var service = new CreateUserCommandHandler(_mockIdGenerator.Object, _mockCryptoService.Object,
               _mockUserRepository.Object, _mockDepartmentRepository.Object, _mockCredentialsRepository.Object);

            // Act
            var result = await service.Execute(command);
            var oKresult = await _controller.CreateUser(command);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.False(oKresult is CreatedResult);
        }

        [Fact(DisplayName = "Test When CreateUser Method Resturns BadRequest")]
        public async Task CreateUser_ReturnsBadRequest_WhenCommandIsNotValid()
        {
            // Arrange
            var command = new CreateUserCommand("", "", "", "","","");
            var department = new Department(new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"));
            var hashedPassword = "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq";
            var userId = "2e512140-dfd7-4927-ba48-8986d243f638";

            _mockDepartmentRepository.Setup(repo => repo.GetDepartmentById(It.IsAny<DepartmentId>()))
                .ReturnsAsync(_Optional<Department>.Of(department));
            _mockCredentialsRepository.Setup(repo => repo.GetCredentialsByEmail(It.IsAny<string>()))
                .ReturnsAsync(_Optional<Credentials>.Empty());
            _mockIdGenerator.Setup(gen => gen.GenerateId()).Returns(userId);
            _mockCryptoService.Setup(service => service.Hash(It.IsAny<string>())).ReturnsAsync(hashedPassword);
            _mockUserRepository.Setup(repo => repo.SaveUser(It.IsAny<User>())).ReturnsAsync(new User(new UserId(userId)));
            _mockCredentialsRepository.Setup(repo => repo.AddCredentials(It.IsAny<Credentials>())).ReturnsAsync(userId);
            var service = new CreateUserCommandHandler(_mockIdGenerator.Object, _mockCryptoService.Object,
               _mockUserRepository.Object, _mockDepartmentRepository.Object, _mockCredentialsRepository.Object);

            // Act
            var result = await service.Execute(command);
            var oKresult = await _controller.CreateUser(command);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.False(oKresult is CreatedResult);
        }

        [Fact(DisplayName = "Test When CommandValidator Returns True ")]
        public void CreateUser_ReturnsTrue_WhenValidationInfoIsValid()
        {
            // Arrange
            var command = new CreateUserCommand("Test User", "+584242374999", "admin", "5fb9be1e-37a6-457b-8719-6a832185b5d3",
                "testUser@gmail.com",
                "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq");
            var validator = new CreateUserCommandValidator();

            // Act
            var validationResult = validator.Validate(command);
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();


            // Assert
            Assert.True(validationResult.IsValid);
            Assert.True(validationResult.Errors.Count == 0);

        }

        [Theory(DisplayName = "Test When CommandValidator Returns False ")]
        [InlineData("", "+584242374999", "admin", "5fb9be1e-37a6-457b-8719-6a832185b5d3","testUser@gmail.com",
                "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")]
        [InlineData("test user", "", "admin", "5fb9be1e-37a6-457b-8719-6a832185b5d3","testUser@gmail.com",
                "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")]
        [InlineData("test user", "+584242374999", "", "5fb9be1e-37a6-457b-8719-6a832185b5d3",
                "testUser@gmail.com","$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")]
        [InlineData("test user", "+584242374999", "admin", "","testUser@gmail.com",
                "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")]
        [InlineData("test user", "+584242374999", "admin", "5fb9be1e-37a6-457b-8719-6a832185b5d3", "",
                "$2a$11$HQ4egknPnudciE3EznetcerpuMxy / zcqmgFiYeFgSs2JTLJQRMuZq")]
        [InlineData("test user", "+584242374999", "5fb9be1e-37a6-457b-8719-6a832185b5d3", "testUser@gmail.com",
                "testUser@gmail.com","")]
     
        public void CreateUser_ReturnsFalse_WhenValidationInfoIsNotValid(string userName, string userPhone, string userRol, 
            string userDep, string userEmail, string userPassw)
        {
            // Arrange
            var command = new CreateUserCommand(userName, userPhone, userRol, userDep,
                userEmail,userPassw);
            var validator = new CreateUserCommandValidator();

            // Act
            var validationResult = validator.Validate(command);
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.False(validationResult.IsValid);
        }




        [Fact(DisplayName = "Test When GetAllUsers Method Returns OkObjectResult")]
        public async Task GetAllUsers_ReturnsOkResult_WhenUsersExist()
        {
            // Arrange
            var usersTestList = new List<User>
        {
            User.Create(new UserId("2e512140-dfd7-4927-ba48-8986d243f638"), 
                        new UserName("Test User1"), 
                        new UserPhone("+584242374999"), 
                        new UserRole("provider"), 
                        new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3")),
            User.Create(new UserId("2e512141-dfd7-4927-ba48-8986d243f638"), 
                        new UserName("Test User2"), 
                        new UserPhone("+594242374999"), 
                        new UserRole("admin"), 
                        new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"))
        };
           
            var optionalUsers = _Optional<List<User>>.Of(usersTestList);
            var dto = new GetAllUsersDto();

            _mockUserRepository.Setup(repo => repo.GetAllUsers(dto)).ReturnsAsync(optionalUsers);

            // Act
            var result = await _controller.GetAllUsers(dto);

            // Assert
            Assert.True(result is OkObjectResult);

        }

        [Fact(DisplayName = "Test When GetAllUsers Method Returns NotFoundObjectResult")]
        public async Task GetAllUsers_ReturnsNoOkResult_WhenUsersDontExist()
        {
            // Arrange
            var optionalUsers = _Optional<List<User>>.Empty;
            var dto = new GetAllUsersDto();

            _mockUserRepository.Setup(repo => repo.GetAllUsers(dto)).ReturnsAsync(optionalUsers);

            // Act
            var result = await _controller.GetAllUsers(dto);

            // Assert
            Assert.True(result is NotFoundObjectResult);

        }



        [Fact(DisplayName = "Test When GetUsersById Method Resturns OkObjectResult ")]
        public async Task GetUserById_ReturnsOkResult_WhenUsersExist()
        {
            // Arrange
           
            var user = User.Create(new UserId("2e512140-dfd7-4927-ba48-8986d243f638"),
                        new UserName("Test User1"),
                        new UserPhone("+584242374999"),
                        new UserRole("provider"),
                        new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"));
            var optionalUser = _Optional<User>.Of(user);
            
            _mockUserRepository.Setup(repo => repo.GetUserById(It.IsAny<UserId>())).ReturnsAsync(optionalUser);

            // Act
          
            var result = await _controller.GetUserById("2e512140-dfd7-4927-ba48-8986d243f638");

            // Assert
            Assert.True(result is OkObjectResult);

        }

        [Fact(DisplayName = "Test When GetUsersById Method Resturns NotFoundObjectResul ")]
        public async Task GetUserById_ReturnsNoOkResult_WhenUsersDontExist()
        {
            // Arrange
            var optionalUser = _Optional<User>.Empty;

            _mockUserRepository.Setup(repo => repo.GetUserById(It.IsAny<UserId>())).ReturnsAsync(optionalUser);

            // Act

            var result = await _controller.GetUserById("2e512140-dfd7-4927-ba48-8986d243f638");

            // Assert
            Assert.True(result is NotFoundObjectResult);

        }



        [Fact(DisplayName = "Test When ToggleActivityUserById Method Resturns OkObjectResult ")]
        public async Task ToggleActivityUserById_ReturnsOkResult_UserExists_()
        {
            // Arrange
            var userId = "2e512140-dfd7-4927-ba48-8986d243f638";
            var user = User.Create(
                new UserId(userId), 
                new UserName("New Name"), 
                new UserPhone("+584242374999"), 
                new UserRole("admin"), 
                new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"));
            var optionalUser = _Optional<User>.Of(user);

            _mockUserRepository.Setup(repo => repo.GetUserById(It.IsAny<UserId>())).ReturnsAsync(optionalUser);
            _mockUserRepository.Setup(repo => repo.ToggleActivityUserById(It.IsAny<UserId>())).ReturnsAsync(new UserId(userId));
           
            // Act
            var result = await _controller.ToggleActivityUserById(userId);

            // Assert
            Assert.True(result is OkObjectResult);
        }

        [Fact(DisplayName = "Test When ToggleActivityUserById Method Resturns BadRequestObjectResult ")]
        public async Task ToggleActivityUserById_ReturnsNoOkResult_UserDontExists_()
        {
            // Arrange
            var userId = "2e512140-dfd7-4927-ba48-8986d243f638";
            var user = User.Create(
                new UserId(userId),
                new UserName("New Name"),
                new UserPhone("+584242374999"),
                new UserRole("admin"),
                new DepartmentId("5fb9be1e-37a6-457b-8719-6a832185b5d3"));
            var optionalUser = _Optional<User>.Of(user);
            _mockUserRepository.Setup(repo => repo.GetUserById(It.IsAny<UserId>())).ReturnsAsync(optionalUser);
            _mockUserRepository.Setup(repo => repo.ToggleActivityUserById(It.IsAny<UserId>())).ReturnsAsync(new UserId(userId));

            // Act
            var result = await _controller.ToggleActivityUserById("");

            // Assert
             Assert.True(result is BadRequestObjectResult);
        }


        [Fact(DisplayName = "Test When UpdateUserById Method Resturns OkObjectResult")]
        public async Task UpdateUserById_ReturnsOkResult_ValidData()
        {
            // Arrange
            var id = "2e512140-dfd7-4927-ba48-8986d243f638";
            var data = new UpdateUserDto("New Name", "+584242374999");
            var command = new UpdateUserByIdCommand(id, data.Name, data.Phone);
            var response = Result<UpdateUserByIdResponse>.Success(new UpdateUserByIdResponse(id));

            _mockUserRepository.Setup(repo => repo.UpdateUserById(It.IsAny<UpdateUserByIdCommand>())).ReturnsAsync(new UserId(id));

            // Act
            var result = await _controller.UpdateUserById(data, id);

            // Assert
            Assert.True(result is OkObjectResult);
        }

        [Fact(DisplayName = "Test When UpdateUserById Method Resturns  BadRequestObjectResult")]
        public async Task UpdateUserById_ReturnsBadRequest_InvalidData()
        {
            // Arrange
            var id = "2e512140-dfd7-4927-ba48-8986d243f638";
            var data = new UpdateUserDto(null, null);
            var validator = new UpdateUserByIdValidator();

            // Act
            var result = await _controller.UpdateUserById(data, id);

            // Assert
            Assert.True(result is BadRequestObjectResult);
        }

     

   






    }
}
