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
