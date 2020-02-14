using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Sql.Facebook.Type
{
    public class StringHelper
    {
        #region Private Fields

        private List<KeyValuePair<string, object>> _values;

        #endregion

        #region Public Properties

        public System.Type Type { get; }

        #endregion

        #region Constructors

        private StringHelper(System.Type type)
        {
            Type = type;
            _values = new List<KeyValuePair<string, object>>();
        }

        #endregion

        #region Public Methods

        public static StringHelper Build(object baseObject)
        {
            return new StringHelper(baseObject.GetType());
        }

        public StringHelper Add(string parameterName, object value)
        {
            _values.Add(new KeyValuePair<string, object>(parameterName, value));
            return this;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Type.Name} {{");

            foreach (KeyValuePair<string, object> item in _values)
            {
                object value = item.Value;

                if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
                {
                    sb.Append($"{item.Key}=[{String.Join(",", (IList)value)}], ");
                }
                else
                {
                    sb.Append($"{item.Key}={value.ToString()}, ");
                }
            }

            sb.Length = sb.Length - 2; // Remove the last space and comma
            sb.Append("}");

            return sb.ToString();
        }
        #endregion
    }
}
