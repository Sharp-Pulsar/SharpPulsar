using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;

namespace SharpPulsar.Sql.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.type.ParameterKind.java
    /// </summary>
    [JsonConverter(typeof(ParameterKindConverter))]
    public enum ParameterKind
    {
        [Description("TYPE_SIGNATURE")]
        Type,

        [Description("NAMED_TYPE_SIGNATURE")]
        NamedType,

        [Description("LONG_LITERAL")]
        Long,

        [Description("")]
        Variable
    }
}
