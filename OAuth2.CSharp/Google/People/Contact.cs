using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;

namespace OfficeClip.OpenSource.OAuth2.CSharp.Google.People
{
    /// <summary>
    /// Test class with people api
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/54830076/google-people-api-c-sharp-code-to-get-list-of-contact-groups"/>
    public class Contact
    {
        private PeopleServiceService _service;
        public Contact(
            string clientId,
            string clientSecret,
            IEnumerable<string> scopes,
            string accessToken,
            string userEmail)
        {
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

        public List<string> GroupNames
        {
            get
            {
                var groupsResource = new ContactGroupsResource(_service);
                var listRequest = groupsResource.List();
                var response = listRequest.Execute();

                // eg to show name of each group
                var groupNames = new List<string>();
                foreach (var group in response.ContactGroups)
                {
                    groupNames.Add(group.FormattedName);
                }
                return groupNames;
            }
        }

        public List<string> ContactList
        {
            get
            {
                var peopleRequest =
                    _service.People.Connections.List("people/me");
                peopleRequest.PersonFields = "names,emailAddresses";
                peopleRequest.SortOrder = (PeopleResource.ConnectionsResource.ListRequest.SortOrderEnum)1;
                var people = peopleRequest.Execute();

                // eg to show display name of each contact
                var contacts = new List<string>();
                foreach (var person in people.Connections)
                {
                    contacts.Add(person.Names[0].DisplayName);
                }
                return contacts;
            }
        }


        public void CreateContact()
        {
            Person contactToCreate = new Person();

            List<Name> names = new List<Name>();
            names.Add(new Name() { HonorificPrefix = "Mr.",  GivenName = "Nagesh", FamilyName = "Kulkarni" });
            contactToCreate.Names = names;

            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(new EmailAddress() { Value = "john@officeclip.com" });
            contactToCreate.EmailAddresses = emailAddresses;

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
            phoneNumbers.Add(new PhoneNumber() { Value = "1234567890" });
            contactToCreate.PhoneNumbers = phoneNumbers;

            List<Birthday> birthday = new List<Birthday>();
            birthday.Add(new Birthday() { Text = "1/8/1988" });
            contactToCreate.Birthdays = birthday;

            List<Organization> organization = new List<Organization>();
            organization.Add(new Organization() { Name = "OfficeClip", Department = "Development", Title = "Software Engineer"});
            contactToCreate.Organizations = organization;

            List<Address> addresses = new List<Address>();
            addresses.Add(new Address() { StreetAddress="MIG-II-131", ExtendedAddress="Housing Board Colony", City="", Region="", Country="", PostalCode="" });
            contactToCreate.Addresses = addresses;
            
            var contactGroupMembership = new ContactGroupMembership()
            {
                ContactGroupResourceName = "contactGroups/7ca636f18d719084"
            };

            contactToCreate.Memberships = new List<Membership>();
            var memberShip = new Membership()
            {
                ContactGroupMembership = contactGroupMembership,
                
            };
            contactToCreate.Memberships.Add(memberShip);

            PeopleResource.CreateContactRequest request = 
                new PeopleResource.CreateContactRequest(_service, contactToCreate);

            Person createdContact = request.Execute();
        }

        public void UpdateContact()
        {
            Person contactToUpdate = _service.People.Get("people/c9202708945039554846").Execute();

            List<Name> names = new List<Name>();
            names.Add(new Name() { HonorificPrefix = "Mr.", GivenName = "Nagesh", FamilyName = "Kulkarni" });
            contactToUpdate.Names = names;

            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(new EmailAddress() { Value = "nagesh@officeclip.com" });
            contactToUpdate.EmailAddresses = emailAddresses;

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
            phoneNumbers.Add(new PhoneNumber() { Value = "9876543210" });
            contactToUpdate.PhoneNumbers = phoneNumbers;

            PeopleResource.UpdateContactRequest updateContactRequest = 
                new PeopleResource.UpdateContactRequest(_service, contactToUpdate, contactToUpdate.ResourceName);

            Person updatedContact = updateContactRequest.Execute();
        }
    }
}
