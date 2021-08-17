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
    public class UserController : Controller
    {
        #region Fields

        private readonly ILogger<UserController> _logger;  // Came with creation
        private readonly DbTools _dbTools;
        private IDataProtector _dataProtecter;
        private readonly IConfiguration _config;

        #endregion Fields

        #region Constructor

        public UserController(ILogger<UserController> logger, DbTools dbTools, IDataProtectionProvider dataProvider, IConfiguration config) {
            _dbTools = dbTools;
            _config = config;
            _logger = logger;
            _dataProtecter = dataProvider.CreateProtector(_config["SecretKey"]);
        }

        #endregion Constructor

        #region ChangePassword

        /// <summary>Gets the proper View.</summary>
        [HttpGet]
        public IActionResult ChangePassword() {
            Login user = _dbTools.Login.SingleOrDefault(x => x.Id == (int)HttpContext.Session.GetInt32("userId"));
            if (user == null) {
                return Redirect("/");
            }
            return View();
        }

        // Attempts to change/update the user's password after model `ChangePassword` does superficial verification.
        [HttpPost]
        public IActionResult ChangePassword(ChangePassword model) {
            if (ModelState.IsValid) {
                var user = _dbTools.Login.SingleOrDefault(x => x.Id == (int)HttpContext.Session.GetInt32("userId"));
                if (BC.Verify(model.OldPassword, user.Password)) {
                    // Should have used `_dbTools.Login.Update` instead.
                    _dbTools.Login.SingleOrDefault(x => x.Id == user.Id).Password = BC.HashPassword(model.NewPassword);
                    _dbTools.SaveChanges();
                    return Redirect("/");
                }
                else {
                    // In case the user mistypes their old password, send the following error.
                    ModelState.AddModelError("OldPassword", "Error in password");
                }
            }
            // If there are errors, return the same view, and show the errors to the user.
            return View(model);
        }

        #endregion ChangePassword

        [HttpGet]
        public IActionResult ChangeEmailUsername() {
            var _user = _dbTools.Login.SingleOrDefault(u => u.Id == (int)HttpContext.Session.GetInt32("userId"));
            ViewBag.User = new Login {
                Username = _user.Username,
                Email = _user.Email,
                Id = _user.Id
            };
            return View();
        }

        /// <summary>
        /// Attempts to change the User's credentials.
        /// </summary>
        /// <param name="model">The user.</param>
        /// <param name="id">The user's Id.</param>
        /// <returns><see cref="Index"/>.</returns>
        [HttpPost]
        public IActionResult ChangeEmailUsername(Login model, int id) {
            if (ModelState.IsValid) {
                Login _user = _dbTools.Login.SingleOrDefault(u => u.Id == id);
                if (_user != null) {
                    // Did the user write in either textbox? Should be checked in the razor page instead.
                    if (!String.IsNullOrEmpty(model.Email)) {
                        _user.Email = model.Email;
                    }
                    if (!String.IsNullOrEmpty(model.Username)) {
                        // If the user changed `Username`, update the Session to reflect that.
                        _user.Username = model.Username;
                        HttpContext.Session.SetString("username", _user.Username);
                    }
                    _dbTools.Login.Update(_user);
                    _dbTools.SaveChanges();

                    return Redirect("/User/TodoList");
                }
            }
            return View(model);
        }

        #region EditTodo

        [HttpGet]
        public IActionResult EditTodo(int id) {
            var todo = _dbTools.TodoItem.SingleOrDefault(t => t.Id == id && t.loginId == (int)HttpContext.Session.GetInt32("userId"));
            if (todo == null) {
                return Redirect("/");
            }
            //Unprotect, since we've encrypted every item.
            todo.Title = _dataProtecter.Unprotect(todo.Title);
            todo.Description = _dataProtecter.Unprotect(todo.Description);
            ViewBag.Todo = todo;
            return View();
        }

        /// <summary>
        /// Updates the selected <see cref="TodoItem"/> with new data.
        /// </summary>
        /// <param name="todoItem">NOT USED</param>
        /// <param name="id">The object's Id, which isn't user submitted.</param>
        /// <param name="itemTitle">The object's title.</param>
        /// <param name="itemDescription">The object's description</param>
        /// <returns><see cref="Index"/>.</returns>
        [HttpPost]
        public IActionResult EditTodo(TodoItem todoItem, int id, string itemTitle, string itemDescription) {
            var _todoItem = _dbTools.TodoItem.SingleOrDefault(x => x.Id == id);
            _todoItem.Title = _dataProtecter.Protect(itemTitle);
            _todoItem.Description = _dataProtecter.Protect(itemDescription);

            _dbTools.TodoItem.Update(_todoItem);
            _dbTools.SaveChanges();

            return Redirect("/User/TodoList");
        }

        #endregion EditTodo

        #region FinishTask

        /// <summary>
        /// Sets a <see cref="TodoItem"/> as finished in the database.
        /// </summary>
        /// <param name="id">The <see cref="TodoItem"/>'s Id.</param>
        /// <returns><see cref="Index"/>.</returns>
        [HttpGet]
        public IActionResult FinishTask(int id) {
            var todo = _dbTools.TodoItem.SingleOrDefault(x => x.Id == id && x.loginId == (int)HttpContext.Session.GetInt32("userId"));
            if (todo != null) {
                //Should have used the Update method instead.
                _dbTools.TodoItem.SingleOrDefault(x => x.Id == id).IsDone = true;
                _dbTools.SaveChanges();
            }
            return Redirect("/User/TodoList");
        }

        #endregion FinishTask

        #region DeleteTodo

        /// <summary>
        /// Attempts to delete a <see cref="TodoItem"/> object from the database.
        /// </summary>
        /// <param name="id">The <see cref="TodoItem"/>'s id.</param>
        /// <returns><see cref="Index"/>.</returns>
        [HttpGet]
        public IActionResult DeleteTodo(int id) {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null) {
                return Redirect("/");
            }
            //Does the `TodoItem` object exist, and does it belong to the current user?
            var todoItem = _dbTools.TodoItem.SingleOrDefault(x => x.Id == id && x.loginId == userId);

            if (todoItem != null) {
                _dbTools.TodoItem.Remove(todoItem);
                _dbTools.SaveChanges();
            }

            return Redirect("/Home/TodoList");
        }

        #endregion DeleteTodo

        #region TodoList

        [HttpGet]
        public IActionResult TodoList() {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null) {
                return Redirect("/");
            }

            ViewBag.userId = userId;

            //Gets all the Logged-in user's `TodoItems` into a list that gets displayed in the view.
            List<TodoItem> todos = _dbTools.TodoItem.Where(t => t.loginId == userId).ToList();
            foreach (var todo in todos) {
                todo.Title = _dataProtecter.Unprotect(todo.Title);
                todo.Description = _dataProtecter.Unprotect(todo.Description);
            }
            ViewBag.Todos = todos;

            return View();
        }

        /// <summary>
        /// Attempts to add a new <see cref="TodoItem"/> to the database.
        /// </summary>
        /// <param name="itemTitle">Title of the <see cref="TodoItem"/>.</param>
        /// <param name="itemDescription">Description of the <see cref="TodoItem"/>.</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult TodoList(string itemTitle, string itemDescription) {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null) {
                return Redirect("/");
            }

            _dbTools.TodoItem.Add(new TodoItem {
                //Encrypting the data.
                Title = _dataProtecter.Protect(itemTitle),
                Description = _dataProtecter.Protect(itemDescription),
                Added = DateTime.UtcNow,  //Get current time.
                loginId = (int)userId  //Should be unnecessary to cast it as an int.
            });

            _dbTools.SaveChanges();
            //Collects the Logged-In user's `TodoItem`s from the database and returns the current view.
            List<TodoItem> todos = _dbTools.TodoItem.Where(t => t.loginId == userId).ToList();
            foreach (var todo in todos) {
                todo.Title = _dataProtecter.Unprotect(todo.Title);
                todo.Description = _dataProtecter.Unprotect(todo.Description);
            }
            ViewBag.Todos = todos;

            return View();
        }

        #endregion TodoList
    }
}