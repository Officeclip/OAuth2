using System;
using System.Collections;
using System.Collections.Generic;

namespace OfficeClip.OpenSource.OAuth2.Services.Google.Contacts
{
    public class ContactsGroup
    {
        private const string endpointUrl = "https://people.googleapis.com/v1/contactGroups";
        private string _uniqueId;

        //var contactGroup = new ContactsGroup(string uniqueId){
        //_uniqueId = uniqueId;
        //}

        public void CreateContact(Contact contact)
        {

        }


        public void DeleteContact(Contact contact)
        {

        }

        public void UpdateContact(Contact contact)
        {

        }
        public List<Contact> GetAllContacts()
        {
            return null;
        }

        public Contact GetContact(string contactUniqueId)
        {
            return null;
        }

    }
}
