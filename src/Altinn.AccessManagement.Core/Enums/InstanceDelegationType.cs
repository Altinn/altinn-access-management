﻿using NpgsqlTypes;
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
    public enum InstanceDelegationType
    {
        /// <summary>
        /// Defining a instance delegation to be of type parallell task person this could also be identified with "Fødselsnummer"/"Dnummer"
        /// </summary>
        [EnumMember(Value = "paralell")]
        [PgName("paralell")]
        Paralell,

        /// <summary>
        /// Identifies a unit could also be identified with a Organization number
        /// </summary>
        [EnumMember(Value = "end-user")]
        [PgName("endUser")]
        EndUser
    }
}
