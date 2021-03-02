using CryptoTec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoTec.Controllers
{
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        private readonly DbTools _dbTools;


        public HomeController(ILogger<HomeController> logger, DbTools dbTools) {
            _dbTools = dbTools;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index() {
            return View();
        }

        [HttpGet]
        public IActionResult UserRegistration() {
            return View();
        }

        [HttpGet]
        public IActionResult TodoList() {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null) {
                return Redirect("/");
            }

            ViewBag.userId = userId;
            List<TodoItem> todos = _dbTools.TodoItem.Where(t => t.loginId == userId).ToList();
            ViewBag.Todos = todos;

            return View();
        }

        [HttpPost]
        public IActionResult TodoList(string itemTitle, string itemDescription) {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null) {
                return Redirect("/");
            }

            _dbTools.TodoItem.Add(new TodoItem {
                Title = itemTitle,
                Description = itemDescription,
                Added = DateTime.UtcNow,
                loginId = (int)userId
            });

            _dbTools.SaveChanges();

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
                    Password = password
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



        [HttpPost]
        public IActionResult Index(string username, string password) {
            Login login = _dbTools.Login.SingleOrDefault(x => x.Username == username);

            if (login != null) {
                if (login.Password == password) {
                    // Successfully logged in
                    HttpContext.Session.SetInt32("userId", login.Id);
                    //ViewBag.Message = "You've logged in, homie.";
                    return Redirect("/Home/TodoList");
                }
                else {
                    // Wrong password
                    ViewBag.Message = "Wrong password!";
                }
            }
            else {
                // Wrong username
                ViewBag.Message = "There's no user with that username!";
            }

            // just to prevent errors.
            return View();
        }



        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
