namespace SmartLibrary.Web.Core.ViewModels
{
    public class RentalFormViewModel
    {
        public int? Id { get; set; }
        public string SubscriberKey { get; set; } = null!;

        public IEnumerable<int> SelectedCopies { get; set; } = new List<int>();

        public IEnumerable<BookCopyViewModel> CurrentCopies { get; set; } = new List<BookCopyViewModel>();
        public int? MaxAllowedCopies { get; set; }
    }
}
