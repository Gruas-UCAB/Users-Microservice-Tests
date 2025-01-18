
using Moq;
using UsersMicroservice.core.Application;
using UsersMicroservice.src.department.application.commands.create_department.types;            
using UsersMicroservice.src.department.application.commands.create_user;
using UsersMicroservice.src.department.application.repositories;
using UsersMicroservice.src.department.domain.value_objects;
using UsersMicroservice.src.department.domain;
using UsersMicroservice.src.department.infrastructure;
using UsersMicroservice.src.department.infrastructure.validators;
using Microsoft.AspNetCore.Mvc;
using UsersMicroservice.core.Common;
using MongoDB.Driver;


namespace TestUserMicroservice.departmentTest
{
    public class DepartmentControllerTests
    {

        private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
        private readonly Mock<IIdGenerator<string>> _mockIdGenerator;
        private readonly DepartmentController _controller;

        public DepartmentControllerTests()
        {
            _mockDepartmentRepository = new Mock<IDepartmentRepository>();
            _mockIdGenerator = new Mock<IIdGenerator<string>>();
            _controller = new DepartmentController(_mockDepartmentRepository.Object, _mockIdGenerator.Object);

        }
        
        [Fact(DisplayName = "Test When CreateDepartment Method Returns OkCreatedResult")]
        public async Task CreateDepartment_ReturnsOKCreatedResult_WhenCommandIsValid()
        {
            
            // Arrange
            var command = new CreateDepartmentCommand("TestDepartment");
            var validator = new CreateDepartmentCommandValidator();
            _mockIdGenerator.Setup(x => x.GenerateId()).Returns("5fb9bg1e-37a6-457b-8718-6a832185b5d3");
            _mockDepartmentRepository.Setup(repo => repo.SaveDepartment(It.IsAny<Department>()))
               .ReturnsAsync(new Department(new DepartmentId("5fb9bg1e-37a6-457b-8718-6a832185b5d3")));
            var service = new CreateDepartmentCommandHandler(_mockIdGenerator.Object, _mockDepartmentRepository.Object);

            // Act
            var validationResult = validator.Validate(command);
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            var response = await service.Execute(command);
            var result = await _controller.CreateDepartment(command);
            

            // Assert
            //Assert.Equal("5fb9bg1e-37a6-457b-8718-6a832185b5d3", response.Unwrap().Id.ToString());
            Assert.True(response.IsSuccessful);
            Assert.True(result is CreatedResult);
           


        }

        [Fact(DisplayName = "Test When CreateDepartment Method Returns NotFoundObjectResult")]
        public async Task CreateDepartment_ReturnsBadRequest_WhenCommandIsNull()
        {

            // Arrange
            var command = new CreateDepartmentCommand("");
            var validator = new CreateDepartmentCommandValidator();
            _mockIdGenerator.Setup(x => x.GenerateId()).Returns("5fb9bg1e-37a6-457b-8718-6a832185b5d3");
            _mockDepartmentRepository.Setup(repo => repo.SaveDepartment(It.IsAny<Department>()))
               .ReturnsAsync(new Department(new DepartmentId("5fb9bg1e-37a6-457b-8718-6a832185b5d3")));
            var service = new CreateDepartmentCommandHandler(_mockIdGenerator.Object, _mockDepartmentRepository.Object);

            // Act
            var validationResult = validator.Validate(command);
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            var response = await service.Execute(command);
            var result = await _controller.CreateDepartment(command);


            // Assert
         
            Assert.False(response.IsSuccessful);
            Assert.False(result is CreatedResult);



        }



        //[Fact(Skip = "saltar")]
        [Fact(DisplayName = "Test When CommandValidator Returns True ")]
        public void CreateDepartment_ReturnsTrue_WhenValidationNameIsValid()
        {
            // Arrange
            var command = new CreateDepartmentCommand("TestDepartment");
            var validator = new CreateDepartmentCommandValidator();

            // Act
            var validationResult = validator.Validate(command);
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();


            // Assert
            Assert.True(validationResult.IsValid);
            Assert.True(validationResult.Errors.Count == 0);
      
        }

        [Theory(DisplayName = "Test When CommandValidator Returns False ")]
        [InlineData("")]
        [InlineData("T")]
        [InlineData("TestDepartmentTestDepartmentTestDepartmentTestDepartmentTestDepartment")]
        public void CreateDepartment_ReturnsFalse_WhenValidationNameNotValid(string depaName)
        {
            // Arrange
            var command = new CreateDepartmentCommand(depaName);
            var validator = new CreateDepartmentCommandValidator();

            // Act
            var validationResult = validator.Validate(command);
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.False(validationResult.IsValid);
        }


        [Fact(DisplayName = "Test When GetAllDepartments Method Returns OkObjectResult ")]
        public async Task GetAllDepartments_ReturnsOk_WhenDepartmentsExist()
        {
            // Arrange

            var department1 = Department.Create(
                    new DepartmentId("5fb9bg1e-37a6-457b-8718-6a832185b5d3"),
                    new DepartmentName("TestDepartment")
            );
            var department2 = Department.Create(
                    new DepartmentId("5fb9bg1e-37a6-457b-8719-6a832185b5d3"),
                    new DepartmentName("TestDepartment2")
            );

            var departments = new List<Department>
            {
                department1,
                department2
            };
            _mockDepartmentRepository.Setup(repo => repo.GetAllDepartments())
            .ReturnsAsync(_Optional<List<Department>>.Of(departments));

            // Act
            var result = await _controller.GetAllDepartments();

            // Assert
            Assert.True(result is OkObjectResult);

        }

        [Fact(DisplayName = "Test When GetAllDepartments Method Resturnd NotFoundObjectResult ")]
        public async Task GetAllDepartments_ReturnsNoOk_WhenDepartmentsDontExist()
        {
            // Arrange
            _mockDepartmentRepository.Setup(repo => repo.GetAllDepartments())
            .ReturnsAsync(_Optional<List<Department>>.Empty);

            // Act
            var result = await _controller.GetAllDepartments();

            // Assert
            Assert.True(result is NotFoundObjectResult);

        }



        [Fact(DisplayName = "Test When GetDepartmentById Method Returns OkObjectResult")]
        public async Task GetDepartmentById_ReturnsOk_WhenDepartmentExist()
        {
            // Arrange
            
            var department = Department.Create(
                    new DepartmentId("5fb9bg1e-37a6-457b-8718-6a832185b5d3"),
                    new DepartmentName("TestDepartment")
                    );

            _mockDepartmentRepository.Setup(repo => repo.GetDepartmentById(It.IsAny<DepartmentId>()))
              .ReturnsAsync(_Optional<Department>.Of(department));
            
            // Act
            var result = await _controller.GetDepartmentById("5fb9bg1e-37a6-457b-8718-6a832185b5d3");

            // Assert

            Assert.True(result is OkObjectResult);

        }

        [Fact(DisplayName = "Test When GetDepartmentById Method Returns NotFoundObjectResult")]
        public async Task GetDepartmentById_ReturnsOk_WhenDepartmentDontExist()
        {
            // Arrange
            _mockDepartmentRepository.Setup(repo => repo.GetDepartmentById(It.IsAny<DepartmentId>()))
              .ReturnsAsync(_Optional<Department>.Empty);

            // Act
            var result = await _controller.GetDepartmentById("5fb9bg1e-37a6-457b-8718-6a832185b5d3");

            // Assert

            Assert.True(result is NotFoundObjectResult);

        }


    }
}