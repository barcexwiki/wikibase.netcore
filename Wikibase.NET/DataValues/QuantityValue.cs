using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MinimalJson;

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
        public const String TypeJsonName = "quantity";

        /// <summary>
        /// The name of the <see cref="UpperBound"/> property in the serialized json object.
        /// </summary>
        private const String UpperBoundJsonName = "upperBound";

        /// <summary>
        /// The name of the <see cref="LowerBound"/> property in the serialized json object.
        /// </summary>
        private const String LowerBoundJsonName = "lowerBound";

        /// <summary>
        /// The name of the <see cref="Amount"/> property in the serialized json object.
        /// </summary>
        private const String AmountJsonName = "amount";

        /// <summary>
        /// The name of the <see cref="Unit"/> property in the serialized json object.
        /// </summary>
        private const String UnitJsonName = "unit";

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
        public decimal UpperBound
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        /// <value>The string value.</value>
        public decimal LowerBound
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
        public QuantityValue(Int64 value)
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
        internal QuantityValue(JsonValue value)
        {
            if ( value == null )
                throw new ArgumentNullException("value");

            JsonObject obj = value.asObject();
            this.Amount = Decimal.Parse(obj.get(AmountJsonName).asString(), CultureInfo.InvariantCulture);
            this.Unit = obj.get(UnitJsonName).asString();
            this.UpperBound = Decimal.Parse(obj.get(UpperBoundJsonName).asString(),CultureInfo.InvariantCulture);
            this.LowerBound = Decimal.Parse(obj.get(LowerBoundJsonName).asString(),CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the data type identifier.
        /// </summary>
        /// <returns>Data type identifier.</returns>
        protected override String JsonName()
        {
            return TypeJsonName;
        }

        /// <summary>
        /// Encodes the instance in a <see cref="JsonValue"/>.
        /// </summary>
        /// <returns>Encoded instance.</returns>
        internal override JsonValue Encode()
        {
            return new JsonObject()
                .add(AmountJsonName, (Amount>=0 ? "+" : "") + Amount.ToString(CultureInfo.InvariantCulture))
                .add(UnitJsonName, Unit)
                .add(UpperBoundJsonName, (UpperBound>=0 ? "+" : "") + UpperBound.ToString(CultureInfo.InvariantCulture))
                .add(LowerBoundJsonName, (LowerBound>=0 ? "+" : "") + LowerBound.ToString(CultureInfo.InvariantCulture));
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
            if (Object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, other))
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
            if (Object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, other))
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
                hashCode = (hashCode * Multiplier) ^ (!Object.ReferenceEquals(null, this.Amount) ? this.Amount.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!Object.ReferenceEquals(null, this.LowerBound) ? this.LowerBound.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!Object.ReferenceEquals(null, this.UpperBound) ? this.UpperBound.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!Object.ReferenceEquals(null, this.Unit) ? this.Unit.GetHashCode() : 0);
                return hashCode;
            }
        }

    }
}