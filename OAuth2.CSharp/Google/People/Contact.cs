﻿using Google.Apis.Auth.OAuth2;
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
                    ErrorNumber = (int)peopleResponse.Status.Code,
                    ErrorMessage = peopleResponse.Status.Message
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
            peopleRequest.PersonFields = "names,emailAddresses,addresses,birthdays,organizations,phoneNumbers,biographies,urls,userDefined";
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

            SignatureInfo signatureInfo;

            try
            {
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
                emailAddresses.Add(new EmailAddress() { Value = contactInfo.EmailAddress, Type = "work" });
                emailAddresses.Add(new EmailAddress() { Value = contactInfo.AlternateEmailAddress, Type = "other" });
                contactToUpdate.EmailAddresses = emailAddresses;

                List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.WorkPhone, Type = "work" });
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.AlternateWorkPhone, Type = "other" });
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.HomePhone, Type = "home" });
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.MobilePhone, Type = "mobile" });
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.Fax, Type = "workFax" });
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

                List<Url> websites = new List<Url>();
                websites.Add(new Url() { Value = contactInfo.Website, Type = "homePage" });
                contactToUpdate.Urls = websites;

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
                    Type = "home"
                });
                contactToUpdate.Addresses = addresses;

                List<UserDefined> userDefineds = new List<UserDefined>();
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf1, Key = "Udf1" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf2, Key = "Udf2" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf3, Key = "Udf3" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf4, Key = "Udf4" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf5, Key = "Udf5" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf6, Key = "Udf6" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf7, Key = "Udf7" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf8, Key = "Udf8" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf9, Key = "Udf9" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf10, Key = "Udf10" });
                contactToUpdate.UserDefined = userDefineds;

                List<Biography> biographies = new List<Biography>();
                biographies.Add(new Biography() { Value = contactInfo.Description });
                contactToUpdate.Biographies = biographies;

                PeopleResource.UpdateContactRequest updateContactRequest = new PeopleResource.UpdateContactRequest(_service, contactToUpdate, contactToUpdate.ResourceName);
                updateContactRequest.UpdatePersonFields = "names,emailAddresses,addresses,birthdays,organizations,phoneNumbers,biographies,urls,userDefined";
                updateContactRequest.CreateRequest();
                var updatedContact = updateContactRequest.Execute();

                if (contactInfo.Base64PhotoBytes != null)
                {
                    try
                    {
                        updateContactPhotoRequest = UpdateImage(updatedContact.ResourceName, contactInfo.Base64PhotoBytes);
                    }
                    catch (Exception ex)
                    {
                        logger?.Error(
                                      "OAuth2.CSharp.Google.People.Contact.UpdateImage",
                                      $"Could not Add Photo: {ex.Message}");
                    }
                }
                signatureInfo = new SignatureInfo()
                {
                    SId = updatedContact.ResourceName,
                    ETag = updatedContact.ETag
                };

                // CR:2022-02-23 Please check for error and if there is an error, populate the error number and error description
            }
            catch (Exception ex)
            {
                logger?.Error(
                              "OAuth2.CSharp.Google.People.Contact.Update",
                              $"Could not update contact: {ex.Message}");
                signatureInfo = new SignatureInfo()
                {
                    SId = contactInfo.SId,
                    ETag = contactInfo.ETag,
                    ErrorNumber = -1,
                    ErrorMessage = ex.Message

                };

            }
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
            SignatureInfo signatureInfo;

            try
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
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.WorkPhone, Type = "work" });
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.AlternateWorkPhone, Type = "other" });
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.HomePhone, Type = "home" });
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.MobilePhone, Type = "mobile" });
                phoneNumbers.Add(new PhoneNumber() { Value = contactInfo.Fax, Type = "workFax" });
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

                List<Url> websites = new List<Url>();
                websites.Add(new Url() { Value = contactInfo.Website, Type = "Website" });
                contactToCreate.Urls = websites;

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
                    Type = "home"
                });
                contactToCreate.Addresses = addresses;

                List<Biography> biographies = new List<Biography>();
                biographies.Add(new Biography() { Value = contactInfo.Description });
                contactToCreate.Biographies = biographies;

                List<UserDefined> userDefineds = new List<UserDefined>();
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf1, Key = "Udf1" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf2, Key = "Udf2" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf3, Key = "Udf3" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf4, Key = "Udf4" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf5, Key = "Udf5" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf6, Key = "Udf6" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf7, Key = "Udf7" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf8, Key = "Udf8" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf9, Key = "Udf9" });
                userDefineds.Add(new UserDefined() { Value = contactInfo.Udf10, Key = "Udf10" });
                contactToCreate.UserDefined = userDefineds;

                Membership membership = new Membership();
                membership.ContactGroupMembership = new ContactGroupMembership()
                {
                    ContactGroupResourceName = groupSid
                };
                contactToCreate.Memberships = new List<Membership>();
                contactToCreate.Memberships.Add(membership);
                PeopleResource.CreateContactRequest createContactRequest = new PeopleResource.CreateContactRequest(_service, contactToCreate);
                createContactRequest.CreateRequest();
                var createdContact = createContactRequest.Execute();

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

                signatureInfo = new SignatureInfo()
                {
                    SId = createdContact.ResourceName,
                    ETag = createdContact.ETag
                };
            }
            catch (Exception ex)
            {
                logger?.Error(
                              "OAuth2.CSharp.Google.People.Contact.Update",
                              $"Could not update contact: {ex.Message}");
                signatureInfo = new SignatureInfo()
                {
                    ErrorNumber = -1,
                    ErrorMessage = ex.Message

                };

            }
            // CR:2022-02-23 Please check for error and if there is an error, populate the error number and error description
            return signatureInfo;
        }

        public PeopleResource.UpdateContactPhotoRequest UpdateImage(string resourceName, string base64ImageBytes)
        {
            var contactToCreate = new Person();

            var base64Url = GetBase64Image(base64ImageBytes);

            List<Photo> photos = new List<Photo>();
            photos.Add(new Photo() { Url = base64Url });
            contactToCreate.Photos = photos;

            photoBody = new UpdateContactPhotoRequest
            {
                PhotoBytes = base64Url
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
            peopleRequest.PersonFields = "names,emailAddresses,addresses,birthdays,organizations,phoneNumbers,biographies,urls,userDefined";
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
                if (people.PhoneNumbers.Count > 2)
                {
                    contactInfo.HomePhone = people.PhoneNumbers[2].Value;
                }
                if (people.PhoneNumbers.Count > 3)
                {
                    contactInfo.MobilePhone = people.PhoneNumbers[3].Value;
                }
                if (people.PhoneNumbers.Count > 4)
                {
                    contactInfo.Fax = people.PhoneNumbers[4].Value;
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

            if (
                (people.Urls != null) &&
                (people.Urls.Count > 0))
            {
                contactInfo.Website = people.Urls[0].Value;
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
                if (people.Addresses.Count > 0)
                {
                    contactInfo.OtherAddress1 = people.Addresses[1].StreetAddress;
                    contactInfo.OtherAddress2 = people.Addresses[1].ExtendedAddress;
                    contactInfo.OtherCity = people.Addresses[1].City;
                    contactInfo.OtherState = people.Addresses[1].Region;
                    contactInfo.OtherCountry = people.Addresses[1].Country;
                    contactInfo.OtherZip = people.Addresses[1].PostalCode;
                }
            }

            if (people.UserDefined != null)
            {
                //CR:CR:2022-02-24 Use a linq expression to get the key and value and assign to contact info.
                if (people.UserDefined.Count > 0)
                {
                    contactInfo.Udf1 = people.UserDefined[0].Value;
                }
                if (people.UserDefined.Count > 1)
                {
                    contactInfo.Udf2 = people.UserDefined[1].Value;
                }
                if (people.UserDefined.Count > 2)
                {
                    contactInfo.Udf3 = people.UserDefined[2].Value;
                }
                if (people.UserDefined.Count > 3)
                {
                    contactInfo.Udf4 = people.UserDefined[3].Value;
                }
                if (people.UserDefined.Count > 4)
                {
                    contactInfo.Udf5 = people.UserDefined[4].Value;
                }
                if (people.UserDefined.Count > 5)
                {
                    contactInfo.Udf6 = people.UserDefined[5].Value;
                }
                if (people.UserDefined.Count > 6)
                {
                    contactInfo.Udf7 = people.UserDefined[6].Value;
                }
                if (people.UserDefined.Count > 7)
                {
                    contactInfo.Udf8 = people.UserDefined[7].Value;
                }
                if (people.UserDefined.Count > 8)
                {
                    contactInfo.Udf9 = people.UserDefined[8].Value;
                }
                if (people.UserDefined.Count > 9)
                {
                    contactInfo.Udf10 = people.UserDefined[9].Value;
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
