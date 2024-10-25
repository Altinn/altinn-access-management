using System;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Seeds;

public static class PersonSeeds
{
    public abstract class PersonBase : UserProfile, IParty, IUserProfile
    {
        public UserProfile UserProfile => this;
    }

    public class Paula : PersonBase
    {
        public new static readonly int UserId = 1000;

        public new static readonly Guid? UserUuid = new Guid("00000000-0000-0000-0000-000000000100");

        public new static readonly int PartyId = 100;

        public new static readonly Party Party = new()
        {
            PartyTypeName = PartyType.Person,
            SSN = "02056260016",
            PartyId = PartyId,
            Name = "PAULA RIMSTAD",
            IsDeleted = false,
            OnlyHierarchyElementWithNoAccess = false,
            PartyUuid = UserUuid,
            Person = new()
            {
                SSN = "02056260016",
                Name = "PAULA RIMSTAD",
                FirstName = "PAULA",
                LastName = "RIMSTAD",
                MailingAddress = "Brannpostveien 15 3014 DRAMMEN",
                MailingPostalCode = "3014",
                MailingPostalCity = "DRAMMEN",
                AddressPostalCode = "3014",
                AddressCity = "DRAMMEN",
            }
        };

        public new static readonly ProfileSettingPreference ProfileSettingPreference = new()
        {
            Language = "no",
            PreSelectedPartyId = 0,
            DoNotPromptForParty = false
        };

        public static Paula Defaults { get; } = new Paula();

        public Paula(params Action<UserProfile>[] modifiers)
        {
            base.UserId = UserId;
            base.UserUuid = UserUuid;
            base.PartyId = PartyId;
            base.Party = Party;
            base.ProfileSettingPreference = ProfileSettingPreference;

            foreach (var modifer in modifiers)
            {
                modifer(this);
            }
        }
    }

    public class Olav : PersonBase
    {
        public new static readonly int UserId = 1001;

        public new static readonly Guid? UserUuid = new Guid("00000000-0000-0000-0000-000000000101");

        public new static readonly int PartyId = 101;

        public new static readonly Party Party = new()
        {
            PartyTypeName = PartyType.Person,
            SSN = "27099450067",
            PartyUuid = UserUuid,
            PartyId = PartyId,
            Name = "ØRJAN RAVNÅS",
            IsDeleted = false,
            OnlyHierarchyElementWithNoAccess = false,
            Person = new()
            {
                SSN = "27099450067",
                Name = "ØRJAN RAVNÅS",
                FirstName = "ØRJAN",
                LastName = "RAVNÅS",
                MailingAddress = "Bjerkeveien 23 1726 SARPSBORG",
                MailingPostalCode = "1726",
                MailingPostalCity = "SARPSBORG",
                AddressPostalCode = "1726",
                AddressCity = "SARPSBORG",
            }
        };

        public new static readonly ProfileSettingPreference ProfileSettingPreference = new()
        {
            Language = "no",
            PreSelectedPartyId = 0,
            DoNotPromptForParty = false
        };

        public static Olav Defaults { get; } = new Olav();

        public Olav(params Action<UserProfile>[] modifiers)
        {
            base.UserId = UserId;
            base.UserUuid = UserUuid;
            base.PartyId = PartyId;
            base.Party = Party;
            base.ProfileSettingPreference = ProfileSettingPreference;

            foreach (var modifer in modifiers)
            {
                modifer(this);
            }
        }
    }

    public class Kasper : PersonBase
    {
        public new static readonly int UserId = 1002;

        public new static readonly Guid? UserUuid = new Guid("00000000-0000-0000-0000-000000000102");

        public new static readonly int PartyId = 102;

        public new static readonly Party Party = new()
        {
            PartyTypeName = PartyType.Person,
            SSN = "07124912037",
            PartyId = PartyId,
            PartyUuid = UserUuid,
            Name = "KASPER BØRSTAD",
            IsDeleted = false,
            OnlyHierarchyElementWithNoAccess = false,
            Person = new()
            {
                SSN = "07124912037",
                Name = "KASPER BØRSTAD",
                FirstName = "KASPER",
                LastName = "BØRSTAD",
                MailingAddress = "Skålevikstølen 7 5178 LODDEFJORD",
                MailingPostalCode = "5178",
                MailingPostalCity = "LODDEFJORD",
                AddressPostalCode = "5178",
                AddressCity = "LODDEFJORD",
            }
        };

        public new static readonly ProfileSettingPreference ProfileSettingPreference = new()
        {
            Language = "no",
            PreSelectedPartyId = 0,
            DoNotPromptForParty = false
        };

        public static Kasper Defaults { get; } = new Kasper();

        public Kasper(params Action<UserProfile>[] modifiers)
        {
            base.UserId = UserId;
            base.UserUuid = UserUuid;
            base.PartyId = PartyId;
            base.Party = Party;
            base.ProfileSettingPreference = ProfileSettingPreference;

            foreach (var modifer in modifiers)
            {
                modifer(this);
            }
        }
    }
}