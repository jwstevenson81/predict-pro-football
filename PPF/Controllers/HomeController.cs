using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;
using PPF.Models;
using System.Web.Security;


namespace PPF.Controllers
{
    public class HomeController : Controller
    {
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        public ActionResult Register()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    Roles.AddUserToRole(model.UserName, "Users");
                    FormsService.SignIn(model.UserName, false /* createPersistentCookie */);
                    // we need to assign the new user the current season's standard picks for the weeks
                    // she doesn't have picks
                    var svc = new SeasonService();
                    svc.SetupNewUserMidSeason(model.UserName);
                    //
                    return RedirectToAction("Index", "MyPredict");
                }
                else
                {
                    ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }


        public ActionResult Index()
        {
            var svc = new SeasonService();
            ViewBag.SeasonYear = svc.GetCurrentSeason().SeasonYear.ToString(CultureInfo.InvariantCulture);
            return View();
        }

        public ActionResult HowToPick()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult StandardPicks()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.UserName, model.Password))
                {
                    FormsService.SignIn(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "MyPredict");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult LogOff()
        {
            FormsService.SignOut();
            return RedirectToAction("Index", "Home");
        }


        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var changed = MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                if (changed)
                    return Json("Your password was successfully changed.  We will now re-direct you to the MyPredict page.");
                //    
                return Json("Your current password was invalid or your new password did not meet the length requirement.  Please try again.");
            }
            //    
            return Json("Please correct the listed errors.");
        }
    }
}
