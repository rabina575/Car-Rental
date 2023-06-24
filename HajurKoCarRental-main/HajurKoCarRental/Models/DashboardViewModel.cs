namespace HajurKoCarRental.Models
{
    public class DashboardViewModel
    {
        public int CarsCount { get; set; }
        public int OffersCount { get; set; }
        public int PassengersCount { get; set; }
        public int ActivePassengersCount { get; set; }
        public int InactivePassengersCount { get; set; }
        public int CarRequestsCount { get; set; }
        public List<CarRequest> CarRequests { get; set; }
        public List<Sales> SalesCounts { get; set; }
    }

    public class Sales
    {
        public string CarName { get; set; }
        public int SalesCount { get; set; }
    }
}
