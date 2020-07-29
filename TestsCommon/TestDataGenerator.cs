// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TestsCommon
{
    public class KnownIssue
    {
        /// <summary>
        /// owner of known issue
        /// This field is used to categorize known test failures, so that we can redirect issues faster
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// known issue message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// known issue constructor
        /// </summary>
        /// <param name="owner">owner of known issue</param>
        /// <param name="message">known issue message</param>
        public KnownIssue(string owner, string message)
        {
            Owner = owner;
            Message = message;
        }
    }

    public static class KnownIssues
    {
        /// <summary>
        /// Known issue message for cases where composable functions feature is missing in SDK
        /// </summary>
        private const string FeatureNotSupported = "Range composable functions are not supported by SDK\r\n"
            + "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/490";

        /// <summary>
        /// Known issue message for cases where types from different namespaces are referenced.
        /// </summary>
        private const string NamespacesSupport = "Multiple namespaces are not yet supported.";

        /// <summary>
        /// Known issue message for cases where HTTP snippet input should be fixed
        /// </summary>
        private const string HttpSnippetWrong = "Http snippet should be fixed";

        /// <summary>
        /// /// Known issue message for cases where Metadata should be fixed
        /// </summary>
        private const string MetadataWrong = "Metadata should be fixed";

        /// <summary>
        /// Known issue message for cases where HTTP sample needs to modify the URL to end with /$ref
        /// </summary>
        private const string RefNeeded = "URL needs to end with /$ref for reference types";

        /// <summary>
        /// Snippet generation flattens issue.
        /// </summary>
        private const string SnippetGenerationFlattens = "Snippet generation flattens the nested Odata queries, see https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/287 for more details";

        /// <summary>
        /// SDK team as owner
        /// </summary>
        private const string SDK = nameof(SDK);

        /// <summary>
        /// Owner value where HTTP snippet needs fixing
        /// </summary>
        private const string HTTP = nameof(HTTP);

        /// <summary>
        /// Owner value where HTTP snippet URL needs fixing to have camelCase
        /// </summary>
        private const string HTTPCamelCase = nameof(HTTPCamelCase);

        /// <summary>
        /// Owner value where HTTP snippet needs to fix HTTP method
        /// </summary>
        private const string HTTPMethodWrong = nameof(HTTPMethodWrong);

        /// <summary>
        /// Oner value where Metadata needs fixing
        /// </summary>
        private const string Metadata = nameof(Metadata);

        /// <summary>
        /// Snippet generation has a bug
        /// </summary>
        private const string SnippetGeneration = nameof(SnippetGeneration);

        #region HTTP methods

        private const string DELETE = nameof(DELETE);
        private const string PUT = nameof(PUT);
        private const string POST = nameof(POST);
        private const string GET = nameof(GET);
        private const string PATCH = nameof(PATCH);

        #endregion

        /// <summary>
        /// Constructs property not found message
        /// </summary>
        /// <param name="type">Type that need to define the property</param>
        /// <param name="property">Property that needs to be defined but missing in metadata</param>
        /// <returns>String representation of property not found message</returns>
        private static string GetPropertyNotFoundMessage(string type, string property)
        {
            return HttpSnippetWrong + $": {type} does not contain definition of {property} in metadata";
        }

        /// <summary>
        /// Constructs metadata errors where a reference property has ContainsTarget=true
        /// </summary>
        /// <param name="type">Type in metadata</param>
        /// <param name="property">Property in metadata</param>
        /// <returns>String representation of metadata error</returns>
        private static string GetContainsTargetRemoveMessage(string type, string property)
        {
            return MetadataWrong + $": {type}->{property} shouldn't have `ContainsTarget=true`";
        }

        /// <summary>
        /// Constructs casing wrong message
        /// </summary>
        /// <param name="wrongCasing">wrong casing in HTTP snippet, e.g. directoryroles</param>
        /// <param name="correctCasing">correct casing that should be in HTTP snippet, e.g. directoryRoles</param>
        /// <returns></returns>
        private static string GetCasingIssueMessage(string wrongCasing, string correctCasing)
        {
            return HttpSnippetWrong + $": {wrongCasing} should be renamed as {correctCasing}";
        }

        /// <summary>
        /// Constructs error message where HTTP method is wrong
        /// </summary>
        /// <param name="docsMethod">wrong HTTP method in docs</param>
        /// <param name="expectedMethod">expected HTTP method in the samples</param>
        /// <returns>String representation of HTTP method wrong error</returns>
        private static string GetMethodWrongMessage(string docsMethod, string expectedMethod)
        {
            return HttpSnippetWrong + $": Docs has HTTP method {docsMethod}, it should be {expectedMethod}";
        }

        /// <summary>
        /// Constructs error message where metadata needs to define a type
        /// </summary>
        /// <param name="typeName">type neme that needs to defined</param>
        /// <returns>String representation of metadata missing type definition error</returns>
        private static string GetTypeNotDefinedMessage(string typeName)
        {
            return MetadataWrong + $": {typeName} type is not defined in metadata.";
        }

        /// <summary>
        /// Gets known issues
        /// </summary>
        /// <returns>A mapping of test names into known issues</returns>
        public static Dictionary<string, KnownIssue> GetIssues()
        {
            return new Dictionary<string, KnownIssue>()
            {
                { "administrativeunit-delta-csharp-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("administrativeunits", "administrativeUnits")) },
                { "call-transfer-csharp-V1-compiles", new KnownIssue(Metadata, "v1 metadata doesn't have endpointType for invitationParticipantInfo") },
                { "call-updatemetadata-csharp-Beta-compiles", new KnownIssue(Metadata, "updateMetadata doesn't exist in metadata") },
                { "create-acceptedsender-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "acceptedSender")) },
                { "create-acceptedsender-csharp-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { "create-allowedgroup-from-printers-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("printerShare", "allowedGroups"))},
                { "create-alloweduser-from-printers-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("printerShare", "allowedUsers"))},
                { "create-certificatebasedauthconfiguration-from-certificatebasedauthconfiguration-csharp-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { "create-certificatebasedauthconfiguration-from-certificatebasedauthconfiguration-csharp-V1-compiles", new KnownIssue(HTTP, RefNeeded) },
                { "create-directoryobject-from-featurerolloutpolicy-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo"))},
                { "create-educationrubric-from-educationassignment-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("educationAssignment", "rubric"))},
                { "create-educationschool-from-educationroot-csharp-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("EducationSchool", "Status")) },
                { "create-item-attachment-from-eventmessage-csharp-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Item needs to be an OutlookItem object, not a string") },
                { "create-item-attachment-from-eventmessage-csharp-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Item needs to be an OutlookItem object, not a string") },
                { "create-manager-from-group-csharp-V1-compiles", new KnownIssue(SnippetGeneration, "See issue: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/289") },
                { "create-rangeborder-from-rangeformat-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "create-rangeborder-from-rangeformat-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "create-reference-attachment-with-post-csharp-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ReferenceAttachment", "SourceUrl, ProviderType, Permission and IsFolder")) },
                { "create-rejectedsender-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { "create-rejectedsenders-from-group-csharp-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { "create-serviceprincipal-from-serviceprincipals-csharp-Beta-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { "create-serviceprincipal-from-serviceprincipals-csharp-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { "create-tablecolumn-from-table-csharp-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Id should be string not int") },
                { "create-tablecolumn-from-table-csharp-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Id should be string not int") },
                { "delete-acceptedsenders-from-group-csharp-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "acceptedSender")) },
                { "delete-alloweduser-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("printer", "allowedUsers")) },
                { "delete-directoryobject-from-directoryrole-csharp-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("directoryroles", "directoryRoles")) },
                { "delete-directoryobject-from-featurerolloutpolicy-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { "delete-educationrubric-from-educationassignment-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("educationAssignment", "rubric"))},
                { "follow-site-csharp-Beta-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "follow-site-csharp-V1-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "get-borders-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-borders-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-callrecord-csharp-Beta-compiles", new KnownIssue(SDK, NamespacesSupport) },
                { "get-callrecord-csharp-V1-compiles", new KnownIssue(SDK, NamespacesSupport) },
                { "get-callrecord-expanded-csharp-Beta-compiles", new KnownIssue(SDK, NamespacesSupport) },
                { "get-callrecord-expanded-csharp-V1-compiles", new KnownIssue(SDK, NamespacesSupport) },
                { "get-callrecord-sessions-csharp-Beta-compiles", new KnownIssue(SDK, NamespacesSupport) },
                { "get-callrecord-sessions-csharp-V1-compiles", new KnownIssue(SDK, NamespacesSupport) },
                { "get-callrecord-sessions-expanded-csharp-Beta-compiles", new KnownIssue(SDK, NamespacesSupport) },
                { "get-callrecord-sessions-expanded-csharp-V1-compiles", new KnownIssue(SDK, NamespacesSupport) },
                { "get-endpoint-csharp-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { "get-endpoints-csharp-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { "get-formatprotection-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-formatprotection-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-message-in-mime-csharp-Beta-compiles", new KnownIssue(Metadata, "Message entity doesn't have hasStream=true.") },
                { "get-message-in-mime-csharp-V1-compiles", new KnownIssue(Metadata, "Message entity doesn't have hasStream=true.") },
                { "get-opentypeextension-3-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "get-opentypeextension-3-csharp-V1-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "get-rangeborder-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rangeborder-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rangebordercollection-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rangebordercollection-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rangefill-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rangefill-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rangefont-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rangefont-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rangeformat-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rangeformat-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rooms-in-roomlist-csharp-Beta-compiles", new KnownIssue(SDK, "SDK doesn't generate type segment in OData URL. https://microsoftgraph.visualstudio.com/Graph%20Developer%20Experiences/_workitems/edit/4997") },
                { "get-rooms-in-roomlist-csharp-V1-compiles", new KnownIssue(SDK, "SDK doesn't generate type segment in OData URL. https://microsoftgraph.visualstudio.com/Graph%20Developer%20Experiences/_workitems/edit/4997") },
                { "get-rows-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-rows-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-scopedadministratorof-csharp-Beta-compiles", new KnownIssue(Metadata, GetTypeNotDefinedMessage("ScopedAdministratorOf")) },
                { "get-singlevaluelegacyextendedproperty-1-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "get-singlevaluelegacyextendedproperty-1-csharp-V1-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "informationprotectionlabel-evaluateapplication-csharp-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("informationprotection","informationProtection")) },
                { "informationprotectionlabel-evaluateclassificationresults-csharp-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("informationprotection","informationProtection")) },
                { "informationprotectionlabel-evaluateremoval-csharp-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("informationprotection","informationProtection")) },
                { "informationprotectionlabel-extractlabel-csharp-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("informationprotection","informationProtection")) },
                { "list-serviceprincipal-csharp-Beta-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { "list-serviceprincipal-csharp-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { "nameditem-range-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "oauth2permissiongrant-delta-csharp-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("oAuth2permissiongrants","oAuth2PermissionGrants")) },
                { "oauth2permissiongrant-delta-csharp-V1-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("oAuth2permissiongrants","oAuth2PermissionGrants")) },
                { "post-reply-csharp-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Odata.Type for concreate Attachment type should be added") },
                { "post-reply-csharp-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Odata.Type for concreate Attachment type should be added") },
                { "printer-getcapabilities-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "range-cell-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-clear-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-clear-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-column-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-delete-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-delete-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-entirecolumn-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-entirecolumn-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-entirerow-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-entirerow-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-insert-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-insert-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-lastcell-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-lastcell-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-lastcolumn-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-lastcolumn-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-lastrow-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-lastrow-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-merge-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-merge-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-unmerge-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-unmerge-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-usedrange-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-usedrange-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-usedrange-valuesonly-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "rangefill-clear-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "rangefill-clear-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "rangeformat-autofitcolumns-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "rangeformat-autofitcolumns-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "rangeformat-autofitrows-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "rangeformat-autofitrows-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "rangesort-apply-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "rangesort-apply-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "remove-group-from-rejectedsenderslist-of-group-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { "remove-rejectedsender-from-group-csharp-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { "remove-user-from-rejectedsenderslist-of-group-csharp-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { "schedule-put-schedulinggroups-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "securescorecontrolprofiles-update-csharp-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },
                { "serviceprincipal-addkey-csharp-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { "serviceprincipal-delete-owners-csharp-Beta-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { "serviceprincipal-delete-owners-csharp-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { "serviceprincipal-removekey-csharp-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { "shift-get-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "shift-put-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "table-databodyrange-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "table-headerrowrange-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "table-headerrowrange-csharp-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "table-totalrowrange-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "table-totalrowrange-csharp-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "tablecolumn-databodyrange-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "tablecolumn-headerrowrange-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "tablecolumn-headerrowrange-csharp-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "tablecolumn-range-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "tablecolumn-range-csharp-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "tablecolumn-totalrowrange-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "tablecolumn-totalrowrange-csharp-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "tablerow-range-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "timeoff-put-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "timeoff-put-csharp-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "timeoffreason-put-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "timeoffreason-put-csharp-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "trustframeworkkeyset-getactivekey-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { "unfollow-item-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(DELETE, POST)) },
                { "unfollow-item-csharp-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(DELETE, POST)) },
                { "unfollow-site-csharp-Beta-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "unfollow-site-csharp-V1-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "update-activitybasedtimeoutpolicy-csharp-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ActivityBasedTimeoutPolicy", "Type")) },
                { "update-activitybasedtimeoutpolicy-csharp-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ActivityBasedTimeoutPolicy", "Type")) },
                { "update-claimsmappingpolicy-csharp-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ClaimsMappingPolicy", "Type")) },
                { "update-formatprotection-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-formatprotection-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-homerealmdiscoverypolicy-csharp-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("HomeRealmDiscoveryPolicy", "Type")) },
                { "update-homerealmdiscoverypolicy-csharp-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("HomeRealmDiscoveryPolicy", "Type")) },
                { "update-openshift-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-page-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "See issue: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/288") },
                { "update-page-csharp-V1-compiles", new KnownIssue(SnippetGeneration, "See issue: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/288") },
                { "update-phoneauthenticationmethod-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-rangeborder-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeborder-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangefill-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangefill-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangefont-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangefont-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-fill-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-fill-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-fill-three-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-fill-three-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-fill-two-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-fill-two-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-font-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-font-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-font-three-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-font-three-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-font-two-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-font-two-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-three-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-three-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-two-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-rangeformat-two-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-room-csharp-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Capacity should be int, isWheelchairAccessible should be renamed as isWheelChairAccessible") },
                { "update-room-csharp-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Capacity should be int, isWheelchairAccessible should be renamed as isWheelChairAccessible") },
                { "update-roomlist-csharp-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("roomlist", "roomList")) },
                { "update-roomlist-csharp-V1-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("roomlist", "roomList")) },
                { "update-synchronizationschema-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-synchronizationtemplate-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-tokenissuancepolicy-csharp-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenIssuancePolicy", "Type")) },
                { "update-tokenissuancepolicy-csharp-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenIssuancePolicy", "Type")) },
                { "update-tokenlifetimepolicy-csharp-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenLifetimePolicy", "Type")) },
                { "update-tokenlifetimepolicy-csharp-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenLifetimePolicy", "Type")) },
                { "update-trustframeworkkeyset-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-workforceintegration-csharp-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": workforceintegration id is needed in the url.") },
                { "update-workforceintegration-csharp-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": workforceintegration id is needed in the url.") },
                { "workbookrange-columnsafter-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-columnsafter-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-columnsbefore-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-columnsbefore-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsabove-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsabove-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsabove-nocount-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsbelow-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsbelow-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsbelow-nocount-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-visibleview-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-visibleview-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrangeview-range-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrangeview-range-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "worksheet-range-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
            };
        }
    }

    /// <summary>
    /// Generates TestCaseData for NUnit
    /// </summary>
    public static class TestDataGenerator
    {
        /// <summary>
        /// Snippet links as shown in markdown files in docs repo
        /// </summary>
        private const string SnippetLinkPattern = @"includes\/snippets\/csharp\/(.*)\-csharp\-snippets\.md";

        /// <summary>
        /// Regex matching the pattern above
        /// </summary>
        private static readonly Regex SnippetLinkRegex = new Regex(SnippetLinkPattern, RegexOptions.Compiled);

        /// <summary>
        /// Generates a dictionary mapping from snippet file name to documentation page listing the snippet.
        /// </summary>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        /// <returns>Dictionary holding the mapping from snippet file name to documentation page listing the snippet.</returns>
        private static Dictionary<string, string> GetDocumentationLinks(Versions version)
        {
            var documentationLinks = new Dictionary<string, string>();
            var documentationDirectory = GraphDocsDirectory.GetDocumentationDirectory(version);
            var files = Directory.GetFiles(documentationDirectory);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                var docsLink = $"https://docs.microsoft.com/en-us/graph/api/{fileName}?view=graph-rest-{new VersionString(version)}&tabs=csharp";

                var match = SnippetLinkRegex.Match(content);
                while (match.Success)
                {
                    documentationLinks[match.Groups[1].Value + "-csharp-snippets.md"] = docsLink;
                    match = match.NextMatch();
                }
            }

            return documentationLinks;
        }

        /// <summary>
        /// For each snippet file creates a test case which takes the file name and version as reference
        /// Test case name is also set to to unique name based on file name
        /// </summary>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        /// <param name="knownFailuresRequested">return whether known failures as test cases or not</param>
        /// <returns>
        /// TestCaseData to be consumed by C# compilation tests
        /// </returns>
        public static IEnumerable<TestCaseData> GetTestCaseData(Versions version, bool knownFailuresRequested = false)
        {
            var documentationLinks = GetDocumentationLinks(version);
            var knownIssues = KnownIssues.GetIssues();
            var snippetFileNames = documentationLinks.Keys.ToList();
            return from fileName in snippetFileNames                                // e.g. application-addpassword-csharp-snippets.md
                   let testNamePostfix = version.ToString() + "-compiles"           // e.g. Beta-compiles
                   let testName = fileName.Replace("snippets.md", testNamePostfix)  // e.g. application-addpassword-csharp-Beta-compiles
                   let docsLink = documentationLinks[fileName]
                   let isKnownIssue = knownIssues.ContainsKey(testName)
                   let knownIssue = isKnownIssue ? knownIssues[testName] : null
                   let knownIssueMessage = knownIssue?.Message ?? string.Empty
                   let owner = knownIssue?.Owner ?? string.Empty
                   let testCaseData = new CsharpTestData
                   {
                       Version = version,
                       IsKnownIssue = isKnownIssue,
                       KnownIssueMessage = knownIssueMessage,
                       DocsLink = docsLink,
                       FileName = fileName
                   }
                   where !(isKnownIssue ^ knownFailuresRequested) // select known issues if requested
                   select new TestCaseData(testCaseData).SetName(testName).SetProperty("Owner", owner);
        }
    }
}
