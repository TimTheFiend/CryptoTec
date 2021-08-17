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
    public class HomeController : Controller
    {
        #region Fields

        private readonly DbTools _dbTools;
        private readonly ILogger<HomeController> _logger;  // Came with creation
        private IDataProtector _dataProtecter;
        private readonly IConfiguration _config;

        #endregion Fields

        #region Constructor

        public HomeController(ILogger<HomeController> logger, DbTools dbTools, IDataProtectionProvider dataProvider, IConfiguration config) {
            _dbTools = dbTools;
            _config = config;
            _logger = logger;
            _dataProtecter = dataProvider.CreateProtector(_config["SecretKey"]);
        }

        #endregion Constructor

        #region Index

        [HttpGet]
        public IActionResult Index() {
            return View();
        }

        /// <summary>Checks and verifies if the inputted information matches that of an existing user.</summary>
        /// <param name="username">The string inputted into the username textbox.</param>
        /// <param name="password">The string inputted into the password PasswordBox.</param>
        /// <returns><c>true</c> sends the user to <see cref="UserController.TodoList"/>; otherwise <see cref="Index"/></returns>
        [HttpPost]
        public IActionResult Index(string username, string password) {
            //Check if there is a user with that username in the database.
            Login login = _dbTools.Login.SingleOrDefault(x => x.Username == username);

            if (login != null) {
                if (BC.Verify(password, login.Password)) {  //`BC.Verify` compares the encrypted password to the inputted password.
                    //If successful; set the current session info to be the `User`'s information.
                    HttpContext.Session.SetInt32("userId", login.Id);
                    HttpContext.Session.SetString("username", login.Username);
                    return Redirect("/User/TodoList");
                }
            }
            ViewBag.Message = "Error in login.";
            // just to prevent errors.
            return View();
        }

        #endregion Index

        #region Came with project

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion Came with project
    }
}