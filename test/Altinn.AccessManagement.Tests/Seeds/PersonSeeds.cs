using System;
using Altinn.AccessManagement.Tests.Util;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Enums;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Seeds;

public static class PersonSeeds
{
    public abstract class PersonBase : UserProfile, IParty, IUserProfile, IToken
    {
        public UserProfile UserProfile => this;

        public string Token => PrincipalUtil.GetToken(UserId, PartyId, 2);
    }

    public class Paula : PersonBase
    {
        public new static int UserId = 20000001;

        public new static Guid? UserUuid = new Guid("511e5189-175b-402b-adc7-1b7185e37dd2");

        public new static int PartyId = 50000001;

        public new static Party Party = new()
        {
            PartyTypeName = PartyType.Person,
            SSN = "02056260016",
            PartyId = PartyId,
            Name = "PAULA RIMSTAD",
            IsDeleted = false,
            OnlyHierarchyElementWithNoAccess = false,
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

        public new static ProfileSettingPreference ProfileSettingPreference = new()
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

    public class Orjan : PersonBase
    {
        public new static int UserId = 20000002;

        public new static Guid? UserUuid = new Guid("375f90cf-a184-4360-81db-1b6e7f439edc");

        public new static int PartyId = 50000002;

        public new static Party Party = new()
        {
            PartyTypeName = PartyType.Person,
            SSN = "27099450067",
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

        public new static ProfileSettingPreference ProfileSettingPreference = new()
        {
            Language = "no",
            PreSelectedPartyId = 0,
            DoNotPromptForParty = false
        };

        public static Orjan Defaults { get; } = new Orjan();

        public Orjan(params Action<UserProfile>[] modifiers)
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
        public new static int UserId = 20000003;

        public new static Guid? UserUuid = new Guid("9144053e-3909-4c11-829d-4521eb543952");

        public new static int PartyId = 50000003;

        public new static Party Party = new()
        {
            PartyTypeName = PartyType.Person,
            SSN = "07124912037",
            PartyId = PartyId,
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

        public new static ProfileSettingPreference ProfileSettingPreference = new()
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