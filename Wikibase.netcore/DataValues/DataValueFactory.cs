using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wikibase.DataValues
{
    /// <summary>
    /// Factory to create the correct <see cref="DataValue"/> from a <see cref="JToken"/>.
    /// </summary>
    internal static class DataValueFactory
    {
        internal static DataValue CreateFromJsonObject(JToken data)
        {
            return CreateFromJsonValue((string)data[DataValue.ValueTypeJsonName], data[DataValue.ValueJsonName]);
        }

        internal static DataValue CreateFromJsonValue(string type, JToken value)
        {               
            switch (type)
            {
                case EntityIdValue.TypeJsonName:
                    return new EntityIdValue(value);
                case StringValue.TypeJsonName:
                    return new StringValue(value);
                case TimeValue.TypeJsonName:
                    return new TimeValue(value);
                case GlobeCoordinateValue.TypeJsonName:
                    return new GlobeCoordinateValue(value);
                case QuantityValue.TypeJsonName:
                    return new QuantityValue(value);
                case MonolingualTextValue.TypeJsonName:
                    return new MonolingualTextValue(value);
                default:
                    throw new NotSupportedException("Unsupported type " + type);
            }
        }
    }
}