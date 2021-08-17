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
    public class RegistrationController : Controller
    {
        #region Fields

        private readonly DbTools _dbTools;
        private readonly ILogger<RegistrationController> _logger;  // Came with creation
        private IDataProtector _dataProtecter;
        private readonly IConfiguration _config;

        #endregion Fields

        #region Constructor

        public RegistrationController(ILogger<RegistrationController> logger, DbTools dbTools, IDataProtectionProvider dataProvider, IConfiguration config) {
            _dbTools = dbTools;
            _config = config;
            _logger = logger;
            _dataProtecter = dataProvider.CreateProtector(_config["SecretKey"]);
        }

        #endregion Constructor

        #region UserRegistration

        [HttpGet]
        public IActionResult UserRegistration() {
            return View();
        }

        /// <summary>
        /// Attempts to add the current user to the database.
        /// </summary>
        /// <param name="username">Their username</param>
        /// <param name="email">Their email</param>
        /// <param name="password">Their password</param>
        /// <returns><see cref="Index"/> but with a different message based on if successful or not.</returns>
        [HttpPost]
        public IActionResult UserRegistration(string username, string email, string password) {
            Login knownLogin = _dbTools.Login.SingleOrDefault(x => x.Username == username);

            if (knownLogin == null) // Unique username
            {
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
    }
}