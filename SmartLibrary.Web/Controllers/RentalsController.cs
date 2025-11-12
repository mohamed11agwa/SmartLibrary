using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using SmartLibrary.Web.Consts;
using SmartLibrary.Web.Core.Enums;
using SmartLibrary.Web.Core.Models;
using System.Security.Claims;

namespace SmartLibrary.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;

        public RentalsController(ApplicationDbContext context, IMapper mapper, IDataProtectionProvider dataProtector)
        {
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");

        }

        public IActionResult Create(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _context.Subscribers.Include(s => s.Subscriptions).Include(s => s.Rentals)
                .ThenInclude(s => s.RentalCopies).SingleOrDefault(s => s.Id == subscriberId);
                     
            if(subscriber is  null)
                return NotFound();

            var(errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var viewModel = new RentalFormViewModel()
            {
                SubscriberKey = sKey,
                MaxAllowedCopies = maxAllowedCopies
            };
            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var (rentalsError, copies) = ValidateCopies(model.SelectedCopies, subscriberId);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            Rental rental = new()
            {
                RentalCopies = copies,
                StartDate = DateTime.Now,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            subscriber.Rentals.Add(rental);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });

        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult getCopyDetails(SearchFormViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.Include(c => c.Book)
                .SingleOrDefault(c => c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book!.IsDeleted);
           
            if(copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if(!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Errors.NotAvailableRental);

            //check that copy is not in rental
            var copyIsInRental = _context.RentalCopies.Any(c => c.BookCopyId == copy.Id && !c.ReturnDate.HasValue);

            if(copyIsInRental)
                return BadRequest(Errors.CopyIsInRental);

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_CopyDetails", viewModel);
        
        }



        private (string errorMessage, int? maxAllowedCopies) ValidateSubscriber(Subscriber subscriber)
        {
            if (subscriber.IsBlackList)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration))
                return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);

            var currentRentals = subscriber.Rentals.SelectMany(r => r.RentalCopies).Count(c => !c.ReturnDate.HasValue);
            var availableCopiesCount = (int)RentalsConfigurations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return (errorMessage:Errors.MaxCopiesReached, maxAllowedCopies: null);

            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
        }


        private (string errorMessage, ICollection<RentalCopy> copies) ValidateCopies(IEnumerable<int> selectedSerials, int subscriberId, int? rentalId = null)
        {
            var selectedCopies = _context.BookCopies
                .Include(c => c.Book)
                .Include(c => c.Rentals)
                .Where(c => selectedSerials.Contains(c.SerialNumber))
                .ToList();

            var currentSubscriberRentals = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .Where(r => r.SubscriberId == subscriberId && (rentalId == null || r.Id != rentalId))
                .SelectMany(r => r.RentalCopies)
                .Where(c => !c.ReturnDate.HasValue)
                .Select(c => c.BookCopy!.BookId)
                .ToList();

            List<RentalCopy> copies = new();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return (errorMessage: Errors.NotAvailableRental, copies);

                if (copy.Rentals.Any(c => !c.ReturnDate.HasValue && (rentalId == null || c.RentalId != rentalId)))
                    return (errorMessage: Errors.CopyIsInRental, copies);

                if (currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                    return (errorMessage: $"This subscriber already has a copy for '{copy.Book.Title}' Book", copies);

                copies.Add(new RentalCopy { BookCopyId = copy.Id });
            }

            return (errorMessage: string.Empty, copies);
        }

    }
}
