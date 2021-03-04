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
        /* Fields */
        private readonly DbTools _dbTools;
        private readonly ILogger<UserController> _logger;  // Came with creation
        private IDataProtector _dataProtecter;
        private readonly IConfiguration _config;

        /* Constructor */
        public UserController(ILogger<UserController> logger, DbTools dbTools, IDataProtectionProvider dataProvider, IConfiguration config)
        {
            _dbTools = dbTools;
            _config = config;
            _logger = logger;
            _dataProtecter = dataProvider.CreateProtector(_config["SecretKey"]);
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

        [HttpGet]
        public IActionResult EditTodo(int id)
        {
            var todo = _dbTools.TodoItem.SingleOrDefault(t => t.Id == id && t.loginId == (int)HttpContext.Session.GetInt32("userId"));
            if (todo == null)
            {
                return Redirect("/");
            }

            ViewBag.Todo = todo;
            return View();
        }

        [HttpPost]
        public IActionResult EditTodo(TodoItem todoItem, int id)
        {
            var _todoItem = _dbTools.TodoItem.SingleOrDefault(x => x.Id == id);
            _todoItem.Title = _dataProtecter.Protect(todoItem.Title);
            _todoItem.Description = _dataProtecter.Protect(todoItem.Description);

            _dbTools.TodoItem.Update(_todoItem);
            _dbTools.SaveChanges();


            return Redirect("/User/TodoList");
        }


        #region FinishTask
        [HttpGet]
        public IActionResult FinishTask(int id)
        {
            var todo = _dbTools.TodoItem.SingleOrDefault(x => x.Id == id && x.loginId == (int)HttpContext.Session.GetInt32("userId"));
            if (todo != null)
            {
                _dbTools.TodoItem.SingleOrDefault(x => x.Id == id).IsDone = true;
                _dbTools.SaveChanges();
            }
            return Redirect("/User/TodoList");
        }
        #endregion FinishTask

        #region DeleteTodo
        [HttpGet]
        public IActionResult DeleteTodo(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Redirect("/");
            }

            var todoItem = _dbTools.TodoItem.SingleOrDefault(x => x.Id == id && x.loginId == userId);

            if (todoItem != null)
            {
                _dbTools.TodoItem.Remove(todoItem);
                _dbTools.SaveChanges();
            }

            return Redirect("/Home/TodoList");
        }
        #endregion DeleteTodo

        #region TodoList
        [HttpGet]
        public IActionResult TodoList()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Redirect("/");
            }

            ViewBag.userId = userId;

            //var foo = _dbTools.Login.SingleOrDefault(x => x.Id == userId).Username;

            List<TodoItem> todos = _dbTools.TodoItem.Where(t => t.loginId == userId).ToList();
            foreach (var todo in todos)
            {
                todo.Title = _dataProtecter.Unprotect(todo.Title);
                todo.Description = _dataProtecter.Unprotect(todo.Description);
            }
            //ViewBag.Data = _dataProtecter;
            ViewBag.Todos = todos;

            return View();
        }

        [HttpPost]
        public IActionResult TodoList(string itemTitle, string itemDescription)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Redirect("/");
            }

            _dbTools.TodoItem.Add(new TodoItem
            {
                Title = _dataProtecter.Protect(itemTitle),
                Description = _dataProtecter.Protect(itemDescription),
                Added = DateTime.UtcNow,
                loginId = (int)userId
            });

            _dbTools.SaveChanges();
            // Lazy
            List<TodoItem> todos = _dbTools.TodoItem.Where(t => t.loginId == userId).ToList();
            foreach (var todo in todos)
            {
                todo.Title = _dataProtecter.Unprotect(todo.Title);
                todo.Description = _dataProtecter.Unprotect(todo.Description);
            }
            ViewBag.Todos = todos;

            return View();
        }
        #endregion TodoList


    }
}
