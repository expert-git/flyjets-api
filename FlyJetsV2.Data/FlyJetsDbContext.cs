using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace FlyJetsV2.Data
{
    public class FlyJetsDbContext: DbContext
    {
        private IConfiguration _config;

        public FlyJetsDbContext(IConfiguration config)
        {
            _config = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
          optionsBuilder.UseSqlServer(_config.GetConnectionString("EnvDb"));
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountPaymentMethod> AccountPaymentMethods { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<LocationTree> LocationsTree { get; set; }
        public virtual DbSet<Aircraft> Aircrafts { get; set; }
        public virtual DbSet<AircraftImage> AircraftImages { get; set; }
        public virtual DbSet<AircraftModel> AircraftModels { get; set; }
        public virtual DbSet<AircraftType> AircraftTypes { get; set; }
        public virtual DbSet<AircraftDocument> AircraftDocuments { get; set; }
        public virtual DbSet<AircraftAvailability> AircraftsAvailability { get; set; }
        public virtual DbSet<AircraftAvailabilityLocation> AircraftAvailabilityLocations { get; set; }
        public virtual DbSet<AircraftAvailabilityPeriod> AircraftsAvailabilityPeriods { get; set; }
        public virtual DbSet<FeeType> FeesTypes { get; set; }
        public virtual DbSet<TaxType> TaxesTypes { get; set; }
        public virtual DbSet<FlightRequest> FlightRequests { get; set; }
        public virtual DbSet<AccountFamilyMember> AccountFamilyMembers { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<BookingFlight> BookingFlights { get; set; }
        public virtual DbSet<BookingFlightStatus> BookingFlightStatuses { get; set; }
        public virtual DbSet<BookingFlightTraveler> BookingFlightTravelers { get; set; }
        public virtual DbSet<Flight> Flights { get; set; }
        public virtual DbSet<EmptyLeg> EmptyLegs { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<SearchHistory> SearchHistories { get; set; }
        //public virtual DbSet<RouteAlert> RouteAlerts { get; set; }
        //public virtual DbSet<RouteAlertSpecificDate> RouteAlertSpecificDates { get; set; }
    }
}
