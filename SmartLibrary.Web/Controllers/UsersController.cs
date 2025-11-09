using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SmartLibrary.Web.Consts;
using SmartLibrary.Web.Core.Models;
using SmartLibrary.Web.Filters;
using SmartLibrary.Web.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SmartLibrary.Web.Controllers
{

    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment, IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var viewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            //var roles = await _roleManager.Roles.ToListAsync();
            var viewModel = new UserFormViewModel
            {
                Roles = await _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToListAsync()
            };
            return PartialView("_Form", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>  Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            ApplicationUser user = new ApplicationUser()
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                //CreatedById = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value
            };
            IdentityResult result = await _userManager.CreateAsync(user, model.Password!);
            if (result.Succeeded)
            {

                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code},
                    protocol: Request.Scheme);
                var placeholders = new Dictionary<string, string>()
                {
                    {"imageUrl", "https://res.cloudinary.com/devagwa/image/upload/v1762438094/icon-positive-vote-1_rdexez_lxhwam_t0tvln.png"},
                    {"header", $"Hey {user.FullName}, thanks For Joining Us!" },
                    {"body", "Please Confirm Your Eamil"},
                    {"url", $"{HtmlEncoder.Default.Encode(callbackUrl!)}"},
                    {"linkTitle", "Activate Account!"}
                };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Email, placeholders);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);


                var userViewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", userViewModel);
            }
            
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }


        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            var userRoles = await _userManager.GetRolesAsync(user);
            var viewModel = _mapper.Map<UserFormViewModel>(user);
            viewModel.SelectedRoles = userRoles.ToList();
            viewModel.Roles = await _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            }).ToListAsync();
            return PartialView("_Form", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id!);

            if (user is null)
                return NotFound();

            user = _mapper.Map(model, user);
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var RolesUpdated = !currentRoles.SequenceEqual(model.SelectedRoles);

                if (RolesUpdated)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }
                await _userManager.UpdateSecurityStampAsync(user);

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }


        [HttpGet]
        [AjaxOnly]
        public IActionResult ResetPassword(string id)
        {
            var user = _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            var viewModel = new ResetPasswordFormViewModel() { };

            return PartialView("_ResetPasswordForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user is null)
                return NotFound();

            var currentPasswordHash = user.PasswordHash;
            await _userManager.RemovePasswordAsync(user);

            IdentityResult result = await _userManager.AddPasswordAsync(user, model.Password);
            if (result.Succeeded)
            {
                user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                user.LastUpdatedOn = DateTime.Now;

                await _userManager.UpdateAsync(user);

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }
            user.PasswordHash = currentPasswordHash;
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatusAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            user.IsDeleted = !user.IsDeleted;
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.Now;

            await _userManager.UpdateAsync(user);
            if (user.IsDeleted)
                await _userManager.UpdateSecurityStampAsync(user);
            
            return Ok(user.LastUpdatedOn.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var isLocked = await _userManager.IsLockedOutAsync(user);
            if(isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            return Ok();
        }

        public async Task<IActionResult> AllowUserName(UserFormViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            var isAllowed = user is null || user.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowEmail(UserFormViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var isAllowed = user is null || user.Id.Equals(model.Id);

            return Json(isAllowed);
        }
    }
}
