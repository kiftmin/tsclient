using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TrackingClient
{
    public static class Processing
    {
        public enum State
        {
            [Display(Name = "Waiting for Unit")]
            WaitingForUnit = 0,
            [Display(Name = "Unit Detected")]
            UnitDetected = 1,
            [Display(Name = "Unit in Station")]
            UnitInStation = 2,
            [Display(Name = "Reading Tag")]
            ReadingTag = 3,
            [Display(Name = "Tag Read")]
            TagRead = 4,
            [Display(Name = "Processing Unit")]
            ProcessingUnit = 5,
            [Display(Name = "Unit Released")]
            UnitReleased = 6,
            [Display(Name = "Transaction Failed")]
            TransactionFailed = 7
        }

        public static string DisplayName(this Enum value)
        {
            Type enumType = value.GetType();
            var enumValue = Enum.GetName(enumType, value);
            MemberInfo member = enumType.GetMember(enumValue)[0];

            var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            var outString = ((DisplayAttribute)attrs[0]).Name;

            if (((DisplayAttribute)attrs[0]).ResourceType != null)
            {
                outString = ((DisplayAttribute)attrs[0]).GetName();
            }

            return outString;
        }
    }
}
