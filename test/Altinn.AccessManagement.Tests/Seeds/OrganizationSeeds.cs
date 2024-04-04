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

    public class OrstadAccounting : OrganizationBase
    {
        public new static int PartyId = 50000101;

        public new static Guid? PartyUuid = new Guid("df274071-9baa-44c3-84ee-9b77d3e82f87");

        public new static PartyType PartyTypeName = PartyType.Organisation;

        public new static string OrgNumber = "910459880";

        public new static string Name = "Orstad Accounting";

        public new static string UnitType = "AS";

        public new static Organization Organization = new()
        {
            OrgNumber = OrgNumber,
            Name = Name,
            UnitType = UnitType,
            EMailAddress = "orstad@accounting.no",
        };

        public static OrstadAccounting Defaults { get; } = new OrstadAccounting();

        public OrstadAccounting(params Action<Party>[] modifiers)
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
