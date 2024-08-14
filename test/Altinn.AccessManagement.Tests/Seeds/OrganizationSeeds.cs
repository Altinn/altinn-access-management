using System;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Seeds;

public static class OrganizationSeeds
{
    public abstract class OrganizationBase : Party, IParty
    {
        public Party Party => this;
    }

    public class Voss : OrganizationBase
    {
        public new static readonly int PartyId = 200;

        public new static readonly Guid? PartyUuid = new Guid("00000000-0000-0000-0000-000000000200");

        public new static readonly PartyType PartyTypeName = PartyType.Organisation;

        public new static readonly string OrgNumber = "910459880";

        public new static readonly string Name = "Voss AS";

        public new static readonly string UnitType = "AS";

        public new static readonly Organization Organization = new()
        {
            OrgNumber = OrgNumber,
            Name = Name,
            UnitType = UnitType,
            EMailAddress = "hello@voss.no",
        };

        public static Voss Defaults { get; } = new Voss();

        public Voss(params Action<Party>[] modifiers)
        {
            base.PartyId = PartyId;
            base.PartyUuid = PartyUuid;
            base.PartyTypeName = PartyTypeName;
            base.OrgNumber = OrgNumber;
            base.Name = Name;
            base.UnitType = UnitType;
            base.Organization = Organization;

            foreach (var modifer in modifiers)
            {
                modifer(this);
            }
        }
    }

    public class VossConsulting : OrganizationBase
    {
        public new static readonly int PartyId = 201;

        public new static readonly Guid? PartyUuid = new Guid("00000000-0000-0000-0000-000000000201");

        public new static readonly PartyType PartyTypeName = PartyType.Organisation;

        public new static readonly string OrgNumber = "810418982";

        public new static readonly string Name = "Voss Consulting";

        public new static readonly string UnitType = "AS";

        public new static readonly Organization Organization = new()
        {
            OrgNumber = OrgNumber,
            Name = Name,
            UnitType = UnitType,
            EMailAddress = "hello@consulting.voss.no",
        };

        public VossConsulting(params Action<Party>[] modifiers)
        {
            base.PartyId = PartyId;
            base.PartyUuid = PartyUuid;
            base.PartyTypeName = PartyTypeName;
            base.OrgNumber = OrgNumber;
            base.Name = Name;
            base.UnitType = UnitType;
            base.Organization = Organization;

            foreach (var modifer in modifiers)
            {
                modifer(this);
            }
        }

        public static VossConsulting Defaults { get; } = new VossConsulting();
    }

    public class VossAccounting : OrganizationBase
    {
        public new static readonly int PartyId = 202;

        public new static readonly Guid? PartyUuid = new Guid("00000000-0000-0000-0000-000000000202");

        public new static readonly PartyType PartyTypeName = PartyType.Organisation;

        public new static readonly string OrgNumber = "810419172";

        public new static readonly string Name = "Voss Accounting";

        public new static readonly string UnitType = "AS";

        public new static readonly Organization Organization = new()
        {
            OrgNumber = OrgNumber,
            Name = Name,
            UnitType = UnitType,
            EMailAddress = "hello@accounting.voss.no",
        };

        public static VossAccounting Defaults { get; } = new VossAccounting();

        public VossAccounting(params Action<Party>[] modifiers)
        {
            base.PartyId = PartyId;
            base.PartyUuid = PartyUuid;
            base.PartyTypeName = PartyTypeName;
            base.OrgNumber = OrgNumber;
            base.Name = Name;
            base.UnitType = UnitType;
            base.Organization = Organization;

            foreach (var modifer in modifiers)
            {
                modifer(this);
            }
        }
    }
}
