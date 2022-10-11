using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Joins;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpPulsar.Common.Naming;

namespace SharpPulsar.Common
{
    public class TopicList
    {
        public const string AllTopicsPattern = ".*";

        private const string SchemeSeparator = "://";

        private static readonly Regex schemeSeparatorPattern = new Regex(SchemeSeparator, RegexOptions.Compiled); 

        // get topics that match 'topicsPattern' from original topics list
        // return result should contain only topic names, without partition part
        public static IList<string> FilterTopics(IList<string> original, string regex)
        {
            var topicsPattern = new Regex(regex);
            return FilterTopics(original, topicsPattern);
        }
        /* public static IList<string> FilterTopics(IList<string> original, Regex topicsPattern)
         {
             var shortenedTopicsPattern = topicsPattern.ToString().Contains(SchemeSeparator) ? new Regex(schemeSeparatorPattern.Split(topicsPattern.ToString())[1]) : topicsPattern;
             var get = original.Select(x => TopicName.Get(x).ToString());

             return get.Where(topic => shortenedTopicsPattern.Match(schemeSeparatorPattern.Split(topic)[1]).Success).ToList();
         }*/
        public static IList<string> FilterTopics(IList<string> original, Regex topicsPattern)
        {
            var pattern = topicsPattern.ToString().Contains(SchemeSeparator) ? new Regex(Regex.Split(topicsPattern.ToString(), @"\:\/\/")[1]) : topicsPattern;

            return original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(Regex.Split(topic, @"\:\/\/")[1]).Success).ToList();
        }
        public static IList<string> FilterTransactionInternalName(IList<string> original)
        {
            return original.Where(topic => !SystemTopicNames.IsTransactionInternalName(TopicName.Get(topic))).ToList();
        }
/*
        public static string CalculateHash(IList<string> Topics)
        {
            // JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
            return Hashing.crc32c().hashBytes(Topics.OrderBy(c => c).collect(Collectors.joining(",")).getBytes(StandardCharsets.UTF_8)).ToString();
        }


*/
        // get topics, which are contained in list1, and not in list2
        public static ISet<string> Minus(ICollection<string> list1, ICollection<string> list2)
        {
            var s1 = new HashSet<string>(list1);
            foreach (var l in list2)
            {
                s1.Remove(l);
            }
            return s1;
        }

    }
}
