using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using SmartLibrary.Web.Consts;
using SmartLibrary.Web.Core.Models;
using SmartLibrary.Web.Filters;
using SmartLibrary.Web.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;

namespace SmartLibrary.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IDataProtector _dataProtector;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public SubscribersController(ApplicationDbContext context, IMapper mapper,
            IImageService imageService, IDataProtectionProvider dataProtector, IWhatsAppClient whatsAppClient, IWebHostEnvironment webHostEnvironment, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _whatsAppClient = whatsAppClient;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = _context.Subscribers
                            .SingleOrDefault(s =>
                                    s.Email == model.Value
                                || s.NationalId == model.Value
                                || s.MobilePhone == model.Value);

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);
            if(subscriber is not null)
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }

        public IActionResult Details(string id)
        {
            var decryptedId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = _context.Subscribers.Include(s => s.Area).Include(s => s.Governorate).Include(s => s.Subscriptions)
                .Include(s => s.Rentals).ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == decryptedId);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.key = id;
            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            return View("Form", viewModel);
        }

        [AjaxOnly]
        public IActionResult GetAreas(int governorateId)
        {
            var areas = _context.Areas.Where(a => a.GovernorateId == governorateId && !a.IsDeleted)
                     .Select(a => new SelectListItem
                     {
                         Value = a.Id.ToString(),
                         Text = a.Name
                     }).OrderBy(a => a.Text).ToList();

            return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subscriber = _mapper.Map<Subscriber>(model);

            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
            var imagePath = "/images/subscribers";

            var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

            if (!isUploaded)
            {
                ModelState.AddModelError("Image", errorMessage!);
                return View("Form", PopulateViewModel(model));
            }

            subscriber.ImageUrl = $"{imagePath}/{imageName}";
            subscriber.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";
            subscriber.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            Subscription subscription = new Subscription()
            {
                CreatedById = subscriber.CreatedById,
                CreatedOn = subscriber.CreatedOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
            };

            subscriber.Subscriptions.Add(subscription);
            _context.Add(subscriber);
            _context.SaveChanges();

            // Send welcome email
            var placeholders = new Dictionary<string, string>()
                {
                    {"imageUrl", "https://res.cloudinary.com/devagwa/image/upload/v1762438094/icon-positive-vote-1_rdexez_lxhwam_t0tvln.png"},
                    {"header", $"Welcome {model.FirstName}, " },
                    {"body", "thank you for joining SmartLibrary"}
                };
            //Builder
            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(model.Email, "Welcome to SmartLibrary", body));

            //Send Welcome Message using WhatsApp
            if (model.HasWhatsApp)
            {
                    var components = new List<WhatsAppComponent>()
                    {
                        new WhatsAppComponent
                        {
                             Type = "body",
                             Parameters = new List<object>()
                             {
                                new WhatsAppTextParameter {Text = model.FirstName}
                             }
                        }
                    };

                var mobilePhone = _webHostEnvironment.IsDevelopment() ? "01128002767" : model.MobilePhone;
                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage($"2{mobilePhone}",
                WhatsAppLanguageCode.English_US, WhatsAppTemplates.WelcomeMessage, components));


            }
            var encryptedId = _dataProtector.Protect(subscriber.Id.ToString());
            return RedirectToAction(nameof(Details), new { id = encryptedId });
        }


        public IActionResult Edit(string id)
        {
            var decryptedId = int.Parse(_dataProtector.Unprotect(id));

            var subscriber = _context.Subscribers.Find(decryptedId);
            if (subscriber is null)
                return NotFound();
            var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
            viewModel.Key = id;
            return View("Form", PopulateViewModel(viewModel));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.Find(subscriberId);
            if (subscriber is null)
                return NotFound();
            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(subscriber.ImageUrl))
                    _imageService.Delete(subscriber.ImageUrl, subscriber.ImageThumbnailUrl);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var imagePath = "/images/subscribers";
                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);
                if (!isUploaded)
                {
                    ModelState.AddModelError("Image", errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }
                model.ImageUrl = $"{imagePath}/{imageName}";
                model.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";
            }
            else if (!string.IsNullOrEmpty(subscriber.ImageUrl))
            {
                model.ImageUrl = subscriber.ImageUrl;
                model.ImageThumbnailUrl = subscriber.ImageThumbnailUrl;
            }

            subscriber = _mapper.Map(model, subscriber);
            subscriber.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            subscriber.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = model.Key });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewSubscription(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _context.Subscribers.Include(s => s.Subscriptions).SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();
            if (subscriber.IsBlackList)
                return BadRequest();

            var lastSubscription = subscriber.Subscriptions.Last();
            var StartDate = lastSubscription.EndDate < DateTime.Today ? DateTime.Today : lastSubscription.EndDate.AddDays(1);

            Subscription newSubscription = new Subscription()
            {
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CreatedOn = DateTime.Now,
                StartDate = StartDate,
                EndDate = StartDate.AddYears(1)
            };

            subscriber.Subscriptions.Add(newSubscription);
            _context.SaveChanges();


            // Send welcome email
            var placeholders = new Dictionary<string, string>()
                {
                    {"imageUrl", "https://res.cloudinary.com/devagwa/image/upload/v1762438094/icon-positive-vote-1_rdexez_lxhwam_t0tvln.png"},
                    {"header", $"Hello {subscriber.FirstName}, " },
                    {"body", $"Your Subscription Has been Renewed through {newSubscription.EndDate.ToString("d MMM, yyyy")}"}
                };
            //Builder
            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(subscriber.Email, "SmartLibrary Subscription Renewal", body));

            //Send Welcome Message using WhatsApp

            if (subscriber.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                    {
                        new WhatsAppComponent
                        {
                             Type = "body",
                             Parameters = new List<object>()
                             {
                                new WhatsAppTextParameter {Text = subscriber.FirstName},
                                new WhatsAppTextParameter {Text = newSubscription.EndDate.ToString("d MMM, yyyy")}
                             }
                        }
                    };

                var mobilePhone = _webHostEnvironment.IsDevelopment() ? "01128002767" : subscriber.MobilePhone;
                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage($"2{mobilePhone}",
                                    WhatsAppLanguageCode.English, WhatsAppTemplates.SubscriptionRenew, components));

            }
            var viewModel = _mapper.Map<SubscriptionViewModel>(newSubscription);
            return PartialView("_SubscriptionRow", viewModel);

        }


        public IActionResult AllowNationalId(SubscriberFormViewModel model)
        {
            var subscriberId = 0;
            if(!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(s => s.NationalId == model.NationalId);
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);
            return Json(isAllowed);
        }
        public IActionResult AllowMobileNumber(SubscriberFormViewModel model)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(s => s.MobilePhone == model.MobilePhone);
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);
            return Json(isAllowed);
        }
        public IActionResult AllowEmail(SubscriberFormViewModel model)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(s => s.Email == model.Email);
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);
            return Json(isAllowed);
        }


        //public async Task PrepareExpirationAlert()
        //{
        //    var subscribers = _context.Subscribers
        //        .Include(s => s.Subscriptions).Where(s =>s.Subscriptions.OrderByDescending(x => x.EndDate).First().EndDate == DateTime.Today.AddDays(5))
        //        .ToList();
        //    foreach (var subscriber in subscribers)
        //    {
        //        // Send welcome email
        //        var placeholders = new Dictionary<string, string>()
        //        {
        //            {"imageUrl", "https://res.cloudinary.com/devagwa/image/upload/v1762775366/calendar_zfohjc_wz495o.png"},
        //            {"header", $"Hello {subscriber.FirstName}, " },
        //            {"body", $"Your Subscription will be expired by {subscriber.Subscriptions.Last().EndDate.ToString("d MMM, yyyy")}"}
        //        };
        //        //Builder
        //        var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

        //        await _emailSender.SendEmailAsync(subscriber.Email, "SmartLibrary Subscription Expiration", body);


        //        //Send Welcome Message using WhatsApp
        //        if (subscriber.HasWhatsApp)
        //        {
        //            var components = new List<WhatsAppComponent>()
        //            {
        //                new WhatsAppComponent
        //                {
        //                     Type = "body",
        //                     Parameters = new List<object>()
        //                     {
        //                        new WhatsAppTextParameter {Text = subscriber.FirstName},
        //                        new WhatsAppTextParameter {Text = subscriber.Subscriptions.Last().EndDate.ToString("d MMM, yyyy")}
        //                     }
        //                }
        //            };

        //            var mobilePhone = _webHostEnvironment.IsDevelopment() ? "01128002767" : subscriber.MobilePhone;

        //            await _whatsAppClient.SendMessage($"2{mobilePhone}",
        //                               WhatsAppLanguageCode.English, WhatsAppTemplates.SubscriptionExpiration, components);
        //        }
        //    }
        //}
        private SubscriberFormViewModel PopulateViewModel(SubscriberFormViewModel? model = null)
        {
            SubscriberFormViewModel viewModel = model is null ? new SubscriberFormViewModel() : model;

            var governorates = _context.Governorates.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            if (model?.GovernorateId > 0)
            {
                var areas = _context.Areas
                    .Where(a => a.GovernorateId == model.GovernorateId && !a.IsDeleted)
                    .OrderBy(a => a.Name)
                    .ToList();

                viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return viewModel;
        }




    }
}
