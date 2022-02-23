using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeClip.OpenSource.OAuth2.CSharp.Google.People;
using OfficeClip.OpenSource.OAuth2.Lib;
using System;
using System.Collections.Generic;
using OfficeClipGoogle = OfficeClip.OpenSource.OAuth2.Lib.Provider.Google;

namespace OAuth2DriverTest
{
    [TestClass]
    public class ContactTest
    {

        [TestMethod]
        public void TestMethod1()
        {
            //var element = Utils.LoadConfigurationFromWebConfig("Google");
            //var client = new OfficeClipGoogle(
            //                    element.ClientId,
            //                    element.ClientSecret,
            //                    element.Scope,
            //                    element.RedirectUri);

            IEnumerable<string> scopes = new string[]{ "https://www.googleapis.com/auth/contacts" };

            var peopleContact = new Contact(
                                            "API key 1",
                                            "AIzaSyCyooBI2Hn7HpDFHRxHpBm757Jq57hbMT4",
                                            scopes,
                                            "https://people.googleapis.com/v1/{person.resourceName=people/*}:updateContact",
                                            string.Empty
                                            );

            List<ContactInfo> contactInfos = new List<ContactInfo>();
            contactInfos.Add(new ContactInfo()
            {
                FirstName = "S.K",
                LastName = "Dutta",
                Salutation = "Mr",
                EmailAddress = "skd@officeclip.com",
                AlternateEmailAddress = "skdutta@gmail.com",
                WorkPhone = "9966321831",
                AlternateWorkPhone = "8125505421",
                BirthDate = DateTime.Parse("1-8-1988"),
                CompanyName = "OfficeClip",
                Department = "Development",
                Title = "Software Engineer",
                Address1 = "H.No: MIG - II - 131",
                Address2 = "Housing Board Colony",
                City = "MahaboobNagar",
                State = "Telangana",
                Country = "India",
                Zip = "509001",
                OtherAddress1 = "H.No 8-10-48",
                OtherAddress2 = "Inside Gadi, Old Tandur",
                OtherCity = "Tandur",
                OtherState = "Telangana",
                OtherCountry = "India",
                OtherZip = "501141",
                Description = "This is to test create contact in Google Contact.",
                ContactSource = "people/c2655525945195541981",
                Base64PhotoBytes = peopleContact.GetBase64Image("https://picsum.photos/200/200?grayscale")
            });
            contactInfos.Add(new ContactInfo()
            {
                FirstName = "Narsimha",
                LastName = "Rao",
                Salutation = "Mr",
                EmailAddress = "narsimha@officeclip.com",
                AlternateEmailAddress = "narsimha@gmail.com",
                WorkPhone = "1234567890",
                AlternateWorkPhone = "0987654321",
                BirthDate = DateTime.Parse("1-8-1988"),
                CompanyName = "OfficeClip",
                Department = "Development",
                Title = "Software Engineer",
                Address1 = "H.No: MIG - II - 131",
                Address2 = "Housing Board Colony",
                City = "MahaboobNagar",
                State = "Telangana",
                Country = "India",
                Zip = "509001",
                OtherAddress1 = "H.No 8-10-48",
                OtherAddress2 = "Inside Gadi, Old Tandur",
                OtherCity = "Tandur",
                OtherState = "Telangana",
                OtherCountry = "India",
                OtherZip = "501141",
                Description = "This is to test create contact in Google Contact.",
                ContactSource = "people/c9202708945039554846",
                Base64PhotoBytes = null
            });
            //Insert("contactGroups/7ca636f18d719084", contactInfos);
            peopleContact.Update(contactInfos);
        }
    }
}
