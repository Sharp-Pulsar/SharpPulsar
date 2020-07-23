using System.Text;

namespace SharpPulsar.Presto.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.security.Identity.java
    /// </summary>
    public class Identity
    {
        #region Public Properties

        public string User { get; }

        /// <summary>
        /// TODO: This is a java.security.Principal object
        /// </summary>
        public dynamic Principal { get; }

        #endregion

        #region Constructors

        public Identity(string user, dynamic principal)
        {
            ParameterCondition.NotNullOrEmpty(user, "user");

            User = user;
            Principal = principal;
        }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var id = (Identity)obj;

            return Equals(User, id.User);
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(User);
        }

        public override string ToString()
        {
            var sb = new StringBuilder("Identity{");
            sb.Append("user='").Append(User).Append("\'");

            if (Principal != null)
            {
                sb.Append(", prinicipal="); //.Append(this.Principal.Get());
            }

            sb.Append("}");

            return sb.ToString();
        }

        #endregion
    }
}
