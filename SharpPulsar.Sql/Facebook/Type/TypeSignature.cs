using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SharpPulsar.Sql.Precondition;

namespace SharpPulsar.Sql.Facebook.Type
{
    public class TypeSignature
    {
        #region Private Fields

        private static readonly Dictionary<string, string> BaseNameAliasToCanonical = new Dictionary<string, string>();

        static TypeSignature()
        {
            BaseNameAliasToCanonical.Add("int", StandardTypes.Integer);
        }

        #endregion

        #region Public Properties

        public string Base { get; }

        public IEnumerable<TypeSignatureParameter> Parameters { get; }

        public bool Calculated { get; }

        #endregion

        #region Constructors

        public TypeSignature(string @base, params TypeSignatureParameter[] parameters) : this(@base, parameters.ToList())
        {
        }

        [JsonConstructor]
        public TypeSignature(string @base, IEnumerable<TypeSignatureParameter> parameters)
        {
            ParameterCondition.NotNullOrEmpty(@base, "base");

            Base = @base;
            Parameters = parameters ?? throw new ArgumentNullException("parameters");
            Calculated = parameters.Any(x => x.Calculated);
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            if (Base.Equals(StandardTypes.Row))
            {
                return "";
            }
            else if (Base.Equals(StandardTypes.Varchar) &&
                Parameters.Count() == 1 &&
                Parameters.ElementAt(0).IsLongLiteral && Parameters.ElementAt(0).LongLiteral == VarcharType.UnboundedLength
                )
            {
                return Base;
            }
            else
            {
                var typeName = new StringBuilder(Base);

                if (Parameters.Any())
                {
                    typeName.Append($"({String.Join(",", Parameters.Select(x => x.ToString()))})");
                }

                return typeName.ToString();
            }
        }

        public IEnumerable<TypeSignature> GetTypeParametersAsTypeSignatures()
        {
            foreach (var parameter in Parameters)
            {
                if (parameter.Kind != ParameterKind.Type)
                {
                    throw new FormatException($"Expected all parameters to be TypeSignatures but {parameter.ToString()} was found");
                }

                yield return parameter.TypeSignature;
            }
        }

        #endregion
    }
}
