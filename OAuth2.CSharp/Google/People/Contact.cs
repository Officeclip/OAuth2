using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using System.Data;
using System.IO;
using NLog;

namespace OfficeClip.OpenSource.OAuth2.CSharp.Google.People
{
    /// <summary>
    /// Test class with people api
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/54830076/google-people-api-c-sharp-code-to-get-list-of-contact-groups"/>
    public class Contact
    {
        private PeopleServiceService _service;
        private static Logger logger =
                                    LogManager.GetCurrentClassLogger();

        public Contact(
            string clientId,
            string clientSecret,
            IEnumerable<string> scopes,
            string accessToken,
            string userEmail)
        {
            logger.Debug("Contact Constructor");
            try
            {
                var flow = new GoogleAuthorizationCodeFlow(
                                        new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    Scopes = scopes
                });

                var token = new TokenResponse
                {
                    AccessToken = accessToken,
                    ExpiresInSeconds = 3000,
                    IssuedUtc = DateTime.UtcNow,
                    RefreshToken = null
                };

                var credential = new UserCredential(flow, "me", token);

                _service = new PeopleServiceService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Test Application",
                });

            }
            catch (Exception ex)
            {                
                throw new Exception("Contact constructor failed", ex);
            }
        }

        public List<ContactGroupInfo> ContactGroups
        {
            get
            {
                var groupsResource = new ContactGroupsResource(_service);
                var listRequest = groupsResource.List();
                var response = listRequest.Execute();

                var contactGroupInfos = new List<ContactGroupInfo>();
                foreach (var group in response.ContactGroups)
                {
                    var contactGroupInfo = new ContactGroupInfo()
                    {
                        Name = group.FormattedName,
                        SId = group.ResourceName,
                        ETag = group.ETag,
                        NumberOfContacts = group.MemberCount
                    };
                    contactGroupInfos.Add(contactGroupInfo);
                }
                return contactGroupInfos;
            }
        }

        /// <summary>
        /// Get all the Google Contacts Sid and etag
        /// </summary>
        /// <remarks> Check the contact driver file on how to implemetn this</remarks>
        /// <returns></returns>
        public List<SignatureInfo> GetSignature(string contactGroupSid)
        {
            //mLogger?.WriteMethod(serviceType, dsSchema);
            //try
            //{
            //    ContactsRequest contactRequest = new ContactsRequest(requestSetting);
            //    GetAllContacts(contactRequest);
            //    foreach (Contact contactItem in contactFeed.Entries)
            //    {
            //        DataRow dr = dsSchema.Tables[0].NewRow();
            //        FillReturnDataRow(dr, contactItem);
            //        dsSchema.Tables[0].Rows.Add(dr);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    mLogger?.WriteError(
            //        "Driver.Google.ContactDriver.GetSignature",
            //        $"Error: {ex.Message}");
            //}

            return null ;
        }

        /// <summary>
        /// Return everything that is determined by the schema
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public List<ContactInfo> GetDataByKey(string sids)
        {
            return null;
        }

        public bool Delete(List<string> sids)
        {
            return false;
        }

        public bool Insert(string sid)
        {
            List<Name> names = new List<Name>();
            names.Add(new Name() { HonorificPrefix = "Testing", GivenName = "Testing", FamilyName = "Testing" });
            var contactToCreate = new Person();
            contactToCreate.Names = names;
            Membership membership = new Membership();
            membership.ContactGroupMembership = new ContactGroupMembership()
            {
                ContactGroupResourceName = sid
            };
            contactToCreate.Memberships = new List<Membership>();
            contactToCreate.Memberships.Add(membership);
            PeopleResource.CreateContactRequest createContactRequest = new PeopleResource.CreateContactRequest(_service, contactToCreate);
            createContactRequest.CreateRequest();
            Person createdContact = createContactRequest.Execute();
            return false;
        }

        public bool Update(ContactInfo contactInfo)
        {
            var peopleRequest = 
                    _service.People.Get(contactInfo.SId);
            peopleRequest.PersonFields = "names";
            var contactToUpdate = peopleRequest.Execute();

            List<Name> names = new List<Name>();
            names.Add(new Name() { HonorificPrefix = "Mr.", GivenName = "PeopleAPI", FamilyName = "Testing" });
            contactToUpdate.Names = names;

            PeopleResource.UpdateContactRequest updateContactRequest =
                new PeopleResource.UpdateContactRequest(_service, contactToUpdate, contactToUpdate.ResourceName);
            updateContactRequest.UpdatePersonFields = "names";
            Person updatedContact = updateContactRequest.Execute();
            return true;
        }

        public ContactInfo GetContact(string sid)
        {
            var peopleRequest =
                    _service.People.Get(sid);
            peopleRequest.PersonFields = "names,emailAddresses,addresses,birthdays,organizations,phoneNumbers";
            var people = peopleRequest.Execute();
            var contactInfo = new ContactInfo()
            {
                SId = people.ResourceName,
                ETag = people.ETag
            };
            if (
                (people.Names != null) && 
                (people.Names.Count > 0))
            {
                contactInfo.FirstName = people.Names[0].GivenName;
                contactInfo.LastName = people.Names[0].FamilyName;
                contactInfo.Salutation = people.Names[0].HonorificPrefix;
            }
            if(people.EmailAddresses != null)
            {
                if(people.EmailAddresses.Count > 0)
                {
                    contactInfo.EmailAddress = people.EmailAddresses[0].Value;
                }
                if (people.EmailAddresses.Count > 1)
                {
                    contactInfo.AlternateEmailAddress = people.EmailAddresses[1].Value;
                }
            }

            if (people.PhoneNumbers != null)
            {
                if (people.PhoneNumbers.Count > 0)
                {
                    contactInfo.WorkPhone = people.PhoneNumbers[0].Value;
                }
                if (people.PhoneNumbers.Count > 1)
                {
                    contactInfo.AlternateWorkPhone = people.PhoneNumbers[1].Value;
                }
            }

            if (
                (people.Birthdays != null) &&
                (people.Birthdays.Count > 0))
            {
                contactInfo.BirthDate = DateTime.Parse(people.Birthdays[0].Text);
            }

            if (
                (people.Organizations != null) &&
                (people.Organizations.Count > 0))
            {
                contactInfo.CompanyName = people.Organizations[0].Name;
                contactInfo.Department = people.Organizations[0].Department;
                contactInfo.Title = people.Organizations[0].Title;
            }

            if (people.Addresses != null)
            {
                if (people.Addresses.Count > 0)
                {
                    contactInfo.Address1 = people.Addresses[0].StreetAddress;
                    contactInfo.Address2 = people.Addresses[0].ExtendedAddress;
                    contactInfo.City = people.Addresses[0].City;
                    contactInfo.State = people.Addresses[0].Region;
                    contactInfo.Country = people.Addresses[0].Country;
                    contactInfo.Zip = people.Addresses[0].PostalCode;
                }
                if (people.Addresses.Count > 1)
                {
                    contactInfo.OtherAddress1 = people.Addresses[1].StreetAddress;
                    contactInfo.OtherAddress2 = people.Addresses[1].ExtendedAddress;
                    contactInfo.OtherCity = people.Addresses[1].City;
                    contactInfo.OtherState = people.Addresses[1].Region;
                    contactInfo.OtherCountry = people.Addresses[1].Country;
                    contactInfo.OtherZip = people.Addresses[1].PostalCode;
                }
            }

            if (
                (people.Biographies != null) &&
                (people.Biographies.Count > 0))
            {
                contactInfo.Description = people.Biographies[0].Value;
            }

            return contactInfo;
        }

        public List<string> ContactList
        {
            get
            {
                //    var peopleRequest =
                //        _service.People.Connections.List("people/me");
                //    peopleRequest.PersonFields = "names,emailAddresses";
                //    peopleRequest.SortOrder = (PeopleResource.ConnectionsResource.ListRequest.SortOrderEnum)1;
                //    var people = peopleRequest.Execute();

                //    // eg to show display name of each contact
                //    var contacts = new List<string>();
                //    foreach (var person in people.Connections)
                //    {
                //        contacts.Add(person.Names[0].DisplayName);
                //    }
                //    return contacts;
                return null;
            }
        }

        //public void CreateContact(string contactGroupResource)
        //{
        //    Person contactToCreate = new Person();

        //    List<Name> names = new List<Name>();
        //    names.Add(new Name() { HonorificPrefix = "Mr.",  GivenName = "Nagesh", FamilyName = "Kulkarni" });
        //    contactToCreate.Names = names;

        //    List<EmailAddress> emailAddresses = new List<EmailAddress>();
        //    emailAddresses.Add(new EmailAddress() { Value = "john@officeclip.com" });
        //    contactToCreate.EmailAddresses = emailAddresses;

        //    List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
        //    phoneNumbers.Add(new PhoneNumber() { Value = "1234567890" });
        //    contactToCreate.PhoneNumbers = phoneNumbers;

        //    List<Birthday> birthday = new List<Birthday>();
        //    birthday.Add(new Birthday() { Text = "1/8/1988" });
        //    contactToCreate.Birthdays = birthday;

        //    List<Organization> organization = new List<Organization>();
        //    organization.Add(new Organization() { Name = "OfficeClip", Department = "Development", Title = "Software Engineer"});
        //    contactToCreate.Organizations = organization;

        //    List<Address> addresses = new List<Address>();
        //    addresses.Add(new Address() { StreetAddress="MIG-II-131", ExtendedAddress="Housing Board Colony", City="", Region="", Country="", PostalCode="" });
        //    contactToCreate.Addresses = addresses;

        //    var contactGroupMembership = new ContactGroupMembership()
        //    {
        //        ContactGroupResourceName = contactGroupResource
        //    };

        //    contactToCreate.Memberships = new List<Membership>();
        //    var memberShip = new Membership()
        //    {
        //        ContactGroupMembership = contactGroupMembership,

        //    };
        //    contactToCreate.Memberships.Add(memberShip);

        //    PeopleResource.CreateContactRequest request = 
        //        new PeopleResource.CreateContactRequest(_service, contactToCreate);

        //    Person createdContact = request.Execute();
        //}

        //public void UpdateContact(string resource)
        //{
        //    var contactService = _service
        //                                .People
        //                                .Get(resource);
        //    contactService.PersonFields = "names";
        //    var contactToUpdate = contactService.Execute();

        //    List<Name> names = new List<Name>();
        //    names.Add(new Name() { HonorificPrefix = "Mr.", GivenName = "Martin", FamilyName = "ABCD" });
        //    contactToUpdate.Names = names;

        //    //List<EmailAddress> emailAddresses = new List<EmailAddress>();
        //    //emailAddresses.Add(new EmailAddress() { Value = "nagesh@officeclip.com" });
        //    //contactToUpdate.EmailAddresses = emailAddresses;

        //    //List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
        //    //phoneNumbers.Add(new PhoneNumber() { Value = "9876543210" });
        //    //contactToUpdate.PhoneNumbers = phoneNumbers;

        //    PeopleResource.UpdateContactRequest updateContactRequest =
        //        new PeopleResource.UpdateContactRequest(_service, contactToUpdate, contactToUpdate.ResourceName);
        //    updateContactRequest.UpdatePersonFields = "names";
        //    //updateContactRequest.PersonFields = "emailAddresses";
        //    //updateContactRequest.Fields = "emailAddresses";
        //    Person updatedContact = updateContactRequest.Execute();
        //}
    }
}
