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
using System.Linq;
using System.Net;

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
        private WebClient webClient;

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
        public List<SignatureInfo> GetSignatures(string contactGroupSid)
        {
            var peopleResourceNames = GetPeopleResourceNames(contactGroupSid);
            var allSignatures = new List<SignatureInfo>();
            for (int i = 0; i<peopleResourceNames.Count; i=i+200)
            {
                var workList = peopleResourceNames.Skip(i).Take(200).ToList();
                var batchList = GetSignature(workList);
                allSignatures.AddRange(batchList);
            }
            return allSignatures;
        }

        private IList<string> GetPeopleResourceNames(string groupResourceName)
        {
            var groupsResource = new ContactGroupsResource(_service);
            var contactGroups = groupsResource.Get(groupResourceName);
            contactGroups.MaxMembers = 25000;
            var peopleResources = contactGroups.Execute();
            var memberResourceNames = peopleResources.MemberResourceNames;
            return memberResourceNames;
        }

        private List<SignatureInfo> GetSignature(List<string> peopleResourceNames)
        {
            var peopleRequest = _service.People.GetBatchGet();
            peopleRequest.ResourceNames = peopleResourceNames;
            peopleRequest.PersonFields = "clientData";
            var peoplesInfo = peopleRequest.Execute();

            List<SignatureInfo> signatureInfoList = new List<SignatureInfo>();
            foreach (var peopleResponse in peoplesInfo.Responses)
            {
                signatureInfoList.Add(new SignatureInfo()
                {
                    SId = peopleResponse.Person.ResourceName,
                    ETag = peopleResponse.Person.ETag
                });
            }
            return signatureInfoList;
        }

        /// <summary>
        /// Return everything that is determined by the schema
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public List<ContactInfo> GetDataByKey(List<string> sids)
        {
            var allContactInfo = new List<ContactInfo>();
            for (int i=0; i<sids.Count; i=i+200)
            {
                var workList = sids.Skip(i).Take(200).ToList();
                var batchList = GetDataByKeyBatch(workList);
                allContactInfo.AddRange(batchList);
            }
            return allContactInfo;
        }

        private List<ContactInfo> GetDataByKeyBatch(List<string> sids)
        {
            var peopleRequest = _service.People.GetBatchGet();
            peopleRequest.ResourceNames = sids;
            peopleRequest.PersonFields = "names,emailAddresses,addresses,birthdays,organizations,phoneNumbers,biographies";
            var peoplesInfo = peopleRequest.Execute();

            List<ContactInfo> contactInfoList = new List<ContactInfo>();
            foreach (var peopleResponse in peoplesInfo.Responses)
            {
                contactInfoList.AddRange(new List<ContactInfo>()
                {
                    GetContactInfo(peopleResponse.Person)
                }); 
            }
            return contactInfoList;
        }

        public bool Delete(List<string> sids)
        {
            BatchDeleteContactsRequest request = new BatchDeleteContactsRequest();
            request.ResourceNames = sids;
            var peopleRequest = _service.People.BatchDeleteContacts(request).Execute();
            return true;
        }

        public List<SignatureInfo> Insert(string groupSid, List<ContactInfo> contacts)
        {
            var contactToCreate = new Person();
            contacts = new List<ContactInfo>();
            List<SignatureInfo> signatureInfoList = new List<SignatureInfo>();
            foreach (var contact in contacts)
            {
                var signatureInfo = Insert(groupSid, contact);
                signatureInfoList.Add(signatureInfo);

            }
            return signatureInfoList;
        }

        private List<ContactInfo> ContactsBatchHardCoded()
        {
            List<ContactInfo> contactInfos = new List<ContactInfo>();
            contactInfos.Add(new ContactInfo()
            {
                FirstName = "Nagesh",
                LastName = "Kulkarni",
                Salutation = "Mr",
                EmailAddress = "nagesh@officeclip.com",
                AlternateEmailAddress = "nageshkulkarni123@gmail.com",
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
            });
            contactInfos.Add(new ContactInfo()
            {
                FirstName = "Sudhakar",
                LastName = "Gundu",
                Salutation = "Mr",
                EmailAddress = "sudhakar@officeclip.com",
                AlternateEmailAddress = "sudhakar@gmail.com",
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
            });
            return contactInfos;
        }

        public SignatureInfo Insert(string groupSid, ContactInfo contact)
        {
            var contactToCreate = new Person();
            List<Name> names = new List<Name>();
            names.Add(new Name()
            {
                HonorificPrefix = contact.Salutation,
                GivenName = contact.FirstName,
                FamilyName = contact.LastName
            });
            contactToCreate.Names = names;

            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(new EmailAddress() { Value = contact.EmailAddress, Type = "Email" });
            emailAddresses.Add(new EmailAddress() { Value = contact.AlternateEmailAddress, Type = "Alternate Email" });
            contactToCreate.EmailAddresses = emailAddresses;

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
            phoneNumbers.Add(new PhoneNumber() { Value = contact.WorkPhone, Type = "mobile" });
            phoneNumbers.Add(new PhoneNumber() { Value = contact.AlternateWorkPhone, Type = "workMobile" });
            contactToCreate.PhoneNumbers = phoneNumbers;

            List<Birthday> birthday = new List<Birthday>();
            birthday.Add(new Birthday() { Text = contact.BirthDate.ToString() });
            contactToCreate.Birthdays = birthday;

            List<Organization> organization = new List<Organization>();
            organization.Add(new Organization()
            {
                Name = contact.CompanyName,
                Department = contact.Department,
                Title = contact.Title
            });
            contactToCreate.Organizations = organization;

            List<Address> addresses = new List<Address>();
            addresses.Add(new Address()
            {
                StreetAddress = contact.Address1,
                ExtendedAddress = contact.Address2,
                City = contact.City,
                Region = contact.State,
                Country = contact.Country,
                PostalCode = contact.Zip,
                Type = "work"
            });
            addresses.Add(new Address()
            {
                StreetAddress = contact.OtherAddress1,
                ExtendedAddress = contact.OtherAddress2,
                City = contact.OtherCity,
                Region = contact.OtherState,
                Country = contact.OtherCountry,
                PostalCode = contact.OtherZip,
                Type = "other"
            });
            contactToCreate.Addresses = addresses;

            List<Biography> biographies = new List<Biography>();
            biographies.Add(new Biography() { Value = contact.Description });
            contactToCreate.Biographies = biographies;

            List<Photo> photos = new List<Photo>();
            photos.Add(new Photo() { Url = "C:/Users/Nagesh Kulkarni/Downloads/image10.png" });
            contactToCreate.Photos = photos;

            Membership membership = new Membership();
            membership.ContactGroupMembership = new ContactGroupMembership()
            {
                ContactGroupResourceName = groupSid
            };
            contactToCreate.Memberships = new List<Membership>();
            contactToCreate.Memberships.Add(membership);
            PeopleResource.CreateContactRequest createContactRequest = new PeopleResource.CreateContactRequest(_service, contactToCreate);
            createContactRequest.CreateRequest();
            Person createdContact = createContactRequest.Execute();
            var signatureInfo = new SignatureInfo()
            {
                SId = createdContact.ResourceName,
                ETag = createdContact.ETag  
            };
            return signatureInfo;       
        }

        public bool Insert(string sid)
        {
            var contactToCreate = new Person();
            var contactInfo = new ContactInfo()
            {
                FirstName = "Nagesh",
                LastName = "Kulkarni",
                Salutation = "Mr",
                EmailAddress = "nagesh@officeclip.com",
                AlternateEmailAddress = "nageshkulkarni123@gmail.com",
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
            };

            List<Name> names = new List<Name>();
            names.Add(new Name()
            {
                HonorificPrefix = contactInfo.Salutation,
                GivenName = contactInfo.FirstName,
                FamilyName = contactInfo.LastName
            });
            contactToCreate.Names = names;

            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(new EmailAddress() { Value = contactInfo.EmailAddress, Type = "Email" });
            emailAddresses.Add(new EmailAddress() { Value = contactInfo.AlternateEmailAddress, Type = "Alternate Email" });
            contactToCreate.EmailAddresses = emailAddresses;

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
            phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.WorkPhone, Type = "mobile" });
            phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.AlternateWorkPhone, Type = "workMobile" });
            contactToCreate.PhoneNumbers = phoneNumbers;

            List<Birthday> birthday = new List<Birthday>();
            birthday.Add(new Birthday() { Text = contactInfo.BirthDate.ToString() });
            contactToCreate.Birthdays = birthday;

            List<Organization> organization = new List<Organization>();
            organization.Add(new Organization()
            {
                Name = contactInfo.CompanyName,
                Department = contactInfo.Department,
                Title = contactInfo.Title
            });
            contactToCreate.Organizations = organization;

            List<Address> addresses = new List<Address>();
            addresses.Add(new Address()
            {
                StreetAddress = contactInfo.Address1,
                ExtendedAddress = contactInfo.Address2,
                City = contactInfo.City,
                Region = contactInfo.State,
                Country = contactInfo.Country,
                PostalCode = contactInfo.Zip,
                Type = "work"
            });
            addresses.Add(new Address()
            {
                StreetAddress = contactInfo.OtherAddress1,
                ExtendedAddress = contactInfo.OtherAddress2,
                City = contactInfo.OtherCity,
                Region = contactInfo.OtherState,
                Country = contactInfo.OtherCountry,
                PostalCode = contactInfo.OtherZip,
                Type = "other"
            });
            contactToCreate.Addresses = addresses;

            List<Biography> biographies = new List<Biography>();
            biographies.Add(new Biography() { Value = contactInfo.Description });
            contactToCreate.Biographies = biographies;

            List<Photo> photos = new List<Photo>();
            photos.Add(new Photo() { Url = "C:/Users/Nagesh Kulkarni/Downloads/image10.png" });
            contactToCreate.Photos = photos;

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
            return true;
        }

        public bool Update(ContactInfo contactInfo)
        {
            var peopleRequest =
                    _service.People.Get(contactInfo.SId);
            peopleRequest.PersonFields = "names,emailAddresses,addresses,birthdays,organizations,phoneNumbers,biographies";
            var contactToUpdate = peopleRequest.Execute();

            List<Name> names = new List<Name>();
            names.Add(new Name()
            {
                HonorificPrefix = contactInfo.Salutation,
                GivenName = contactInfo.FirstName,
                FamilyName = contactInfo.LastName
            });
            contactToUpdate.Names = names;

            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(new EmailAddress() { Value = contactInfo.EmailAddress, DisplayName = "Email" });
            emailAddresses.Add(new EmailAddress() { Value = contactInfo.AlternateEmailAddress, DisplayName = "Alternate Email" });
            contactToUpdate.EmailAddresses = emailAddresses;

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
            phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.WorkPhone, Type = "mobile" });
            phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.AlternateWorkPhone, Type = "workMobile" });
            contactToUpdate.PhoneNumbers = phoneNumbers;

            List<Birthday> birthday = new List<Birthday>();
            birthday.Add(new Birthday() { Text = contactInfo.BirthDate.ToString() });
            contactToUpdate.Birthdays = birthday;

            List<Organization> organization = new List<Organization>();
            organization.Add(new Organization()
            {
                Name = contactInfo.CompanyName,
                Department = contactInfo.Department,
                Title = contactInfo.Title
            });
            contactToUpdate.Organizations = organization;

            List<Address> addresses = new List<Address>();
            addresses.Add(new Address()
            {
                StreetAddress = contactInfo.Address1,
                ExtendedAddress = contactInfo.Address2,
                City = contactInfo.City,
                Region = contactInfo.State,
                Country = contactInfo.Country,
                PostalCode = contactInfo.Zip,
                Type = "work"
            });
            addresses.Add(new Address()
            {
                StreetAddress = contactInfo.OtherAddress1,
                ExtendedAddress = contactInfo.OtherAddress2,
                City = contactInfo.OtherCity,
                Region = contactInfo.OtherState,
                Country = contactInfo.OtherCountry,
                PostalCode = contactInfo.OtherZip,
                Type = "other"
            });
            contactToUpdate.Addresses = addresses;

            contactToUpdate.Biographies.Add(new Biography() { Value = contactInfo.Description });

            PeopleResource.UpdateContactRequest updateContactRequest =
                new PeopleResource.UpdateContactRequest(_service, contactToUpdate, contactToUpdate.ResourceName);
            updateContactRequest.UpdatePersonFields = "names,emailAddresses,addresses,birthdays,organizations,phoneNumbers,biographies";
            Person updatedContact = updateContactRequest.Execute();
            return true;
        }

        public ContactInfo GetContact(string sid)
        {
            var peopleRequest =
                    _service.People.Get(sid);
            peopleRequest.PersonFields = "names,emailAddresses,addresses,birthdays,organizations,phoneNumbers,biographies";
            var people = peopleRequest.Execute();
            var contactInfo = GetContactInfo(people);
            return contactInfo;
        }

        private ContactInfo GetContactInfo(Person people)
        {
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
            if (people.EmailAddresses != null)
            {
                if (people.EmailAddresses.Count > 0)
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
