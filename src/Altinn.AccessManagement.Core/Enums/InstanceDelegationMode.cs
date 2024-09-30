using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InstanceDelegationMode
    {
        /// <summary>
        /// Normal instance delegation
        /// </summary>
        [EnumMember(Value = "normal")]
        [PgName("normal")]
        Normal,

        /// <summary>
        /// Special case of instance delegation extends delegations to organizations to all users in the receiving organization with parallel role/package to also getting access to the instance
        /// </summary>
        [EnumMember(Value = "parallelSigning")]
        [PgName("parallelsigning")]
        ParallelSigning
    }
}
