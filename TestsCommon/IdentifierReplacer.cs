using System;
using System.Text.RegularExpressions;

namespace TestsCommon
{
    public class IdentifierReplacer
    {
        private readonly IDTree tree;
        private readonly Regex idRegex = new Regex(@"{([A-Za-z\.]+)\-id}", RegexOptions.Compiled);

        public IdentifierReplacer(IDTree tree)
        {
            this.tree = tree;
        }

        public string ReplaceIds(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var matches = idRegex.Matches(input);
            IDTree currentIdNode = tree;
            foreach (Match match in matches)
            {
                var id = match.Groups[0].Value; // e.g. {application-id}
                var idType = match.Groups[1].Value; // e.g. extract application from {application-id}

                currentIdNode = currentIdNode[idType];
                input = input.Replace(id, currentIdNode.Value);
            }

            return input;
        }
    }
}
