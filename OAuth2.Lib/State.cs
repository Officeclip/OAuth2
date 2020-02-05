using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public class State
    {
        public Dictionary<string, string> StateValues;
        private const string NonceName = "Nonce";

        public bool IsValid
        {
            get; private set;
        }

        /// <summary>
        /// Used for generation of state
        /// </summary>
        /// <param name="nonceValue">the nonce value that will be compared later. User empty string if no
        /// comparison is needed</param>
        public State(string nonceValue)
        {
            StateValues = new Dictionary<string, string>
                                                    {
                                                        { NonceName, nonceValue }
                                                    };
        }

        /// <summary>
        /// Used for parsing the value returned from the provider
        /// </summary>
        /// <param name="stateString">The state string returned by the provider</param>
        /// <param name="nonceValue">the nonce string to compare. Use blank string if no comparison is needed</param>
        public State(string stateString, string nonceValue)
        {
            string jsonString = HttpUtility.UrlDecode(stateString);
            StateValues = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(jsonString);
            if (nonceValue.Length > 0)
            {
                var nonceReturnedValue = StateValues[NonceName];
                IsValid = (nonceValue == nonceReturnedValue);
            }
            IsValid = true;
        }

        /// <summary>
        /// This constructor should always used to duplicate the state object because once used
        /// it becomes obsolete.
        /// </summary>
        /// <param name="previousState"></param>
        public State(State previousState)
        {
            StateValues = new Dictionary<string, string>();
            foreach (var key in previousState.StateValues.Keys)
            {
                StateValues[key] = previousState.StateValues[key];
            }
        }

        public void Add(string key, string value)
        {
            StateValues.Add(
                key,
                value ?? string.Empty);
        }

        public string GetValue(string key)
        {
            return StateValues[key];
        }

        public override string ToString()
        {
            string stateValue = new JavaScriptSerializer().Serialize(StateValues);
            return HttpUtility.UrlEncode(stateValue);
        }
    }
}
