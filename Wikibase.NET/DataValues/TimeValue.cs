using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Wikibase.DataValues
{
    /// <summary>
    /// The precision for a <see cref="TimeValue"/>.
    /// </summary>
    public enum TimeValuePrecision
    {
        /// <summary>
        /// Gigayear, 1 billion years.
        /// </summary>
        GigaYear = 0,

        /// <summary>
        /// 100 megayears, 100 million years.
        /// </summary>
        HundredMegaYears = 1,

        /// <summary>
        /// 10 megayears, 10 million years.
        /// </summary>
        TenMegaYears = 2,

        /// <summary>
        /// 1 megayear, 1 million years.
        /// </summary>
        MegaYear = 3,

        /// <summary>
        /// 100 kiloyears, 100,000 years.
        /// </summary>
        HundredKiloYears = 4,

        /// <summary>
        /// 10 kiloyears, 10,000 years.
        /// </summary>
        TenKiloYears = 5,

        /// <summary>
        /// 1 kiloyear, 1,000 years or one millenium.
        /// </summary>
        Millennium = 6,

        /// <summary>
        /// 100 years or one century.
        /// </summary>
        Century = 7,

        /// <summary>
        /// 10 years or one decade.
        /// </summary>
        Decade = 8,

        /// <summary>
        /// 1 year.
        /// </summary>
        Year = 9,

        /// <summary>
        /// 1 month.
        /// </summary>
        Month = 10,

        /// <summary>
        /// 1 day.
        /// </summary>
        Day = 11,

        /// <summary>
        /// 1 hour.
        /// </summary>
        Hour = 12,

        /// <summary>
        /// 1 minute.
        /// </summary>
        Minute = 13,

        /// <summary>
        /// 1 second.
        /// </summary>
        Second = 14,
    }

    /// <summary>
    /// The calendar models supported by WikiData.
    /// </summary>
    public enum CalendarModel
    {
        /// <summary>
        /// Undefined calendar model.
        /// </summary>
        Unknown,

        /// <summary>
        /// Gregorian calendar, proleptic if necessary.
        /// </summary>
        GregorianCalendar,

        /// <summary>
        /// Julian Calendar.
        /// </summary>
        JulianCalendar
    }

    /// <summary>
    /// Data value for times
    /// </summary>
    public class TimeValue : DataValue
    {
        private struct ParsedTime
        {
            public long year;
            public int month;
            public int day;
            public int hour;
            public int minute;
            public int second;

            public ParsedTime(string time)
            {
                bool valid = false;
                year = 0;
                month = 0;
                day = 0;
                hour = 0;
                minute = 0;
                second = 0;

                Match m = Regex.Match(time, "^(?<year>[+-]\\d{4,11})-(?<month>\\d{2})-(?<day>\\d{2})T(?<hour>\\d{2}):(?<minute>\\d{2}):(?<second>\\d{2})Z$");

                if (m.Success)
                {
                    year = long.Parse(m.Groups["year"].Value);
                    day = int.Parse(m.Groups["day"].Value);
                    month = int.Parse(m.Groups["month"].Value);
                    hour = int.Parse(m.Groups["hour"].Value);
                    minute = int.Parse(m.Groups["minute"].Value);
                    second = int.Parse(m.Groups["second"].Value);
                    valid = (
                                day <= 31 &&
                                month <= 12 &&
                                hour <= 60 &&
                                minute <= 60 &&
                                second <= 30 &&
                                year <= 99999999999 &&
                                year >= -99999999999
                            );
                }

                if (!valid)
                    throw new FormatException("The format of the time is not valid");
            }

            public override string ToString()
            {
                string result = string.Format("{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}Z", year, month, day, hour, minute, second);
                if (year >= 0) result = "+" + result;
                return result;
            }
        }


        #region Json names

        /// <summary>
        /// The identifier of this data type in the serialized json object.
        /// </summary>
        public const string TypeJsonName = "time";

        /// <summary>
        /// The name of the <see cref="DisplayCalendarModel"/> property in the serialized json object.
        /// </summary>
        private const string CalendarModelJsonName = "calendarmodel";

        /// <summary>
        /// The name of the <see cref="Time"/> property in the serialized json object.
        /// </summary>
        private const string TimeJsonName = "time";

        /// <summary>
        /// The name of the <see cref="TimeZoneOffset"/> property in the serialized json object.
        /// </summary>
        private const string TimeZoneJsonName = "timezone";

        /// <summary>
        /// The name of the <see cref="Before"/> property in the serialized json object.
        /// </summary>
        private const string BeforeJsonName = "before";

        /// <summary>
        /// The name of the <see cref="After"/> property in the serialized json object.
        /// </summary>
        private const string AfterJsonName = "after";

        /// <summary>
        /// The name of the <see cref="Precision"/> property in the serialized json object.
        /// </summary>
        private const string PrecisionJsonName = "precision";

        #endregion Json names

        #region private fields

        private static Dictionary<CalendarModel, string> s_calendarModelIdentifiers = new Dictionary<CalendarModel, string>()
        {
             {CalendarModel.GregorianCalendar, "http://www.wikidata.org/entity/Q1985727" },
             {CalendarModel.JulianCalendar, "http://www.wikidata.org/entity/Q1985786"}
        };

        private ParsedTime _time;
        private int _timeZoneOffset;
        private int _before;
        private int _after;

        #endregion private fields

        #region properties

        /// <summary>
        /// Gets or sets the date and time.
        /// </summary>
        /// <value>The date and time.</value>
        public DateTime DateTime
        {
            get
            {
                return GetDateTimeValue();
            }
            set
            {
                Time = value.ToString("+yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }
        }

        private DateTime GetDateTimeValue()
        {
            if (Time.StartsWith("+0000000", StringComparison.Ordinal))
            {
                return DateTime.Parse(Time.Substring(8), CultureInfo.InvariantCulture);
            }
            if (Time.StartsWith("+", StringComparison.Ordinal))
            {
                return DateTime.Parse(Time.Substring(1), CultureInfo.InvariantCulture);
            }
            else
            {
                throw new InvalidOperationException("Time value out of range");
            }
        }

        /// <summary>
        /// Point in time, represented per ISO8601
        /// The year can have up to 11 digits, the date always be signed, in the format +00000002013-01-01T00:00:00Z
        /// </summary>
        public string Time
        {
            get
            {
                return _time.ToString();
            }
            set
            {
                try
                {
                    _time = new ParsedTime(value);
                }
                catch
                {
                    throw new ArgumentException("Time is not in the +yyyyyyyyyyyy-mm-ddThh:mm:ssZ format or it is out of range.", "Time");
                }
            }
        }

        /// <summary>
        /// Timezone information as an offset from UTC in minutes.
        /// </summary>
        public int TimeZoneOffset
        {
            get
            {
                return _timeZoneOffset;
            }
            set
            {
                if (IsValidTimeOffset(value))
                    _timeZoneOffset = value;
                else
                    throw new ArgumentOutOfRangeException("TimeOffset out of range (-720,720)", "TimeOffset");
            }
        }

        /// <summary>
        /// If the date is uncertain, how many units before the given time could it be?
        /// The unit is given by the <see cref="Precision"/>.
        /// </summary>
        public int Before
        {
            get
            {
                return _before;
            }
            set
            {
                if (IsValidBeforeAfter(value))
                    _before = value;
                else
                    throw new ArgumentOutOfRangeException("Before is out of range (cannot be negative)", "Before");
            }
        }

        /// <summary>
        /// If the date is uncertain, how many units after the given time could it be?
        /// The unit is given by the <see cref="Precision"/>.
        /// </summary>
        public int After
        {
            get
            {
                return _after;
            }
            set
            {
                if (IsValidBeforeAfter(value))
                    _after = value;
                else
                    throw new ArgumentOutOfRangeException("After is out of range (cannot be negative)", "After");
            }
        }

        /// <summary>
        /// Unit used for the <see cref="After"/> and <see cref="Before"/> values.
        /// </summary>
        public TimeValuePrecision Precision
        {
            get;
            set;
        }

        /// <summary>
        /// Calendar model that should be used to display this time value.
        /// </summary>
        /// <remarks>
        /// Note that time is always saved in proleptic Gregorian, this URI states how the value should be displayed.
        /// </remarks>
        public CalendarModel DisplayCalendarModel
        {
            get;
            set;
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Creates a new time value with the given settings.
        /// </summary>
        /// <param name="time">Time value in ISO8601 format (with 11 year digits).</param>
        /// <param name="timeZoneOffset">Time zone offset in minutes.</param>
        /// <param name="before">Number of <paramref name="precision">units</paramref> the actual time value could be before the given time value.</param>
        /// <param name="after">Number of <paramref name="precision">units</paramref> the actual time value could be after the given time value.</param>
        /// <param name="precision">Date/time precision.</param>
        /// <param name="calendarModel">Calendar model property.</param>
        public TimeValue(string time, int timeZoneOffset, int before, int after, TimeValuePrecision precision, CalendarModel calendarModel)
        {
            if (!IsValidBeforeAfter(before))
                throw new ArgumentException("Before is out of range (cannot be negative)", "before");
            if (!IsValidBeforeAfter(after))
                throw new ArgumentException("After is out of range (cannot be negative)", "after");

            try
            {
                _time = new ParsedTime(time);
            }
            catch
            {
                throw new ArgumentException("Time is not in a valid format.", "time");
            }


            _timeZoneOffset = timeZoneOffset;
            _before = before;
            _after = after;

            Precision = precision;
            DisplayCalendarModel = calendarModel;
        }

        /// <summary>
        /// Creates a new time value for the give <paramref name="time"/> using the <see cref="TimeValuePrecision.Day"/>.
        /// </summary>
        /// <param name="time">Date value.</param>
        public static TimeValue DateValue(DateTime time)
        {
            return new TimeValue(
                time.ToString("+0000000yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                0,  // timezoneoffset
                0,  // before
                0,  // after
                TimeValuePrecision.Day,
                CalendarModel.GregorianCalendar);
        }

        /// <summary>
        /// Parses a <see cref="JToken"/> to a time value.
        /// </summary>
        /// <param name="value"><see cref="JToken"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a JSON object.</exception>
        internal TimeValue(JToken value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Type != JTokenType.Object)
                throw new ArgumentException("not a JSON object", nameof(value));

            JObject obj = (JObject)value;
            Time = (string)obj[TimeJsonName];
            TimeZoneOffset = (int)obj[TimeZoneJsonName];
            Before = (int)obj[BeforeJsonName];
            After = (int)obj[AfterJsonName];
            Precision = (TimeValuePrecision)(int)obj[PrecisionJsonName];

            string calendar = (string)obj[CalendarModelJsonName];
            if (s_calendarModelIdentifiers.Any(x => x.Value == calendar))
            {
                DisplayCalendarModel = s_calendarModelIdentifiers.First(x => x.Value == calendar).Key;
            }
            else
            {
                DisplayCalendarModel = CalendarModel.Unknown;
            }
        }

        #endregion constructor

        #region methods

        private static bool IsValidTimeOffset(int timeZoneOffset)
        {
            return timeZoneOffset >= -720 && timeZoneOffset <= 720;
        }

        private static bool IsValidBeforeAfter(int amount)
        {
            return amount >= 0;
        }


        /// <summary>
        /// Encodes as a <see cref="JsonValue"/>.
        /// </summary>
        /// <returns>Encoded class.</returns>
        /// <exception cref="InvalidOperationException"><see cref="DisplayCalendarModel"/> is <see cref="CalendarModel.Unknown"/>.</exception>
        internal override JToken Encode()
        {
            if (DisplayCalendarModel == CalendarModel.Unknown)
            {
                throw new InvalidOperationException("Calendar model value not set.");
            }

            JToken j = new JObject
            {
                {TimeJsonName, Time},
                {TimeZoneJsonName, TimeZoneOffset},
                {BeforeJsonName, Before},
                {AfterJsonName, After},
                {PrecisionJsonName, Convert.ToInt32(Precision)},
                {CalendarModelJsonName, s_calendarModelIdentifiers[DisplayCalendarModel]}
            };
            return j;
        }

        /// <summary>
        /// Gets the data type identifier.
        /// </summary>
        /// <returns></returns>
        protected override string JsonName
        {
            get
            {
                return TypeJsonName;
            }
        }

        /// <summary>
        /// Compares two objects of this class and determines if they are equal in value
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        private bool IsEqual(TimeValue other)
        {
            TimeValue otherTime = other as TimeValue;

            return (otherTime != null)
                && (Time == otherTime.Time)
                && (Before == otherTime.Before)
                && (TimeZoneOffset == otherTime.TimeZoneOffset)
                && (Precision == otherTime.Precision)
                && (DisplayCalendarModel == otherTime.DisplayCalendarModel)
                && (After == otherTime.After);
        }

        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public override bool Equals(object other)
        {
            // Is null?
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (object.ReferenceEquals(this, other))
            {
                return false;
            }

            // Is the same type?
            if (other.GetType() != this.GetType())
            {
                return false;
            }

            return IsEqual((TimeValue)other);
        }

        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public bool Equals(TimeValue other)
        {
            // Is null?
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return IsEqual(other);
        }

        /// <summary>
        /// Gets the hash code of this object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // Prime numbers
                const int Base = (int)2166136261;
                const int Multiplier = 16777619;

                int hashCode = Base;
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Time) ? this.Time.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Before) ? this.Before.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.After) ? this.After.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.TimeZoneOffset) ? this.TimeZoneOffset.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.DisplayCalendarModel) ? this.DisplayCalendarModel.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion methods
    }
}