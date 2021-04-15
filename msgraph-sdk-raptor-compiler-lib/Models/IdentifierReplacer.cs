using System;
using System.Text.RegularExpressions;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public class IdentifierReplacer
    {
        /// <summary>
        /// tree of IDs that appear in sample Graph URLs
        /// </summary>
        private readonly IDTree tree;

        /// <summary>
        /// regular expression to match strings like {namespace.type-id}
        /// also extracts namespace.type part separately so that we can use it as a lookup key.
        /// </summary>
        private readonly Regex idRegex = new Regex(@"{([A-Za-z\.]+)\-id}", RegexOptions.Compiled);

        public IdentifierReplacer(IDTree tree)
        {
            this.tree = tree;
        }

        /// <summary>
        /// Replaces ID placeholders of the form {name-id} by looking up name in the IDTree.
        /// If there is more than one placeholder, it traverses through the tree, e.g.
        /// For input string "sites/{site-id}/lists/{list-id}",
        ///   {site-id} is replaced by root->site entry from the IDTree.
        ///   {list-id} is replaced by root->site->list entry from the IDTree.
        /// </summary>
        /// <param name="input">String containing ID placeholders</param>
        /// <returns>input string, but its placeholders replaced from IDTree</returns>
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
                var id = match.Groups[0].Value;     // e.g. {site-id}
                var idType = match.Groups[1].Value; // e.g. extract site from {site-id}

                currentIdNode = currentIdNode[idType];
                input = input.Replace(id, currentIdNode.Value);
            }

            return input;
        }
    }
}
