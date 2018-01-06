using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wikibase.DataValues
{
    /// <summary>
    /// Possible values for the <see cref="GlobeCoordinateValue.Globe"/>.
    /// </summary>
    public enum Globe
    {
        /// <summary>
        /// Unknown globe value.
        /// </summary>
        Unknown,

        /// <summary>
        /// Earth.
        /// </summary>
        Earth,
    }

    /// <summary>
    /// Data value for globe coordinates
    /// </summary>
    public class GlobeCoordinateValue : DataValue
    {
        #region Json names

        /// <summary>
        /// The identifier of this data type in the serialized json object.
        /// </summary>
        public const string TypeJsonName = "globecoordinate";

        /// <summary>
        /// The name of the <see cref="Latitude"/> property in the serialized json object.
        /// </summary>
        private const string LatitudeJsonName = "latitude";

        /// <summary>
        /// The name of the <see cref="Longitude"/> property in the serialized json object.
        /// </summary>
        private const string LongitudeJsonName = "longitude";

        /// <summary>
        /// The name of the deprecated altitude property in the serialized json object.
        /// </summary>
        private const string AltitudeJsonName = "altitude";

        /// <summary>
        /// The name of the <see cref="Precision"/> property in the serialized json object.
        /// </summary>
        private const string PrecisionJsonName = "precision";

        /// <summary>
        /// The name of the <see cref="Globe"/> property in the serialized json object.
        /// </summary>
        private const string GlobeJsonName = "globe";

        #endregion Json names

        private static Dictionary<Globe, string> s_globeJsonNames = new Dictionary<Globe, string>()
        {
             {Globe.Earth, "http://www.wikidata.org/entity/Q2" }
        };

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>The latitude.</value>
        public double Latitude
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>The longitude.</value>
        public double Longitude
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        /// <value>The precision.</value>
        public double Precision
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the globe on which the location resides.
        /// </summary>
        /// <value>The globe on which the location resides.</value>
        public Globe Globe
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="precision">The precision.</param>
        /// <param name="globe">The globe on which the location resides.</param>
        public GlobeCoordinateValue(double latitude, double longitude, double precision, Globe globe)
        {
            Latitude = latitude;
            Longitude = longitude;
            Precision = precision;
            Globe = globe;
        }

        /// <summary>
        /// Parses a <see cref="JsonValue"/> to a globe coordinate value.
        /// </summary>
        /// <param name="value"><see cref="JsonValue"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a JSON object.</exception>
        internal GlobeCoordinateValue(JToken value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Type != JTokenType.Object)
                throw new ArgumentException("not a JSON object", nameof(value));

            JObject obj = (JObject)value;
            Latitude = (double)obj[LatitudeJsonName];
            Longitude = (double)obj[LongitudeJsonName];
            if (obj[PrecisionJsonName] != null && obj[PrecisionJsonName].Type != JTokenType.Null)
            {
                Precision = (double)obj[PrecisionJsonName];
            }
            string globe = (string)obj[GlobeJsonName];
            if (s_globeJsonNames.Any(x => x.Value == globe))
            {
                Globe = s_globeJsonNames.First(x => x.Value == globe).Key;
            }
            else
            {
                this.Globe = Globe.Unknown;
            }
        }

        /// <summary>
        /// Gets the type identifier of the type at server side.
        /// </summary>
        /// <returns>The type identifier.</returns>
        protected override string JsonName
        {
            get
            {
                return TypeJsonName;
            }
        }

        /// <summary>
        /// Encodes as a <see cref="JsonValue"/>.
        /// </summary>
        /// <returns>Encoded class.</returns>
        /// <exception cref="InvalidOperationException"><see cref="GlobeCoordinateValue.Globe"/> is <see cref="Wikibase.DataValues.Globe.Unknown"/>.</exception>
        internal override JToken Encode()
        {
            if (Globe == Globe.Unknown)
            {
                throw new InvalidOperationException("Globe value not set.");
            }

            JToken j = new JObject
            {
                { LatitudeJsonName, Latitude },
                { LongitudeJsonName, Longitude },
                { PrecisionJsonName, Precision },
                { GlobeJsonName, s_globeJsonNames[Globe] }
            };
            return j;
        }


        /// <summary>
        /// Compares two objects of this class and determines if they are equal in value
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        private bool IsEqual(GlobeCoordinateValue other)
        {
            GlobeCoordinateValue coordinate = other as GlobeCoordinateValue;

            return (coordinate != null)
                && (this.Globe == coordinate.Globe)
                && (this.Precision == coordinate.Precision)
                && (this.Longitude == coordinate.Longitude)
                && (this.Latitude == coordinate.Latitude);
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

            return IsEqual((GlobeCoordinateValue)other);
        }

        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public bool Equals(GlobeCoordinateValue other)
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
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Globe) ? this.Globe.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Precision) ? this.Precision.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Longitude) ? this.Longitude.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Longitude) ? this.Latitude.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}