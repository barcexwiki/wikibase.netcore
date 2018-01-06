using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wikibase.DataValues
{
    /// <summary>
    /// Possible unit values for the <see cref="QuantityValue.Unit"/>.
    /// </summary>
    public enum QuantityUnit
    {
        /// <summary>
        /// Undefined value.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Number without a dimension.
        /// </summary>
        DimensionLess = 1,
    }

    /// <summary>
    /// Encapsulates the quantity value type.
    /// </summary>
    public class QuantityValue : DataValue
    {
        #region Jscon names

        /// <summary>
        /// The identifier of this data type in the serialized json object.
        /// </summary>
        public const string TypeJsonName = "quantity";

        /// <summary>
        /// The name of the <see cref="UpperBound"/> property in the serialized json object.
        /// </summary>
        private const string UpperBoundJsonName = "upperBound";

        /// <summary>
        /// The name of the <see cref="LowerBound"/> property in the serialized json object.
        /// </summary>
        private const string LowerBoundJsonName = "lowerBound";

        /// <summary>
        /// The name of the <see cref="Amount"/> property in the serialized json object.
        /// </summary>
        private const string AmountJsonName = "amount";

        /// <summary>
        /// The name of the <see cref="Unit"/> property in the serialized json object.
        /// </summary>
        private const string UnitJsonName = "unit";

        #endregion Jscon names

        // TODO: Better data structures, string is too general

        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        /// <value>The string value.</value>
        public decimal Amount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unit of measurement.
        /// </summary>
        /// <value>The unit of measurement.</value>
        public string Unit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        /// <value>The string value.</value>
        public decimal? UpperBound
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        /// <value>The string value.</value>
        public decimal? LowerBound
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new quantity value for a exact integer value.
        /// </summary>
        /// <param name="amount">Amount the quantity represents.</param>
        /// <param name="lowerBound">Upper bound of the amount.</param>
        /// <param name="upperBound">Lower bound of the amount.</param>
        /// <param name="unit">Integer value.</param>
        public QuantityValue(decimal amount, decimal lowerBound, decimal upperBound, string unit)
        {
            Amount = amount;
            UpperBound = upperBound;
            LowerBound = lowerBound;
            Unit = unit;
        }

        /// <summary>
        /// Creates a new quantity value for a exact integer value.
        /// </summary>
        /// <param name="value">Integer value.</param>
        public QuantityValue(long value)
        {
            Amount = value;
            UpperBound = Amount;
            LowerBound = Amount;
            Unit = QuantityUnit.DimensionLess.ToString();
        }

        /// <summary>
        /// Parses a <see cref="JsonValue"/> to a <see cref="QuantityValue"/>
        /// </summary>
        /// <param name="value"><see cref="JsonValue"/> to be parsed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a JSON object.</exception>
        internal QuantityValue(JToken value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Type != JTokenType.Object)
                throw new ArgumentException("not a JSON object", nameof(value));

            JObject obj = (JObject)value;

            Amount = decimal.Parse((string)obj[AmountJsonName], CultureInfo.InvariantCulture);
            Unit = (string)obj[UnitJsonName];

            if (obj[UpperBoundJsonName] != null)
            {
                this.UpperBound = decimal.Parse((string)obj[UpperBoundJsonName], CultureInfo.InvariantCulture);
            }
            else
            {
                this.UpperBound = null;
            }
                
            if (obj[LowerBoundJsonName] != null)
            {
                this.LowerBound = decimal.Parse((string)obj[LowerBoundJsonName], CultureInfo.InvariantCulture);
            }
            else
            {
                this.LowerBound = null;
            }            
        }

        /// <summary>
        /// Gets the data type identifier.
        /// </summary>
        /// <returns>Data type identifier.</returns>
        protected override string JsonName
        {
            get
            {
                return TypeJsonName;
            }
        }

        /// <summary>
        /// Encodes the instance in a <see cref="JsonValue"/>.
        /// </summary>
        /// <returns>Encoded instance.</returns>
        internal override JToken Encode()
        {
            JObject j = new JObject
            {
                { AmountJsonName, (Amount >= 0 ? "+" : "") + Amount.ToString(CultureInfo.InvariantCulture) },
                { UnitJsonName, Unit }
            };

            if (UpperBound != null)
                j.Add(new JProperty(UpperBoundJsonName, (UpperBound >= 0 ? "+" : "") + UpperBound.Value.ToString(CultureInfo.InvariantCulture)));

            if (LowerBound != null)
                j.Add(new JProperty(LowerBoundJsonName, (LowerBound >= 0 ? "+" : "") + LowerBound.Value.ToString(CultureInfo.InvariantCulture)));

            return j;
        }


        /// <summary>
        /// Compares two objects of this class and determines if they are equal in value
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        private bool IsEqual(QuantityValue other)
        {
            QuantityValue quantity = other as QuantityValue;

            return (quantity != null)
                && (this.Amount == quantity.Amount)
                && (this.LowerBound == quantity.LowerBound)
                && (this.Unit == quantity.Unit)
                && (this.UpperBound == quantity.UpperBound);
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

            return IsEqual((QuantityValue)other);
        }

        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public bool Equals(QuantityValue other)
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
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Amount) ? this.Amount.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.LowerBound) ? this.LowerBound.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.UpperBound) ? this.UpperBound.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Unit) ? this.Unit.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}