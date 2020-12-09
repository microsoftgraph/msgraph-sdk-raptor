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
    // Owner is used to categorize known test failures, so that we can redirect issues faster
    public record KnownIssue (string Owner, string Message);

    public static class KnownIssues
    {
        #region SDK issues

        private const string FeatureNotSupported = "Range composable functions are not supported by SDK\r\n"
            + "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/490";
        private const string SearchHeaderIsNotSupported = "Search header is not supported by the SDK";
        private const string CountIsNotSupported = "OData $count is not supported by the SDK at the moment";
        private const string MissingContentProperty = "IReportRootGetM365AppPlatformUserCountsRequestBuilder is missing Content property";
        private const string PutAsyncIsNotSupported = "PutAsync methods are not auto generated.\r\n"
            + "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/844";

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
        private const string TestGeneration = nameof(TestGeneration);
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
        /// Returns a mapping of issues of which the source comes from service/documentation/metadata and are common accross langauges
        /// </summary>
        /// <param name="language">language to generate the exception from</param>
        /// <returns>mapping of issues of which the source comes from service/documentation/metadata and are common accross langauges</returns>
        public static Dictionary<string, KnownIssue> GetCommonIssues(Languages language)
        {
            var lng = language.AsString();
            return new Dictionary<string, KnownIssue>()
            {
                { $"convert-team-from-group-{lng}-V1-compiles", new KnownIssue(HTTP, "isFavoriteByDefault is only available in Beta. https://github.com/microsoftgraph/microsoft-graph-docs/issues/10145") },
                { $"convert-team-from-non-standard2-{lng}-V1-compiles", new KnownIssue(HTTP, "isFavoriteByDefault is only available in Beta. https://github.com/microsoftgraph/microsoft-graph-docs/issues/10145") },
                { $"call-transfer-{lng}-V1-compiles", new KnownIssue(Metadata, "v1 metadata doesn't have endpointType for invitationParticipantInfo") },
                { $"call-updatemetadata-{lng}-Beta-compiles", new KnownIssue(Metadata, "updateMetadata doesn't exist in metadata") },
                { $"create-acceptedsender-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "acceptedSender")) },
                { $"create-acceptedsender-{lng}-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"create-allowedgroup-from-printers-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("printerShare", "allowedGroups"))},
                { $"create-alloweduser-from-printers-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("printerShare", "allowedUsers"))},
                { $"create-certificatebasedauthconfiguration-from-certificatebasedauthconfiguration-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
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
                { $"create-tablecolumn-from-table-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Id should be string not int") },
                { $"create-tablecolumn-from-table-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Id should be string not int") },
                { $"create-team-post-full-payload-{lng}-Beta-compiles", new KnownIssue(HTTP, "name should be displayName on teamsTab objects") },
                { $"create-tokenlifetimepolicy-from-application-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"delete-acceptedsenders-from-group-{lng}-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "acceptedSender")) },
                { $"delete-alloweduser-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("printer", "allowedUsers")) },
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
                { $"list-conversation-members-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + "Me doesn't have \"Chats\". \"Chats\" is a high level EntitySet.") },
                { $"nameditem-range-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"participant-configuremixer-{lng}-Beta-compiles", new KnownIssue(Metadata, "ConfigureMixer doesn't exist in metadata") },
                { $"printer-getcapabilities-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"remove-group-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"remove-rejectedsender-from-group-{lng}-V1-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"remove-user-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"removeonpremisesagentfromanonpremisesagentgroup-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"securescorecontrolprofiles-update-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },
                { $"shift-put-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"table-databodyrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"table-headerrowrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"table-totalrowrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-databodyrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-headerrowrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-range-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablecolumn-totalrowrange-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"tablerow-range-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"unfollow-item-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(DELETE, POST)) },
                { $"update-activitybasedtimeoutpolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ActivityBasedTimeoutPolicy", "Type")) },
                { $"update-activitybasedtimeoutpolicy-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ActivityBasedTimeoutPolicy", "Type")) },
                { $"update-claimsmappingpolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ClaimsMappingPolicy", "Type")) },
                { $"update-claimsmappingpolicy-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ClaimsMappingPolicy", "Type")) },
                { $"update-homerealmdiscoverypolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("HomeRealmDiscoveryPolicy", "Type")) },
                { $"update-homerealmdiscoverypolicy-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("HomeRealmDiscoveryPolicy", "Type")) },
                { $"update-openidconnectprovider-{lng}-Beta-compiles", new KnownIssue(HTTP, "OpenIdConnectProvider should be specified") },
                { $"update-room-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Capacity should be int, isWheelchairAccessible should be renamed as isWheelChairAccessible") },
                { $"update-room-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Capacity should be int, isWheelchairAccessible should be renamed as isWheelChairAccessible") },
                { $"update-teamsapp-{lng}-V1-compiles", new KnownIssue(Metadata, $"teamsApp needs hasStream=true. In addition to that, we need these fixed: {Environment.NewLine}https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/160 {Environment.NewLine}https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/336") },
                { $"update-tokenissuancepolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenIssuancePolicy", "Type")) },
                { $"update-tokenissuancepolicy-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenIssuancePolicy", "Type")) },
                { $"update-tokenlifetimepolicy-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenLifetimePolicy", "Type")) },
                { $"update-tokenlifetimepolicy-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("TokenLifetimePolicy", "Type")) },
                { $"update-workforceintegration-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": workforceintegration id is needed in the url.") },
                { $"update-workforceintegration-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": workforceintegration id is needed in the url.") },
                { $"worksheet-range-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(POST, GET)) },
                { $"get-channel-messages-delta-1-{lng}-V1-compiles", new KnownIssue(Metadata, "Delta function is not declared") },
                { $"post-privilegedroleassignmentrequest-{lng}-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { $"create-b2cuserflow-from-b2cuserflows-identityprovider-{lng}-Beta-compiles", new KnownIssue(SnippetGeneration, "Snippet Generation needs casting support for *CollectionWithReferencesPage. See details at: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/327") },
                {$"update-b2xuserflows-identityprovider-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                {$"update-b2cuserflows-identityprovider-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                {$"create-connector-from-connectorgroup-{lng}-Beta-compiles", new KnownIssue(SDK, "Missing method") },
                {$"get-document-value-{lng}-Beta-compiles", new KnownIssue(Metadata, "HasStream missing") },
            };
        }

        /// <summary>
        /// Gets known issues
        /// </summary>
        /// <param name="versionEnum">version to get the known issues for</param>
        /// <returns>A mapping of test names into known CSharp issues</returns>
        public static Dictionary<string, KnownIssue> GetCSharpIssues(Versions versionEnum)
        {
            var version = versionEnum == Versions.V1 ? "V1" : "Beta";
            return new Dictionary<string, KnownIssue>()
            {
                { "call-transfer-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationAdditionalData) },
                { "create-b2xuserflow-from-b2xuserflows-identityproviders-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "Snippet Generation needs casting support for *CollectionWithReferencesPage. See details at: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/327") },
                { "create-externalitem-from-connections-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },
                { "create-rangeborder-from-rangeformat-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "create-rangeborder-from-rangeformat-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "create-schema-from-connection-async-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },
                { "follow-site-csharp-Beta-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "follow-site-csharp-V1-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "get-android-count-csharp-V1-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-borders-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-borders-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-count-group-only-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-count-group-only-csharp-V1-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-count-only-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-count-only-csharp-V1-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-count-user-only-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-count-user-only-csharp-V1-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-formatprotection-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-formatprotection-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "get-group-transitivemembers-count-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-group-transitivemembers-count-csharp-V1-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-opentypeextension-3-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "get-opentypeextension-3-csharp-V1-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                { "get-phone-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-phone-count-csharp-V1-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-pr-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-pr-count-csharp-V1-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
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
                { "get-team-count-csharp-V1-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-tier-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-tier-count-csharp-V1-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-user-memberof-count-only-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-user-memberof-count-only-csharp-V1-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                { "get-video-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-wa-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-wa-count-csharp-V1-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-web-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-web-count-csharp-V1-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
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
                { "team-put-schedule-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },
                { "unfollow-site-csharp-Beta-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "unfollow-site-csharp-V1-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "update-formatprotection-csharp-Beta-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-formatprotection-csharp-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "update-organizationalbrandingproperties-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
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
                {$"timeoff-put-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                {$"timeoffreason-put-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "post-reply-csharp-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Odata.Type for concrete Attachment type should be added") },
                { "post-reply-csharp-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": Odata.Type for concrete Attachment type should be added") },
                {$"schedule-put-schedulinggroups-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "shift-get-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-openshift-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-synchronizationtemplate-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-phoneauthenticationmethod-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-trustframeworkkeyset-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-synchronizationschema-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
            };
        }
        /// <summary>
        /// Gets known issues
        /// </summary>
        /// <param name="versionEnum">version to get the known issues for</param>
        /// <returns>A mapping of test names into known Java issues</returns>
        public static Dictionary<string, KnownIssue> GetJavaIssues(Versions versionEnum)
        {
            var version = versionEnum == Versions.V1 ? "V1" : "Beta";
            return new Dictionary<string, KnownIssue>()
            {
                { "range-cell-java-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-usedrange-valuesonly-java-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsabove-nocount-java-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsbelow-nocount-java-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-merge-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-lastrow-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrange-rowsbelow-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"get-rows-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-entirecolumn-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrangeview-range-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-delete-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-lastcell-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-unmerge-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-entirerow-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrange-columnsbefore-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-insert-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-clear-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-usedrange-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-column-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-lastcolumn-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrange-columnsafter-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrange-rowsabove-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"unfollow-site-java-{version}-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                {$"follow-site-java-{version}-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                {$"workbookrange-visibleview-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"update-page-java-{version}-compiles", new KnownIssue(SnippetGeneration, "See issue: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/288") },
                {$"get-singlevaluelegacyextendedproperty-1-java-{version}-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },
                {$"get-rooms-in-roomlist-java-{version}-compiles", new KnownIssue(SDK, "SDK doesn't generate type segment in OData URL. https://microsoftgraph.visualstudio.com/Graph%20Developer%20Experiences/_workitems/edit/4997") },
                {$"get-opentypeextension-3-java-{version}-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationFlattens) },

                {$"get-securescore-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {$"get-securescorecontrolprofile-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {$"get-securescorecontrolprofiles-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {$"get-securescores-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {"get-alert-java-V1-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {"get-alerts-java-V1-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {"update-alert-java-V1-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },

                {$"group-getmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"group-getmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"orgcontact-getmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"orgcontact-getmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"serviceprincipal-getmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"serviceprincipal-getmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"user-getmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"user-getmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"device-checkmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"group-checkmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"group-checkmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"orgcontact-checkmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"serviceprincipal-checkmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"serviceprincipal-checkmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"user-checkmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"user-checkmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"offershiftrequest-approve-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"offershiftrequest-decline-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"swapshiftchangerequest-approve-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"swapshiftchangerequest-decline-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"timeoffrequest-approve-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"timeoffrequest-decline-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"get-group-transitivemembers-count-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                {$"get-user-memberof-count-only-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                {"directoryobject-checkmembergroups-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {"directoryobject-getmembergroups-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {"directoryobject-getmemberobjects-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {"phoneauthenticationmethod-disablesmssignin-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                {"phoneauthenticationmethod-enablesmssignin-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                {"passwordauthenticationmethod-resetpassword-systemgenerated-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                {"passwordauthenticationmethod-resetpassword-adminprovided-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                {"user-upgrade-teamsapp-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                {"printjob-redirect-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },


                {$"get-deleteditems-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/361") },
                {$"get-all-roomlists-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/361") },
                {$"get-all-rooms-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/361") },
                {$"get-pr-count-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/361") },
                {$"get-tier-count-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/361") },
                {$"get-count-group-only-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/361") },
                {$"get-count-only-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/361") },
                {$"get-count-user-only-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/361") },
                
                {"update-accesspackageassignmentpolicy-java-Beta-compiles", new KnownIssue(SDK, "Missing property") },
                {"reportroot-getcredentialusagesummary-java-Beta-compiles", new KnownIssue(SDK, "Missing method") },

                {$"create-list-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Duplicated variable name") },

                {$"create-listitem-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Should be in additional data manager") },
                {$"update-listitem-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Should be in additional data manager") },
                {$"update-plannerassignedtotaskboardtaskformat-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Should be in additional data manager") },
                {$"update-plannerplandetails-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Should be in additional data manager") },

                {$"create-or-get-onlinemeeting-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Conflicting Graph and Java type") },
                {$"schedule-share-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Conflicting Graph and Java type") },

                {$"get-one-thumbnail-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Issue with Size argument") },
                {$"get-thumbnail-content-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Issue with Size argument") },

                {$"user-supportedtimezones-iana-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing quotes around query string parameter argument?") },

                {"alert-updatealerts-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Enums are not generated properly") },
                
                {$"get-channel-messages-delta-2-java-{version}-compiles", new KnownIssue(Metadata, "Delta function is not declared") },
                {$"get-channel-messages-delta-3-java-{version}-compiles", new KnownIssue(Metadata, "Delta function is not declared") },
                {"shift-put-java-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },

                {$"upload-via-put-id-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for content: https://github.com/microsoftgraph/microsoft-graph-explorer-api/issues/371") },

                {"create-printer-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Parameters with null values are not accounted for as action parameters") },
                {"call-answer-app-hosted-media-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Parameters with null values are not accounted for as action parameters") },
                {"call-answer-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Parameters with null values are not accounted for as action parameters") },

                {"get-group-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                {"get-set-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                {"update-set-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                {"update-term-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                {"get-store-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                {"update-store-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                {"get-term-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                {"get-relation-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                {"create-term-from--java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },

                {"create-accesspackageresourcerequest-from-accesspackageresourcerequests-java-Beta-compiles", new KnownIssue(SDK, SnippetGenerationRequestObjectDisambiguation) },
                {"governanceroleassignmentrequest-post-java-Beta-compiles", new KnownIssue(SDK, SnippetGenerationRequestObjectDisambiguation) },
                {"create-accesspackageassignmentrequest-from-accesspackageassignmentrequests-java-Beta-compiles", new KnownIssue(SDK, SnippetGenerationRequestObjectDisambiguation) },
                {"get-accesspackageassignmentrequest-java-Beta-compiles", new KnownIssue(SDK, SnippetGenerationRequestObjectDisambiguation) },

                {"update-educationpointsoutcome-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Lossy conversion") },
                {"update-printer-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Lossy conversion") },
                {"update-connector-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Lossy conversion") },
                {"educationsubmission-return-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Reserved keyword usage") },
                {"tablecolumncollection-add-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Tries to instantiate a primite??") },
                {"group-evaluatedynamicmembership-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Double Quotes not escaped") },
                {"get-joinedteams-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Wrong page type in use") },
                {"create-educationrubric-from-educationuser-java-Beta-compiles", new KnownIssue(TestGeneration, "Code truncated???") },

                {"securescorecontrolprofiles-update-java-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },
            };
        }
        /// <summary>
        /// Gets known issues by language
        /// </summary>
        /// <param name="language">language to get the issues for</param>
        /// <param name="version">version to get the issues for</param>
        /// <returns>A mapping of test names into known issues</returns>
        public static Dictionary<string, KnownIssue> GetIssues(Languages language, Versions version)
        {
            return (language switch
            {
                Languages.CSharp => GetCSharpIssues(version),
                Languages.Java => GetJavaIssues(version),
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
            var languageName = language.AsString();
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

            var language = runSettings.Language;
            var version = runSettings.Version;
            var documentationLinks = GetDocumentationLinks(version, language);
            var knownIssues = KnownIssues.GetIssues(language, version);
            var snippetFileNames = documentationLinks.Keys.ToList();
            return from fileName in snippetFileNames                                            // e.g. application-addpassword-csharp-snippets.md
                   let arbitraryDllPostfix = runSettings.DllPath == null || runSettings.Language != Languages.CSharp ? string.Empty : "arbitraryDll-"
                   let testNamePostfix = arbitraryDllPostfix + version.ToString() + "-compiles" // e.g. Beta-compiles or arbitraryDll-Beta-compiles
                   let testName = fileName.Replace("snippets.md", testNamePostfix)              // e.g. application-addpassword-csharp-Beta-compiles
                   let docsLink = documentationLinks[fileName]
                   let knownIssueLookupKey = testName.Replace("arbitraryDll-", string.Empty)
                   let isKnownIssue = knownIssues.ContainsKey(knownIssueLookupKey)
                   let knownIssue = isKnownIssue ? knownIssues[knownIssueLookupKey] : null
                   let knownIssueMessage = knownIssue?.Message ?? string.Empty
                   let owner = knownIssue?.Owner ?? string.Empty
                   let testCaseData = new LanguageTestData(
                       version,
                       isKnownIssue,
                       knownIssueMessage,
                       docsLink,
                       fileName,
                       runSettings.DllPath,
                       runSettings.JavaCoreVersion,
                       runSettings.JavaLibVersion,
                       runSettings.JavaPreviewLibPath)
                   where !(isKnownIssue ^ runSettings.KnownFailuresRequested) // select known issues if requested
                   select new TestCaseData(testCaseData).SetName(testName).SetProperty("Owner", owner);
        }
    }
}
