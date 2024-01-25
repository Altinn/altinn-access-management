using System;
using System.Collections;
using System.Collections.Generic;
using Altinn.AccessManagement.Core.Asserts;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Xunit;

namespace Altinn.AccessManagement.Tests.Asserters;

/// <summary>
/// summary
/// </summary>
public class AsserterTests
{
    /// <summary>
    /// summary
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <returns></returns>
    public static IAssert<TModel> Asserter<TModel>() => new Asserter<TModel>();
}