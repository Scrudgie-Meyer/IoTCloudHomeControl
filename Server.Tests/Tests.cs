using Server.Controllers;
using Server.Data.DBManager;

namespace Server.Tests
{
    public class UserControllerTests
    {
        private static DBSetup _dbContext;
        private UserController _controller;

        //[OneTimeSetUp]
        //public void GlobalSetup()
        //{
        //    var options = new DbContextOptionsBuilder<DBSetup>()
        //        .UseInMemoryDatabase("SharedTestDB")
        //        .Options;

        //    _dbContext = new DBSetup(options);
        //}

        //[SetUp]
        //public void Setup()
        //{
        //    // Очистити базу перед кожним тестом
        //    _dbContext.Users.RemoveRange(_dbContext.Users);
        //    _dbContext.SaveChanges();

        //    _controller = new UserController(_dbContext);
        //}

        //[Test]
        //public async Task CreateUser_ShouldReturnCreatedUser()
        //{
        //    var user = new User { Username = "NewUser", Email = "new@mail.com", PasswordHash = "abc" };
        //    var result = await _controller.CreateUser(user);

        //    Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
        //    var created = (result.Result as CreatedAtActionResult)?.Value as User;
        //    Assert.AreEqual("NewUser", created?.Username);
        //}

        //[Test]
        //public async Task GetUser_ShouldReturnCorrectUser()
        //{
        //    var user = new User { Username = "GetUser", Email = "get@mail.com", PasswordHash = "xyz" };
        //    _dbContext.Users.Add(user);
        //    _dbContext.SaveChanges();

        //    var result = await _controller.GetUser(user.Id);
        //    Assert.IsInstanceOf<OkObjectResult>(result.Result);
        //    var fetched = (result.Result as OkObjectResult)?.Value as User;
        //    Assert.AreEqual("GetUser", fetched?.Username);
        //}

        //[Test]
        //public async Task DeleteUser_ShouldRemoveUser()
        //{
        //    var user = new User { Username = "DeleteMe", Email = "del@mail.com", PasswordHash = "qwe" };
        //    _dbContext.Users.Add(user);
        //    _dbContext.SaveChanges();

        //    var result = await _controller.DeleteUser(user.Id);
        //    Assert.IsInstanceOf<NoContentResult>(result);

        //    var check = await _controller.GetUser(user.Id);
        //    Assert.IsInstanceOf<NotFoundResult>(check.Result);
        //}
    }
}
