// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;

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
        private const string StreamRequestDoesNotSupportDelete = "Stream requests only support PUT and GET.";
        private const string DeleteAsyncIsNotSupportedForReferences = "DeleteAsync is not supported for reference collections\r\n"
            + "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/471";
        private const string TypeCastIsNotSupported = "Type cast operation is not supported in SDK.";

        private const string ComplexTypeNavigationProperties = "Complex Type navigation properties are not generated\r\n"
            + "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/1003";

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
        private const string EventActionsShouldNotBeReordered = "There is a reorder rule in XSLT. It should be removed" +
            " See https://github.com/microsoftgraph/msgraph-metadata/pull/64";
        #endregion

        #region Snipppet Generation Issues
        private const string SnippetGenerationCreateAsyncSupport = "Snippet generation doesn't use CreateAsync" +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/301";
        private const string SnippetGenerationRequestObjectDisambiguation = "Snippet generation should rename objects that end with Request to end with RequestObject" +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/298";
        private const string StructuralPropertiesAreNotHandled = "We don't generate request builders for URL navigation to structural properties." +
            " We should build a custom request with URL as this is not supported in SDK." +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/485";
        private const string SameBlockNames = "Same block names indeterministic snippet generation" +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/463";
        private const string NamespaceOdataTypeAnnotationsWithoutHashSymbol = "We do not support namespacing when odata.type annotations are not prepended with hash symbol." +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/580";
        private const string DateTimeOffsetHandlingInUrls = "Dates supplied via GET request urls are not parsed to dates\r\n"
            + "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/612";
        private const string IdentitySetAndIdentityShouldNestAdditionalData = "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/613";
        #endregion

        #region Needs analysis
        private const string NeedsAnalysisText = "This is a consistently failing test, the root cause is not yet identified";
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
        private const string NeedsAnalysis = nameof(NeedsAnalysis);
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
        public static Dictionary<string, KnownIssue> GetCommonIssues(Languages language, Versions versionEnum)
        {
            var version = versionEnum.ToString();
            var lng = language.AsString();
            return new Dictionary<string, KnownIssue>()
            {
                { $"call-updatemetadata-{lng}-Beta-compiles", new KnownIssue(Metadata, "updateMetadata doesn't exist in metadata") },
                { $"create-certificatebasedauthconfiguration-from-certificatebasedauthconfiguration-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"create-directoryobject-from-featurerolloutpolicy-{lng}-{version}-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo"))},
                { $"create-directoryobject-from-featurerolloutpolicy-policies-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo"))},
                { $"create-directoryobject-from-orgcontact-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"create-educationrubric-from-educationassignment-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("educationAssignment", "rubric"))},
                { $"create-educationschool-from-educationroot-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("EducationSchool", "Status")) },
                { $"create-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"create-homerealmdiscoverypolicy-from-serviceprincipal-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"create-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"create-onpremisesagentgroup-from-publishedresource-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"create-reference-attachment-with-post-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ReferenceAttachment", "SourceUrl, ProviderType, Permission and IsFolder")) },
                { $"create-tokenlifetimepolicy-from-application-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"delete-directoryobject-from-featurerolloutpolicy-{lng}-{version}-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-directoryobject-from-featurerolloutpolicy-policies-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-educationrubric-from-educationassignment-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("educationAssignment", "rubric"))},
                { $"delete-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"delete-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"delete-publishedresource-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"directoryobject-delta-{lng}-Beta-compiles", new KnownIssue(Metadata, "Delta is not defined on directoryObject, but on user and group") },
                { $"event-accept-{lng}-Beta-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-decline-{lng}-Beta-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-tentativelyaccept-{lng}-Beta-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-accept-{lng}-V1-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-decline-{lng}-V1-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-tentativelyaccept-{lng}-V1-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"get-endpoint-{lng}-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { $"get-endpoints-{lng}-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { $"get-identityriskevent-{lng}-Beta-compiles", new KnownIssue(HTTP, IdentityRiskEvents) },
                { $"get-identityriskevents-{lng}-Beta-compiles", new KnownIssue(HTTP, IdentityRiskEvents) },
                { $"list-conversation-members-1-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + "Me doesn't have \"Chats\". \"Chats\" is a high level EntitySet.") },
                { $"participant-configuremixer-{lng}-Beta-compiles", new KnownIssue(Metadata, "ConfigureMixer doesn't exist in metadata") },
                { $"remove-group-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"remove-user-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"removeonpremisesagentfromanonpremisesagentgroup-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"securescorecontrolprofiles-update-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },
                { $"shift-put-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"unfollow-item-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(DELETE, POST)) },
                { $"update-openidconnectprovider-{lng}-Beta-compiles", new KnownIssue(HTTP, "OpenIdConnectProvider should be specified") },
                { $"update-teamsapp-{lng}-V1-compiles", new KnownIssue(Metadata, $"teamsApp needs hasStream=true. In addition to that, we need these fixed: {Environment.NewLine}https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/160 {Environment.NewLine}https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/336") },
                { $"create-b2cuserflow-from-b2cuserflows-identityprovider-{lng}-Beta-compiles", new KnownIssue(SnippetGeneration, "Snippet Generation needs casting support for *CollectionWithReferencesPage. See details at: https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/327") },
                {$"update-b2xuserflows-identityprovider-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                {$"update-b2cuserflows-identityprovider-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                {$"create-connector-from-connectorgroup-{lng}-Beta-compiles", new KnownIssue(SDK, "Missing method") },
                {$"shift-get-{lng}-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/11521") },
                {$"shift-get-{lng}-V1-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/11521") },
            };
        }

        /// <summary>
        /// Gets known issues
        /// </summary>
        /// <param name="versionEnum">version to get the known issues for</param>
        /// <returns>A mapping of test names into known CSharp issues</returns>
        public static Dictionary<string, KnownIssue> GetCSharpIssues(Versions versionEnum)
        {
            var version = versionEnum.ToString();
            return new Dictionary<string, KnownIssue>()
            {
                { "create-b2xuserflow-from-b2xuserflows-identityproviders-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "Snippet Generation needs casting support for *CollectionWithReferencesPage. See details at: https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/327") },

                { "create-schema-from-connection-async-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },

                { "update-accessreviewscheduledefinition-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "Multiline string is not escaping quotes. https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/484") },

                { "get-endpoints-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SameBlockNames) },

                {$"unfollow-site-csharp-{version}-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                {$"follow-site-csharp-{version}-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "get-android-count-csharp-V1-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-count-group-only-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-count-only-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-count-user-only-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-group-transitivemembers-count-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-user-memberof-count-only-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },

                {$"get-phone-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-pr-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-team-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-tier-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-video-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-wa-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-web-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },

                {$"get-rooms-in-roomlist-csharp-{version}-compiles", new KnownIssue(SDK, "SDK doesn't generate type segment in OData URL. https://microsoftgraph.visualstudio.com/Graph%20Developer%20Experiences/_workitems/edit/4997") },

                { "put-privilegedrolesettings-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "put-regionalandlanguagesettings-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "team-put-schedule-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "update-organizationalbrandingproperties-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                {$"get-organizationalbrandingproperties-1-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                {$"update-organizationalbrandingproperties-4-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                {$"update-organizationalbrandingproperties-8-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                {$"timeoff-put-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                {$"timeoffreason-put-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                {$"schedule-put-schedulinggroups-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "update-emailauthenticationmethod-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "update-accesspackageassignmentpolicy-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "create-externalitem-from-connections-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "create-userflowlanguageconfiguration-from--1-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "create-userflowlanguageconfiguration-from--2-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "shift-get-3-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                {$"update-adminconsentrequestpolicy-csharp-{version}-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported) },
                { "update-accessreviewscheduledefinition-csharp-V1-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported)},

                { "reportroot-getm365appplatformusercounts-csv-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appplatformusercounts-json-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercoundetail-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercountdetail-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercounts-csv-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercounts-json-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },

                { "update-openshift-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-synchronizationtemplate-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-phoneauthenticationmethod-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-trustframeworkkeyset-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-synchronizationschema-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },

                { "get-chat-csharp-V1-compiles", new KnownIssue(HTTP, "User chats are not available in V1 yet: https://github.com/microsoftgraph/microsoft-graph-docs/issues/12162") },
                { "get-featurerolloutpolicy-expand-appliesto-csharp-V1-compiles", new KnownIssue(HTTP, "Directory singleton doesn't have featureRolloutPolicies in V1: https://github.com/microsoftgraph/microsoft-graph-docs/issues/12162") },

                { "put-b2cuserflows-apiconnectorconfiguration-postfederationsignup-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, StructuralPropertiesAreNotHandled) },
                { "put-b2xuserflows-apiconnectorconfiguration-postfederationsignup-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, StructuralPropertiesAreNotHandled) },
                { "put-b2xuserflows-apiconnectorconfiguration-postattributecollection-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, StructuralPropertiesAreNotHandled) },
                { "put-b2cuserflows-apiconnectorconfiguration-postattributecollection-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, StructuralPropertiesAreNotHandled) },

                { "create-accesspackageassignmentrequest-from-accesspackageassignmentrequests-csharp-Beta-compiles", new KnownIssue(HttpSnippetWrong, "Need @odata.type for abstract type in JSON. https://github.com/microsoftgraph/microsoft-graph-docs/issues/11770") },

                { "delete-userflowlanguagepage-csharp-Beta-compiles", new KnownIssue(SDK, StreamRequestDoesNotSupportDelete) },
                { "team-put-schedule-2-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported)},
                { "timecard-replace-csharp-Beta-compiles", new KnownIssue(SDK, PutAsyncIsNotSupported)},
                { "get-transitivereports-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported)},
                { "get-transitivereports-user-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported)},
                { "caseexportoperation-getdownloadurl-csharp-Beta-compiles", new KnownIssue(SDK, TypeCastIsNotSupported) },

                { "remove-rejectedsender-from-group-csharp-V1-compiles", new KnownIssue(SDK, DeleteAsyncIsNotSupportedForReferences) },
                { "delete-acceptedsenders-from-group-csharp-V1-compiles", new KnownIssue(SDK, DeleteAsyncIsNotSupportedForReferences) },
                { "put-b2xuserflows-apiconnectorconfiguration-postattributecollection-csharp-V1-compiles", new KnownIssue(SDK, ComplexTypeNavigationProperties) },
                { "put-b2xuserflows-apiconnectorconfiguration-postfederationsignup-csharp-V1-compiles", new KnownIssue(SDK, ComplexTypeNavigationProperties) },

                { "update-socialidentityprovider-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12780") },
                { "update-appleidentityprovider-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12780")},
                { "deploymentaudience-updateaudience-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12811")},
                { "update-unifiedrolemanagementpolicyrule-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12814")},
                { "list-administrativeunit-csharp-V1-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12770")},
                { "list-educationclass-csharp-V1-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12842")},
                { "create-accessreviewscheduledefinition-inactiveguests-m365-csharp-V1-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/13431")},
                { "create-educationsubmissionresource-from-educationsubmission-csharp-V1-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/13191")},

                { "create-deployment-from--csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol)},
                { "create-noncustodialdatasource-from--csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol) },
                { "update-deployment-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol)},
                { "update-deployment-1-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol)},
                { "update-deployment-2-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol)},

                { "reports-getuserarchivedprintjobs-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, DateTimeOffsetHandlingInUrls)},
                { "reports-getprinterarchivedprintjobs-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, DateTimeOffsetHandlingInUrls)},
                { "reports-getgrouparchivedprintjobs-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, DateTimeOffsetHandlingInUrls)},
                { "post-channelmessage-2-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-chatmessage-2-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-channelmessage-3-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-chatmessagereply-2-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},

                { "post-channelmessage-3-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-channelmessage-2-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-chatmessage-2-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-chatmessagereply-2-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "shift-put-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},

                { "create-educationrubric-from-educationassignment-csharp-V1-compiles", new KnownIssue(Metadata, "EducationRubric containsTarget should be False to use $ref")},
                { "delete-educationrubric-from-educationassignment-csharp-V1-compiles", new KnownIssue(Metadata, "EducationRubric containsTarget should be False to use $ref")},
                { "add-educationcategory-to-educationassignment-csharp-V1-compiles", new KnownIssue(Metadata, "EducationRubric containsTarget should be False to use $ref")},
                { "educationassignment-setupresourcesfolder-csharp-V1-compiles", new KnownIssue(Metadata, "EducationAssignmentSetUpResourcesFolder defined as odata action instead of function for 'PostAsync' generation")},
                { "educationsubmission-setupresourcesfolder-csharp-V1-compiles", new KnownIssue(Metadata, "EducationAssignmentSetUpResourcesFolder defined as odata action instead of function for 'PostAsync' generation")},
                { "educationsubmission-setupresourcesfolder-csharp-Beta-compiles", new KnownIssue(Metadata, "EducationAssignmentSetUpResourcesFolder defined as odata action instead of function for 'PostAsync' generation")},
                { "educationassignment-setupresourcesfolder-csharp-Beta-compiles", new KnownIssue(Metadata, "EducationAssignmentSetUpResourcesFolder defined as odata action instead of function for 'PostAsync' generation")},


                { "appconsentrequest-filterbycurrentuser-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-accesspackageassignmentrequest-from-accesspackageassignmentrequests-2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-accessreviewscheduledefinition-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-datasource-from--1-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-datasource-from--2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-directoryobject-from-orgcontact-1-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-directoryobject-from-orgcontact-2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-externalgroup-from-connection-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-externalgroupmember-from--1-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-externalgroupmember-from--2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-externalgroupmember-from--3-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-legalhold-from--csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "directoryobject-delta-2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "recent-notebooks-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "update-externalitem-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "update-passwordlessmicrosoftauthenticatorauthenticationmethodconfiguration-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "userconsentrequest-filterbycurrentuser-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-connectorgroup-from-connector-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
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
                {$"update-page-java-{version}-compiles", new KnownIssue(SnippetGeneration, "See issue: https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/428") },
                {$"get-rooms-in-roomlist-java-{version}-compiles", new KnownIssue(SDK, "SDK doesn't generate type segment in OData URL. https://microsoftgraph.visualstudio.com/Graph%20Developer%20Experiences/_workitems/edit/4997") },

                {$"get-securescore-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {$"get-securescorecontrolprofile-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {$"get-securescorecontrolprofiles-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {$"get-securescores-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                { "get-alert-java-V1-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                { "get-alerts-java-V1-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                { "update-alert-java-V1-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },

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
                { "directoryobject-checkmembergroups-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                { "directoryobject-getmembergroups-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                { "directoryobject-getmemberobjects-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                { "phoneauthenticationmethod-disablesmssignin-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "phoneauthenticationmethod-enablesmssignin-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "passwordauthenticationmethod-resetpassword-systemgenerated-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "passwordauthenticationmethod-resetpassword-adminprovided-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "user-upgrade-teamsapp-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "printjob-redirect-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },


                {$"get-deleteditems-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-all-roomlists-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-all-rooms-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-pr-count-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-tier-count-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-count-group-only-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-count-only-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-count-user-only-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },

                { "update-accesspackageassignmentpolicy-java-Beta-compiles", new KnownIssue(SDK, "Missing property") },
                { "reportroot-getcredentialusagesummary-java-Beta-compiles", new KnownIssue(SDK, "Missing method") },

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

                { "alert-updatealerts-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Enums are not generated properly") },

                {$"get-channel-messages-delta-2-java-{version}-compiles", new KnownIssue(Metadata, "Delta function is not declared") },
                {$"get-channel-messages-delta-3-java-{version}-compiles", new KnownIssue(Metadata, "Delta function is not declared") },
                { "shift-put-java-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },

                {$"upload-via-put-id-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for content: https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/371") },

                { "create-printer-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Parameters with null values are not accounted for as action parameters") },
                { "call-answer-app-hosted-media-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Parameters with null values are not accounted for as action parameters") },
                { "call-answer-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Parameters with null values are not accounted for as action parameters") },

                { "get-group-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "get-set-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "update-set-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "update-term-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "get-store-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "update-store-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "get-term-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "get-relation-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "create-term-from--java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },

                { "create-accesspackageresourcerequest-from-accesspackageresourcerequests-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { "governanceroleassignmentrequest-post-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { "create-accesspackageassignmentrequest-from-accesspackageassignmentrequests-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { "get-accesspackageassignmentrequest-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { "post-privilegedroleassignmentrequest-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },

                { "update-educationpointsoutcome-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Lossy conversion") },
                { "update-printer-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Lossy conversion") },
                { "update-connector-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Lossy conversion") },
                { "educationsubmission-return-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Reserved keyword usage") },
                { "tablecolumncollection-add-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Tries to instantiate a primite??") },
                { "group-evaluatedynamicmembership-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Double Quotes not escaped") },
                { "get-joinedteams-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Wrong page type in use") },
                { "create-educationrubric-from-educationuser-java-Beta-compiles", new KnownIssue(TestGeneration, "Code truncated???") },

                { "securescorecontrolprofiles-update-java-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },

                { "create-acceptedsender-java-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-rejectedsender-java-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-document-value-java-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "remove-rejectedsender-from-group-java-V1-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "delete-acceptedsenders-from-group-java-V1-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { $"call-transfer-java-{version}-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },

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
            }).Union(GetCommonIssues(language, version)).ToDictionary(x => x.Key, x => x.Value);
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
            return from testData in GetLanguageTestData(runSettings)
                   where !(testData.IsKnownIssue ^ runSettings.KnownFailuresRequested) // select known issues if requested
                   select new TestCaseData(testData).SetName(testData.TestName).SetProperty("Owner", testData.Owner);
        }

        /// <summary>
        /// For each snippet file creates a test case which takes the file name and version as reference
        /// Test case name is also set to to unique name based on file name
        /// </summary>
        /// <param name="runSettings">Test run settings</param>
        /// <returns>
        /// TestCaseData to be consumed by C# execution tests
        /// </returns>
        public static IEnumerable<TestCaseData> GetExecutionTestData(RunSettings runSettings)
        {
            return from testData in GetLanguageTestData(runSettings)
                   let fullPath = Path.Join(GraphDocsDirectory.GetSnippetsDirectory(testData.Version, runSettings.Language), testData.FileName)
                   let fileContent = File.ReadAllText(fullPath)
                   let executionTestData = new ExecutionTestData(testData, fileContent)
                   where !testData.IsKnownIssue // select compiling tests
                   && fileContent.Contains("GetAsync()") // select only the get tests
                   select new TestCaseData(executionTestData).SetName(testData.TestName).SetProperty("Owner", testData.Owner);
        }

        private static IEnumerable<LanguageTestData> GetLanguageTestData(RunSettings runSettings)
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
                   select new LanguageTestData(
                           version,
                           isKnownIssue,
                           knownIssueMessage,
                           docsLink,
                           fileName,
                           runSettings.DllPath,
                           runSettings.JavaCoreVersion,
                           runSettings.JavaLibVersion,
                           runSettings.JavaPreviewLibPath,
                           testName,
                           owner);
        }
    }
}
