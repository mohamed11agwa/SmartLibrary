namespace SmartLibrary.Web.Core.ViewModels
{
    public class RentalFormViewModel
    {
        public string SubscriberKey { get; set; } = null!;

        public IEnumerable<int> SelectedCopies { get; set; } = new List<int>();
    }
}
