using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Dojo_Activity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Dojo_Activity.Controllers
{
    public class HomeController : Controller
    {
        private DojoACContext dbContext;

        public HomeController(DojoACContext context)
        {
            dbContext = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost("register")]
        public IActionResult Register(IndexViewModel modelData)
        {
            User registeredUser = modelData.RegisteredUser;
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == registeredUser.Email))
                {
                    ModelState.AddModelError("RegisteredUser.Email", "Email already in use!");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    registeredUser.Password = Hasher.HashPassword(registeredUser, registeredUser.Password);
                    dbContext.Add(registeredUser);
                    dbContext.SaveChanges();
                    User currUser = dbContext.Users.FirstOrDefault(u => u.Email == registeredUser.Email);
                    HttpContext.Session.SetInt32("userId", currUser.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            return View("Index", modelData);
        }

        [HttpPost("login")]
        public IActionResult CheckUser(IndexViewModel modelData)
        {
            LoginUser loggedUser = modelData.LoggedUser;
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == loggedUser.Email);

                if (userInDb == null)
                {
                    ModelState.AddModelError("LoggedUser.Email", "Invalid Email/Password");
                }
                else
                {
                    var hasher = new PasswordHasher<LoginUser>();
                    var result = hasher.VerifyHashedPassword(loggedUser, userInDb.Password, loggedUser.Password);

                    if (result == 0)
                    {
                        ModelState.AddModelError("LoggedUser.Email", "Invalid Email/Password");
                    }
                    else
                    {
                        HttpContext.Session.SetInt32("userId", userInDb.UserId);
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            return View("Index", modelData);
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("userId") == null)
            {
                return RedirectToAction("Index");
            }

            User thisUser = dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("userId"));
            ViewBag.ThisUser = thisUser;

            List<Actvty> AllActivities = dbContext.Activities
                .Include(w => w.Participants)
                .ThenInclude(a => a.User)
                .OrderBy(ea => ea.ActivityDate)
                .ToList();

            foreach (Actvty a in AllActivities.ToList())
            {
                if (a.ActivityDate < DateTime.Now)
                {
                    AllActivities.Remove(a);
                }
            }
            ViewBag.AllActivities = AllActivities;

            List<User> Creators = dbContext.Users.ToList();
            ViewBag.Creators = Creators;

            return View("Dashboard");
        }

        [HttpGet("delete/{actId}")]
        public IActionResult DeleteActivity(int actId)
        {
            Actvty deleteActivity = dbContext.Activities.FirstOrDefault(w => w.ActivityId == actId);
            dbContext.Activities.Remove(deleteActivity);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("join/{actId}")]
        public IActionResult YesActivity(int actId)
        {
            Actvty thisActivity = dbContext.Activities.FirstOrDefault(a => a.ActivityId == actId);
            User usersActivities = dbContext.Users
                .Include(a => a.AttendedActivities)
                .ThenInclude(e => e.Activity)
                .ToList().FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("userId"));
            
            bool attendAct = true;
            foreach (var a in usersActivities.AttendedActivities)
            {
                if (a.Activity.ActivityDate.Date == thisActivity.ActivityDate.Date)
                {
                    attendAct = false;
                }
            }

            if (attendAct)
            {
                Participation participation = new Participation();
                participation.UserId = (int)HttpContext.Session.GetInt32("userId");
                participation.ActivityId = actId;
                dbContext.Participations.Add(participation);
                dbContext.SaveChanges();
            }

            return RedirectToAction("Dashboard");
        }

        [HttpGet("leave/{partId}")]
        public IActionResult NoActivity(int partId)
        {
            Participation participation = dbContext.Participations.FirstOrDefault(a => a.ParticipationId == partId);
            dbContext.Participations.Remove(participation);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("New")]
        public IActionResult NewActivity()
        {
            if (HttpContext.Session.GetInt32("userId") == null)
            {
                return RedirectToAction("Index");
            }
            return View("AddActivity");
        }

        [HttpPost("AddActivity")]
        public IActionResult AddActivity(Actvty newActivity)
        {
            if(ModelState.IsValid)
            {
                newActivity.PlannerId = (int)HttpContext.Session.GetInt32("userId");
                dbContext.Add(newActivity);
                dbContext.SaveChanges();
                Actvty thisActivity = dbContext.Activities.OrderByDescending(w => w.CreatedAt).FirstOrDefault();
                return Redirect("/activity/"+thisActivity.ActivityId);
            }
            return View("AddActivity", newActivity);
        }

        [HttpGet("activity/{actId}")]
        public IActionResult WeddInfo(int actId)
        {
            if (HttpContext.Session.GetInt32("userId") == null)
            {
                return RedirectToAction("Index");
            }

            User thisUser = dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("userId"));
            ViewBag.ThisUser = thisUser;

            Actvty thisActivity = dbContext.Activities.FirstOrDefault(w => w.ActivityId == actId);
            ViewBag.ThisActivity = thisActivity;

            User eventCoord = dbContext.Users.FirstOrDefault(ec => ec.UserId == thisActivity.PlannerId);
            ViewBag.EventCoordinator = eventCoord;

            var actParticipants = dbContext.Activities
                .Include(w => w.Participants)
                .ThenInclude(u => u.User)
                .FirstOrDefault(w => w.ActivityId == actId);
            
            ViewBag.AllParticipants = actParticipants.Participants;
            return View("ActivityInfo");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
