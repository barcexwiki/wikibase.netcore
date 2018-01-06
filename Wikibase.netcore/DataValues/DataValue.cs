using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wikibase.DataValues
{
    /// <summary>
    /// Abstract base class for a data value.
    /// </summary>
    public abstract class DataValue
    {
        #region Json names

        /// <summary>
        /// Json name for the datavalue type.
        /// </summary>
        public const string ValueTypeJsonName = "type";

        /// <summary>
        /// Json name for the datavalue content.
        /// </summary>
        public const string ValueJsonName = "value";

        #endregion Json names

        /// <summary>
        /// Get the hash.
        /// </summary>
        /// <returns>The hash.</returns>
        public string GetHash()
        {
            return Md5(this.Encode().ToString());
        }

        private static string Md5(string text)
        {
            if ((text == null) || (text.Length == 0))
            {
                return string.Empty;
            }
            byte[] result;
            MD5 md5provider = null;
            try
            {
                md5provider = MD5.Create();
                result = md5provider.ComputeHash(Encoding.GetEncoding(0).GetBytes(text));
            }
            finally
            {
                if (md5provider != null)
                    md5provider.Dispose();
            }
            return System.BitConverter.ToString(result);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same
        /// type.
        ///</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.</returns>
        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (other == null)
            {
                return false;
            }
            if (this.GetType() != other.GetType())
            {
                return false;
            }
            DataValue otherDataValue = (DataValue)other;
            return this.Encode() == otherDataValue.Encode();
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Encode().GetHashCode();
        }

        /// <summary>
        /// Converts the instance to a string.
        /// </summary>
        /// <returns>String representation of the instance.</returns>
        public override string ToString()
        {
            return Encode().ToString();
        }

        /// <summary>
        /// Get the type of the data value.
        /// </summary>
        /// <returns>Data type identifier.</returns>
        protected abstract string JsonName { get;  }

        /// <summary>
        /// Encode the value part of the data value to json.
        /// </summary>
        /// <returns>The json value.</returns>
        internal abstract JToken Encode();

        /// <summary>
        /// Encode the data value to json.
        /// </summary>
        /// <returns>The json value.</returns>
        internal JToken FullEncode()
        {
            JToken j = new JObject
            {
                { ValueTypeJsonName, JsonName },
                { ValueJsonName, Encode() }
            };
            return j;
        }
    }
}