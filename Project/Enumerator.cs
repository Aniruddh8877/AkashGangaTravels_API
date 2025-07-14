using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public enum BillStatus
    {
        Paid = 1,
        Cancel = 2
    }
    public enum PaymentStatus
    {
        Paid = 1,
        Due = 2
    }
    public enum BookletStatus
    {
        NotSale = 1,
        Sold = 2
    }

    public enum CouponStatus
    {
        NotGenerated = 5,
        Generated = 1,
        Issued = 2,
        PartialyReedeem = 3,
        Reedeem = 4
    }
    public enum Bookby
    {
        Agent = 1,
        Customer = 2
    }
    public enum BookingType
    {
        Direct = 1,
        Enquiry = 2
    }
    public enum BookingStatus
    {
        TourPending = 1,
        TourCompleted = 2,
        TourCancelled = 3
    }
    public enum EnquiryStatus
    {
        Pending = 1,
        Confirm = 2,
        Cancelled = 3
    }
    public enum DestinationType
    {
        Domestic = 1,
        International = 2
    }
    public enum DocType
    {
        Pdf = 1,
        Doc = 2,
        Excel = 3,
        Print = 4
    }
    public enum KeyFor
    {
        Admin = 1,
        Patient = 2,
    }
    public enum Months
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }

    public enum PaymentMode
    {
        CASH = 1,
        ONLINE = 2,
        CHEQUE = 3,
        OTHERS = 4
    }

    public enum Gender
    {
        Male = 2,
        Female = 1,
        Other = 3
    }
    public enum StaffType
    {
        SuperAdmin = 1,
        Admin = 2,
    }

    public enum Status
    {
        Active = 1,
        Inactive = 2
    }

    public enum LoginResult
    {
        Error = 1,
        WrongUserName = 2,
        AccountNotActive = 3,
        WrongPassword = 4,
        Successful = 5
    }
    public enum BackupStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
    }

    public enum FlightOption
    {
        With_Flight = 1,
        Without_Flight = 2
    }
    public enum MealPlan
    {
        MAP = 1,
        AP = 2,
        CP = 3,
    }
}
