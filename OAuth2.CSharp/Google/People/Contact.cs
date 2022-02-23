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
using Google.Apis.Util;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

namespace OfficeClip.OpenSource.OAuth2.CSharp.Google.People
{
    /// <summary>
    /// Test class with people api
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/54830076/google-people-api-c-sharp-code-to-get-list-of-contact-groups"/>
    public class Contact
    {
        private PeopleServiceService _service;
        private UpdateContactPhotoRequest photoBody;
        private PeopleResource.UpdateContactPhotoRequest updateContactPhotoRequest;
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

            var signatureInfoList = new List<SignatureInfo>();
            foreach (var peopleResponse in peoplesInfo.Responses)
            {
                // CR:2022-02-23 Please check for error and if there is an error, populate the error number and error description
                signatureInfoList.Add(new SignatureInfo()
                {
                    SId = peopleResponse.Person.ResourceName,
                    ETag = peopleResponse.Person.ETag,
                    ErrorNumber = 0,
                    ErrorMessage = ""
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
            for (int i = 0; i<sids.Count; i=i+200)
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

            var contactInfoList = new List<ContactInfo>();
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

        public List<SignatureInfo> Update(List<ContactInfo> contactInfos)
        {
            List<SignatureInfo> signatureInfoList = new List<SignatureInfo>();
            foreach (var contactInfo in contactInfos)
            {
                var contactInfoValue = Update(contactInfo);
                signatureInfoList.Add(new SignatureInfo()
                {
                    ETag = contactInfoValue.ETag
                });
            }
            return signatureInfoList;
        }

        public SignatureInfo Update(ContactInfo contactInfo)
        {
            // get the existing contact, write over it with contactInfo,
            // update the contact and send using the same etag

            var peopleRequest =
                _service.People.Get(contactInfo.ContactSource);
            peopleRequest.PersonFields = "clientData";
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
            emailAddresses.Add(new EmailAddress() { Value = contactInfo.EmailAddress, Type = "Email" });
            emailAddresses.Add(new EmailAddress() { Value = contactInfo.AlternateEmailAddress, Type = "Alternate Email" });
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

            List<Biography> biographies = new List<Biography>();
            biographies.Add(new Biography() { Value = contactInfo.Description });
            contactToUpdate.Biographies = biographies;

            PeopleResource.UpdateContactRequest updateContactRequest = new PeopleResource.UpdateContactRequest(_service, contactToUpdate, contactToUpdate.ResourceName);
            updateContactRequest.UpdatePersonFields = "names,emailAddresses,addresses,birthdays,organizations,phoneNumbers,biographies";
            updateContactRequest.CreateRequest();
            Person updatedContact = updateContactRequest.Execute();

            if (contactInfo.Base64PhotoBytes != null)
            {
                try
                {
                    updateContactPhotoRequest = UpdateImage(updatedContact.ResourceName, contactInfo.Base64PhotoBytes);
                }
                catch (Exception ex)
                {
                    logger?.Error(
                                  "OAuth2.CSharp.Google.People.Contact.Insert",
                                  $"Could not Add Photo: {ex.Message}");
                }
            }
            // CR:2022-02-23 Please check for error and if there is an error, populate the error number and error description

            var signatureInfo = new SignatureInfo()
            {
                SId = updateContactPhotoRequest.ResourceName,
                ETag = updatedContact.ETag
            };
            return signatureInfo;
        }

        public List<SignatureInfo> Insert(string groupSid, List<ContactInfo> contacts)
        {
            List<SignatureInfo> signatureInfoList = new List<SignatureInfo>();
            foreach (var contact in contacts)
            {
                var signatureInfo = Insert(groupSid, contact);
                signatureInfoList.Add(signatureInfo);

            }
            return signatureInfoList;
        }

        public SignatureInfo Insert(string groupSid, ContactInfo contactInfo)
        {
            var contactToCreate = new Person();
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

            if (contactInfo.Base64PhotoBytes != null)
            {
                try
                {
                    updateContactPhotoRequest = UpdateImage(createdContact.ResourceName, contactInfo.Base64PhotoBytes);
                }
                catch (Exception ex)
                {
                    logger?.Error(
                                  "OAuth2.CSharp.Google.People.Contact.Insert",
                                  $"Could not Add Photo: {ex.Message}");
                }
            }
            // CR:2022-02-23 Please check for error and if there is an error, populate the error number and error description
            var signatureInfo = new SignatureInfo()
            {
                SId = updateContactPhotoRequest.ResourceName,
                ETag = createdContact.ETag
            };
            return signatureInfo;
        }

        public PeopleResource.UpdateContactPhotoRequest UpdateImage(string resourceName, string base64ImageBytes)
        {
            var contactToCreate = new Person();

            //var base64Url = GetBase64Image(url);

            List<Photo> photos = new List<Photo>();
            photos.Add(new Photo() { Url = base64ImageBytes });
            contactToCreate.Photos = photos;

            photoBody = new UpdateContactPhotoRequest
            {
                PhotoBytes = base64ImageBytes
            };

            updateContactPhotoRequest = new PeopleResource.UpdateContactPhotoRequest(_service, photoBody, resourceName);
            updateContactPhotoRequest.CreateRequest();
            var updatedContact = updateContactPhotoRequest.Execute();

            return updateContactPhotoRequest;
        }

        byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;

            try
            {
                WebProxy myProxy = new WebProxy();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                stream = response.GetResponseStream();

                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    buf = ms.ToArray();
                }

                stream.Close();
                response.Close();
            }
            catch (Exception exp)
            {
                buf = null;
            }

            return (buf);
        }

        public string GetBase64Image(string imageUrl)
        {
            StringBuilder sb = new StringBuilder();
            var bytes = GetImage(imageUrl);
            sb.Append(Convert.ToBase64String(bytes, 0, bytes.Length));
            return sb.ToString();
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
    }
}
