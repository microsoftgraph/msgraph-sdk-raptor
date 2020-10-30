// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System;
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
        #region SDK issues

        private const string FeatureNotSupported = "Range composable functions are not supported by SDK\r\n"
            + "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/490";
        private const string SearchHeaderIsNotSupported = "Search header is not supported by the SDK";
        private const string CountIsNotSupported = "OData $count is not supported by the SDK at the moment";
        private const string MissingContentProperty = "IReportRootGetM365AppPlatformUserCountsRequestBuilder is missing Content property";
        private const string NamespacesSupport = "Multiple namespaces are not yet supported.";

        #endregion

        #region HTTP Snippet Issues
        private const string HttpSnippetWrong = "Http snippet should be fixed";
        private const string RefNeeded = "URL needs to end with /$ref for reference types";
        private const string RefShouldBeRemoved = "URL shouldn't end with /$ref";
        #endregion

        #region Metadata Issues
        private const string MetadataWrong = "Metadata should be fixed";
        private const string IdentityRiskEvents = "identityRiskEvents not defined in metadata.";
        #endregion

        #region Metadata Preprocessing Issues
        private const string ProposedNewTimeIsDropped = "Metadata preprocessing is dropping `ProposedNewTime`." +
            " See https://github.com/microsoftgraph/msgraph-metadata/issues/21";
        #endregion

        #region Snipppet Generation Issues
        private const string SnippetGenerationFlattens = "Snippet generation flattens the nested Odata queries." +
            " See https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/287";
        private const string SnippetGenerationAdditionalData = "Open types should use additional data for non-existent properties." +
            " See https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/296";
        private const string SnippetGenerationCreateAsyncSupport = "Snippet generation doesn't use CreateAsync" +
            " See https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/301";
        private const string SnippetGenerationTypeHint = "Type hint is missing in null assignments" +
            " See https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/297";
        private const string SnippetGenerationReferenceCreation = "Reference creation should use id instead of full object" +
            " See https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/300";
        private const string SnippetGenerationRequestObjectDisambiguation = "Snippet generation should rename objects that end with Request to end with RequestObject" +
            " See https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/298";
        #endregion

        #region Test Owner values (to categorize results in Azure DevOps)
        private const string SDK = nameof(SDK);
        private const string HTTP = nameof(HTTP);
        private const string HTTPCamelCase = nameof(HTTPCamelCase);
        private const string HTTPMethodWrong = nameof(HTTPMethodWrong);
        private const string Metadata = nameof(Metadata);
        private const string MetadataPreprocessing = nameof(MetadataPreprocessing);
        private const string SnippetGeneration = nameof(SnippetGeneration);
        #endregion

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
        /// Returns a mapping of issues of which the source comes from service/documentation/metadata and are common accross langauges
        /// </summary>
        /// <param name="language">language to generate the exception from</param>
        /// <returns>mapping of issues of which the source comes from service/documentation/metadata and are common accross langauges</returns>
        public static Dictionary<string, KnownIssue> GetCommonIssues(Languages language)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            var lng = language.ToString().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
            return new Dictionary<string, KnownIssue>()
            {
                { $"administrativeunit-delta-{lng}-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("administrativeunits", "administrativeUnits")) },
                { $"convert-team-from-group-{lng}-V1-compiles", new KnownIssue(HTTP, "isFavoriteByDefault is only available in Beta. https://github.com/microsoftgraph/microsoft-graph-docs/issues/10145") },
                { $"convert-team-from-non-standard2-{lng}-V1-compiles", new KnownIssue(HTTP, "isFavoriteByDefault is only available in Beta. https://github.com/microsoftgraph/microsoft-graph-docs/issues/10145") },
                { $"call-transfer-{lng}-V1-compiles", new KnownIssue(Metadata, "v1 metadata doesn't have endpointType for invitationParticipantInfo") },
                { $"call-updatemetadata-{lng}-Beta-compiles", new KnownIssue(Metadata, "updateMetadata doesn't exist in metadata") },
                { $"create-acceptedsender-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "acceptedSender")) },
                { $"create-acceptedsender-{lng}-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"create-allowedgroup-from-printers-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("printerShare", "allowedGroups"))},
                { $"create-alloweduser-from-printers-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("printerShare", "allowedUsers"))},
                { $"create-certificatebasedauthconfiguration-from-certificatebasedauthconfiguration-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"create-certificatebasedauthconfiguration-from-certificatebasedauthconfiguration-{lng}-V1-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"create-directoryobject-from-featurerolloutpolicy-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo"))},
                { $"create-directoryobject-from-orgcontact-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"create-educationrubric-from-educationassignment-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("educationAssignment", "rubric"))},
                { $"create-educationschool-from-educationroot-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("EducationSchool", "Status")) },
                { $"create-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"create-homerealmdiscoverypolicy-from-serviceprincipal-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"create-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"create-item-attachment-from-eventmessage-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Item needs to be an OutlookItem object, not a string") },
                { $"create-item-attachment-from-eventmessage-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Item needs to be an OutlookItem object, not a string") },
                { $"create-onpremisesagentgroup-from-publishedresource-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"create-reference-attachment-with-post-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ReferenceAttachment", "SourceUrl, ProviderType, Permission and IsFolder")) },
                { $"create-rejectedsender-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"create-rejectedsenders-from-group-{lng}-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"create-serviceprincipal-from-serviceprincipals-{lng}-Beta-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { $"create-serviceprincipal-from-serviceprincipals-{lng}-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { $"create-tablecolumn-from-table-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Id should be string not int") },
                { $"create-tablecolumn-from-table-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Id should be string not int") },
                { $"create-team-post-full-payload-{lng}-Beta-compiles", new KnownIssue(HTTP, "name should be displayName on teamsTab objects") },
                { $"create-tokenlifetimepolicy-from-application-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"delete-acceptedsenders-from-group-{lng}-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "acceptedSender")) },
                { $"delete-alloweduser-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("printer", "allowedUsers")) },
                { $"delete-directoryobject-from-directoryrole-{lng}-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("directoryroles", "directoryRoles")) },
                { $"delete-directoryobject-from-featurerolloutpolicy-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-educationrubric-from-educationassignment-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("educationAssignment", "rubric"))},
                { $"delete-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"delete-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"delete-permission-{lng}-Beta-compiles", new KnownIssue(HTTP, "HTTP sample needs to remove `root` from the URL") },
                { $"delete-publishedresource-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"directoryobject-delta-{lng}-Beta-compiles", new KnownIssue(Metadata, "Delta is not defined on directoryObject, but on user and group") },
                { $"event-decline-{lng}-Beta-compiles", new KnownIssue(MetadataPreprocessing, ProposedNewTimeIsDropped) },
                { $"event-tentativelyaccept-{lng}-Beta-compiles", new KnownIssue(MetadataPreprocessing, ProposedNewTimeIsDropped) },
                { $"get-endpoint-{lng}-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { $"get-endpoints-{lng}-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { $"get-identityriskevent-{lng}-Beta-compiles", new KnownIssue(HTTP, IdentityRiskEvents) },
                { $"get-identityriskevents-{lng}-Beta-compiles", new KnownIssue(HTTP, IdentityRiskEvents) },
                { $"get-user-oauth2permissiongrants-{lng}-Beta-compiles", new KnownIssue(Metadata, "Oauth2PermissionGrants are not defined for user") },
                { $"informationprotectionlabel-evaluateapplication-{lng}-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("informationprotection","informationProtection")) },
                { $"informationprotectionlabel-evaluateclassificationresults-{lng}-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("informationprotection","informationProtection")) },
                { $"informationprotectionlabel-evaluateremoval-{lng}-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("informationprotection","informationProtection")) },
                { $"informationprotectionlabel-extractlabel-{lng}-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("informationprotection","informationProtection")) },
                { "list-conversation-members-csharp-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + "Me doesn't have \"Chats\". \"Chats\" is a high level EntitySet.") },
                { $"list-serviceprincipal-{lng}-Beta-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { $"list-serviceprincipal-{lng}-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { $"nameditem-range-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"oauth2permissiongrant-delta-{lng}-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("oAuth2permissiongrants","oAuth2PermissionGrants")) },
                { $"oauth2permissiongrant-delta-{lng}-V1-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("oAuth2permissiongrants","oAuth2PermissionGrants")) },
                { $"participant-configuremixer-{lng}-Beta-compiles", new KnownIssue(Metadata, "ConfigureMixer doesn't exist in metadata") },
                { $"post-reply-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Odata.Type for concrete Attachment type should be added") },
                { $"post-reply-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Odata.Type for concrete Attachment type should be added") },
                { $"printer-getcapabilities-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"remove-group-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"remove-rejectedsender-from-group-{lng}-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"remove-user-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"removeonpremisesagentfromanonpremisesagentgroup-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"schedule-put-schedulinggroups-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"securescorecontrolprofiles-update-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },
                { $"serviceprincipal-addkey-{lng}-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { $"serviceprincipal-delete-owners-{lng}-Beta-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { $"serviceprincipal-delete-owners-{lng}-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { $"serviceprincipal-removekey-{lng}-V1-compiles", new KnownIssue(HTTP, GetCasingIssueMessage("serviceprincipal", "servicePrincipal")) },
                { $"shift-get-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"shift-put-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"table-databodyrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"table-headerrowrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"table-headerrowrange-{lng}-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"table-totalrowrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"table-totalrowrange-{lng}-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-databodyrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-headerrowrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-headerrowrange-{lng}-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-range-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-range-{lng}-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-totalrowrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-totalrowrange-{lng}-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablerow-range-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"timeoff-put-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"timeoff-put-{lng}-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"timeoffreason-put-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"timeoffreason-put-{lng}-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"unfollow-item-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(DELETE, POST)) },
                { $"unfollow-item-{lng}-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(DELETE, POST)) },
                { $"update-activitybasedtimeoutpolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ActivityBasedTimeoutPolicy", "Type")) },
                { $"update-activitybasedtimeoutpolicy-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ActivityBasedTimeoutPolicy", "Type")) },
                { $"update-claimsmappingpolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ClaimsMappingPolicy", "Type")) },
                { $"update-homerealmdiscoverypolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("HomeRealmDiscoveryPolicy", "Type")) },
                { $"update-homerealmdiscoverypolicy-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("HomeRealmDiscoveryPolicy", "Type")) },
                { $"update-openidconnectprovider-{lng}-Beta-compiles", new KnownIssue(HTTP, "OpenIdConnectProvider should be specified") },
                { $"update-openshift-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"update-room-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Capacity should be int, isWheelchairAccessible should be renamed as isWheelChairAccessible") },
                { $"update-room-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Capacity should be int, isWheelchairAccessible should be renamed as isWheelChairAccessible") },
                { $"update-roomlist-{lng}-Beta-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("roomlist", "roomList")) },
                { $"update-roomlist-{lng}-V1-compiles", new KnownIssue(HTTPCamelCase, GetCasingIssueMessage("roomlist", "roomList")) },
                { $"update-synchronizationschema-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"update-synchronizationtemplate-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"update-teamsapp-{lng}-V1-compiles", new KnownIssue(Metadata, $"teamsApp needs hasStream=true. In addition to that, we need these fixed: {Environment.NewLine}https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/160 {Environment.NewLine}https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/336") },
                { $"update-tokenissuancepolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenIssuancePolicy", "Type")) },
                { $"update-tokenissuancepolicy-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenIssuancePolicy", "Type")) },
                { $"update-tokenlifetimepolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenLifetimePolicy", "Type")) },
                { $"update-tokenlifetimepolicy-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenLifetimePolicy", "Type")) },
                { $"update-trustframeworkkeyset-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"update-workforceintegration-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": workforceintegration id is needed in the url.") },
                { $"update-workforceintegration-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": workforceintegration id is needed in the url.") },
                { $"update-phoneauthenticationmethod-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"worksheet-range-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
            };
        }

        /// <summary>
        /// Gets known issues
        /// </summary>
        /// <returns>A mapping of test names into known CSharp issues</returns>
        public static Dictionary<string, KnownIssue> GetCSharpIssues()
        {
            return new Dictionary<string, KnownIssue>()
            {
                { "call-transfer-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationAdditionalData) },
                { "create-b2cuserflow-from-b2cuserflows-identityprovider-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "Snippet Generation needs casting support for *CollectionWithReferencesPage. See details at: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/327") },
                { "create-b2xuserflow-from-b2xuserflows-identityproviders-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "Snippet Generation needs casting support for *CollectionWithReferencesPage. See details at: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/327") },
                { "create-externalitem-from-connections-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },
                { "create-manager-for-user-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationReferenceCreation) },
                { "create-manager-from-group-csharp-V1-compiles", new KnownIssue(SnippetGeneration, "See issue: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/289") },
                { "create-printer-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationTypeHint) },
                { "create-rangeborder-from-rangeformat-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "create-rangeborder-from-rangeformat-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "create-schema-from-connection-async-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },
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
                { "get-count-group-only-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-count-only-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-count-user-only-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-formatprotection-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-formatprotection-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-group-transitivemembers-count-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-opentypeextension-3-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "get-opentypeextension-3-csharp-V1-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "get-phone-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-pr-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
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
                { "get-singlevaluelegacyextendedproperty-1-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "get-singlevaluelegacyextendedproperty-1-csharp-V1-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "get-team-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-tier-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-user-memberof-count-only-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-video-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-wa-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-web-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "post-privilegedroleassignmentrequest-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { "put-privilegedrolesettings-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },
                { "put-regionalandlanguagesettings-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },
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
                { "reportroot-getm365appplatformusercounts-csv-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appplatformusercounts-json-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercoundetail-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercountdetail-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercounts-csv-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercounts-json-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "synchronizationschema-parseexpression-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationTypeHint) },
                { "tablerowcollection-add-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationTypeHint) },
                { "team-put-schedule-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },
                { "unfollow-site-csharp-Beta-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "unfollow-site-csharp-V1-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "update-formatprotection-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-formatprotection-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-page-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "See issue: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/288") },
                { "update-page-csharp-V1-compiles", new KnownIssue(SnippetGeneration, "See issue: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/288") },
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
                { "update-settings-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "Naming inconsistency in variables. See https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/299 for more details") },
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
            };
        }
        /// <summary>
        /// Gets known issues
        /// </summary>
        /// <returns>A mapping of test names into known Java issues</returns>
        public static Dictionary<string, KnownIssue> GetJavaIssues()
        {
            return new Dictionary<string, KnownIssue>()
            {

            };
        }
        /// <summary>
        /// Gets known issues by language
        /// </summary>
        /// <param name="language">language to get the issues for</param>
        /// <returns>A mapping of test names into known issues</returns>
        public static Dictionary<string, KnownIssue> GetIssues(Languages language)
        {
            return (language switch
            {
                Languages.CSharp => GetCSharpIssues(),
                Languages.Java => GetJavaIssues(),
                _ => new Dictionary<string, KnownIssue>(),
            }).Union(GetCommonIssues(language)).ToDictionary(x => x.Key, x => x.Value);
        }
    }
    /// <summary>
    /// Generates TestCaseData for NUnit
    /// </summary>
    public static class TestDataGenerator
    {
        /// <summary>
        /// Generates a dictionary mapping from snippet file name to documentation page listing the snippet.
        /// </summary>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        /// <returns>Dictionary holding the mapping from snippet file name to documentation page listing the snippet.</returns>
        private static Dictionary<string, string> GetDocumentationLinks(Versions version, Languages language)
        {
            var documentationLinks = new Dictionary<string, string>();
            var documentationDirectory = GraphDocsDirectory.GetDocumentationDirectory(version);
            var files = Directory.GetFiles(documentationDirectory);
#pragma warning disable CA1308 // Normalize strings to uppercase
            var languageName = language.ToString().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
            var SnippetLinkPattern = @$"includes\/snippets\/{languageName}\/(.*)\-{languageName}\-snippets\.md";
            var SnippetLinkRegex = new Regex(SnippetLinkPattern, RegexOptions.Compiled);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                var docsLink = $"https://docs.microsoft.com/en-us/graph/api/{fileName}?view=graph-rest-{new VersionString(version).DocsUrlSegment()}&tabs={languageName}";

                var match = SnippetLinkRegex.Match(content);
                while (match.Success)
                {
                    documentationLinks[$"{match.Groups[1].Value}-{languageName}-snippets.md"] = docsLink;
                    match = match.NextMatch();
                }
            }

            return documentationLinks;
        }

        /// <summary>
        /// For each snippet file creates a test case which takes the file name and version as reference
        /// Test case name is also set to to unique name based on file name
        /// </summary>
        /// <param name="runSettings">Test run settings</param>
        /// <returns>
        /// TestCaseData to be consumed by C# compilation tests
        /// </returns>
        public static IEnumerable<TestCaseData> GetTestCaseData(RunSettings runSettings)
        {
            if (runSettings == null)
            {
                throw new ArgumentNullException(nameof(runSettings));
            }

            var version = runSettings.Version;
            var documentationLinks = GetDocumentationLinks(version, language);
            var knownIssues = KnownIssues.GetIssues(language);
            var snippetFileNames = documentationLinks.Keys.ToList();
            return from fileName in snippetFileNames                                            // e.g. application-addpassword-csharp-snippets.md
                   let arbitraryDllPostfix = runSettings.DllPath == null ? string.Empty : "arbitraryDll-"
                   let testNamePostfix = arbitraryDllPostfix + version.ToString() + "-compiles" // e.g. Beta-compiles or arbitraryDll-Beta-compiles
                   let testName = fileName.Replace("snippets.md", testNamePostfix)              // e.g. application-addpassword-csharp-Beta-compiles
                   let docsLink = documentationLinks[fileName]
                   let knownIssueLookupKey = testName.Replace("arbitraryDll-", string.Empty)
                   let isKnownIssue = knownIssues.ContainsKey(knownIssueLookupKey)
                   let knownIssue = isKnownIssue ? knownIssues[knownIssueLookupKey] : null
                   let knownIssueMessage = knownIssue?.Message ?? string.Empty
                   let owner = knownIssue?.Owner ?? string.Empty
                   let testCaseData = new LanguageTestData
                   {
                       Version = version,
                       IsKnownIssue = isKnownIssue,
                       KnownIssueMessage = knownIssueMessage,
                       DocsLink = docsLink,
                       FileName = fileName,
                       DllPath = runSettings.DllPath
                   }
                   where !(isKnownIssue ^ runSettings.KnownFailuresRequested) // select known issues if requested
                   select new TestCaseData(testCaseData).SetName(testName).SetProperty("Owner", owner);
        }
    }
}
