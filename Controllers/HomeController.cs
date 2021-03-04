using CryptoTec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BC = BCrypt.Net.BCrypt;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.DataProtection;

namespace CryptoTec.Controllers
{
    public class HomeController : Controller {
        /* Fields */
        private readonly DbTools _dbTools;
        private readonly ILogger<HomeController> _logger;  // Came with creation
        private IDataProtector _dataProtecter;
        private readonly IConfiguration _config;

        /* Constructor */
        public HomeController(ILogger<HomeController> logger, DbTools dbTools, IDataProtectionProvider dataProvider, IConfiguration config) {
            _dbTools = dbTools;
            _config = config;
            _logger = logger;
            _dataProtecter = dataProvider.CreateProtector(_config["SecretKey"]);
        }

        /* Temp */
        private int GetUserId()
        {
            return (int) HttpContext.Session.GetInt32("userId");
        }

        #region ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            Login user = _dbTools.Login.SingleOrDefault(x => x.Id == (int)HttpContext.Session.GetInt32("userId"));
            if (user == null)
            {
                return Redirect("/");
            }
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePassword model)
        {
            if (ModelState.IsValid)
            {
                var user = _dbTools.Login.SingleOrDefault(x => x.Id == (int)HttpContext.Session.GetInt32("userId"));
                if (BC.Verify(model.OldPassword, user.Password))
                {
                    _dbTools.Login.SingleOrDefault(x => x.Id == user.Id).Password = BC.HashPassword(model.NewPassword);
                    _dbTools.SaveChanges();
                    return Redirect("/");
                }
                else
                {
                    ModelState.AddModelError("OldPassword", "Error in password");
                }
            }
            return View(model);
        }
        #endregion EndChangePassword


        #region Index
        [HttpGet]
        public IActionResult Index() {
            
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password) {
            Login login = _dbTools.Login.SingleOrDefault(x => x.Username == username);

            if (login != null) {
                if (BC.Verify(password, login.Password)) {
                    HttpContext.Session.SetInt32("userId", login.Id);
                    return Redirect("/Home/TodoList");
                }
            }
            ViewBag.Message = "Error in login.";
            // just to prevent errors.
            return View();
        }
        #endregion Index

        #region DeleteTodo
        [HttpGet]
        public IActionResult DeleteTodo(int id) {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null) {
                return Redirect("/");
            }

            var todoItem = _dbTools.TodoItem.SingleOrDefault(x => x.Id == id && x.loginId == userId);
            
            if (todoItem != null) {
                _dbTools.TodoItem.Remove(todoItem);
                _dbTools.SaveChanges();
            }

            return Redirect("/Home/TodoList");
        }
        #endregion DeleteTodo

        #region UserRegistration
        [HttpGet]
        public IActionResult UserRegistration() {
            return View();
        }

        [HttpPost]
        public IActionResult UserRegistration(string username, string email, string password) {
            Login knownLogin = _dbTools.Login.SingleOrDefault(x => x.Username == username);

            if (knownLogin == null) {
                // Unique username
                Login login = new Login {
                    Username = username,
                    Email = email,
                    Password = BC.HashPassword(password)
                };

                _dbTools.Login.Add(login);
                _dbTools.SaveChanges();

                ViewBag.Message = "User has been successfully created.";
            }
            else {
                ViewBag.Message = "An error has occured.";
            }
            return View();
        }
        #endregion UserRegistration

        #region TodoList
        [HttpGet]
        public IActionResult TodoList() {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null) {
                return Redirect("/");
            }

            ViewBag.userId = userId;

            HttpContext.Session.SetString("username", _dbTools.Login.SingleOrDefault(x => x.Id == userId).Username);
            //var foo = _dbTools.Login.SingleOrDefault(x => x.Id == userId).Username;

            List<TodoItem> todos = _dbTools.TodoItem.Where(t => t.loginId == userId).ToList();
            foreach (var todo in todos) {
                todo.Title = _dataProtecter.Unprotect(todo.Title);
                todo.Description = _dataProtecter.Unprotect(todo.Description);
            }
            //ViewBag.Data = _dataProtecter;
            ViewBag.Todos = todos;

            return View();
        }

        [HttpPost]
        public IActionResult TodoList(string itemTitle, string itemDescription) {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Redirect("/");
            }

            _dbTools.TodoItem.Add(new TodoItem {
                Title = _dataProtecter.Protect(itemTitle),
                Description = _dataProtecter.Protect(itemDescription),
                Added = DateTime.UtcNow,
                loginId = (int)userId
            });

            _dbTools.SaveChanges();

            return View();
        }
        #endregion TodoList

        #region Came with project
        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion Came with project
    }
}
