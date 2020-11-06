using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services
{
    public enum ErrorCodes
    {
        EmailExists,
        UnKnown,
        NotFound,
        InvalidPassword,
        FirstNameIsRequired,
        StringLengthExceededMaximumAllowedLength,
        LastNameIsRequired,
        CanNotBeEmpty,
        PasswordsAreNotMatched,

        //payment error codes
        PaymentAmountTooSmall,
        PaymentAmountTooLarge,
        PaymentBalanceInsufficient,
        PaymentExpiredCard,
        PaymentUnknownError
    }

    public enum AccountTypes
    {
        Flyer = 1,
        AircraftProvider = 2,
        Admin = 4
    }

    public enum DbUpdateErrorCodes
    {
        UniqueKeyError = 2627
    }

    public enum BookingDirection
    {
        Roundtrip = 1,
        Oneway = 2
    }

    public enum BookingTypes
    {
        CharterAircraft = 2,
        CharterAircraftSeat = 4,
        CommercialSeat = 8,
        CharterFlight = 16,
        CharterFlightSeat = 32,
        CharterAircraftOrSeat = 64,
        CharterAircraftOrComm = 128,
        CharterSeatOrComm = 254,
        Any = 0
    }

    public enum FlightTypes
    {
        CharterAircraft = 2,
        CharterAircraftSeat = 4,
        CommercialSeat = 8,
        CharterFlight = 16,
        CharterFlightSeat = 32
    }

    public enum Titles
    {
        Mr = 1,
        Mrs = 2,
        Ms = 3,
        Miss = 4,
        Master = 5
    }

    public enum DocumentTypes
    {
        Passport = 1,
        DrivingLicense = 2,
        BirthdayCertificate = 3
    }

    public enum NotificationsChannels
    {
        InApp = 2,
        Email = 4,
        TextMessage = 8
    }

    public enum RequestStatuses
    {
        Pending = 1,
        Canceled = 2,
        Completed = 3
    }

    public enum AccountStatuses
    {
        PendingApproval = 1,
        Active = 2,
        PendingConfirmation = 3,
    }

    public enum NotificationsTypes
    {
        NewFlightRequest = 1,
        NewAircraftProviderRegistrationRequest = 2,
        NewFlightRequestMessage = 3,
        NewMessage = 4,
        NewFlyer = 5,
        NewAircraftProvider = 6,
        NewBooking = 7
    }

    public enum FlightStatuses
    {
        OnSchedule = 1,
        Delayed = 2,
        Canceled = 3,
        Departed = 4,
        Landed = 5,
        UnKnown = 6,
    }

    public enum BookingFlightStatuses
    {
        OnSchedule = 1,
        CheckedIn = 2,
        Departed = 3,
        Missed = 4,
        Canceled = 5,
        Unkown = 6
    }

    public enum BookingStatuses
    {
        New = 1,
        PendingPayment = 2,
        PendingMinimumTravelers = 3,
        PendingConfirmation = 4,
        Confirmed = 5
        //Canceled = 6
    }

    public enum TaxableItems
    {
        InclusiveCostOnly = 1,
        InclusiveCostAndAllFees = 2,
        InclusiveCostAndFeesExceptDonations = 3,
        FeesExceptDonations = 4
    }

    public enum ReferenceNumberCodes
    {
        BOOKING
    }

    public enum LocationsTypes
    {
        Airport = 2,//1
        Location = 4,//2
        Camp = 8,//3
        Country = 16,//4
        State = 32,//5
        City = 64//6
    }

    public enum ConversationTopics
    {
        Support = 1,
        FlightRequest = 2
    }

    public enum AircraftTypes
    {
        VeryLightJet = 2,
        LightJet = 4,
        MidSizeJet = 8,
        SuperMidSizeJet = 16,
        HeavyJet = 32
    }

    public enum FilterBookedFlightsBy
    {
        Current = 2,
        Upcoming = 4,
        Historical = 8
    }

    public enum BookingFlightRequestTypes
    {
        Catering = 1,
        Others = 2
    }

    public enum WeekDays
    {
        Sun = 2,
        Mon = 4,
        Tues = 8,
        Wed = 16,
        Thur = 32,
        Fri = 64,
        Sat = 128
    }

    public enum AircraftDocumentTypes
    {
        AirwothinessCertificate = 1,
        InsuranceDocument = 2
    }

    public enum PaymentMethods
    {
        CreditCard = 1,
        BankTransfer = 2
    }
}
