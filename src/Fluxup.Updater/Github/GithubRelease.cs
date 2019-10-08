using System;
using Newtonsoft.Json;

namespace Fluxup.Updater.Github
{
    internal class GithubRelease
    {
        [JsonProperty("url")]
        public Uri Url { get; private set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; private set; }

        [JsonProperty("assets_url")]
        public Uri AssetsUrl { get; private set; }

        [JsonProperty("upload_url")]
        public string UploadUrl { get; private set; }

        [JsonProperty("tarball_url")]
        public Uri TarballUrl { get; private set; }

        [JsonProperty("zipball_url")]
        public Uri ZipballUrl { get; private set; }

        [JsonProperty("id")]
        public long Id { get; private set; }

        [JsonProperty("node_id")]
        public string NodeId { get; private set; }

        [JsonProperty("tag_name")]
        public string TagName { get; private set; }

        [JsonProperty("target_commitish")]
        public string TargetCommitish { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("body")]
        public string Body { get; private set; }

        [JsonProperty("draft")]
        public bool Draft { get; private set; }

        [JsonProperty("prerelease")]
        public bool Prerelease { get; private set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; private set; }

        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; private set; }

        [JsonProperty("author")]
        public Author Author { get; private set; }

        [JsonProperty("assets")]
        public Asset[] Assets { get; private set; }
    }

    internal class Asset
    {
        [JsonProperty("url")]
        public Uri Url { get; private set; }

        [JsonProperty("browser_download_url")]
        public Uri BrowserDownloadUrl { get; private set; }

        [JsonProperty("id")]
        public long Id { get; private set; }

        [JsonProperty("node_id")]
        public string NodeId { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("label")]
        public string Label { get; private set; }

        [JsonProperty("state")]
        public string State { get; private set; }

        [JsonProperty("content_type")]
        public string ContentType { get; private set; }

        [JsonProperty("size")]
        public long Size { get; private set; }

        [JsonProperty("download_count")]
        public long DownloadCount { get; private set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; private set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; private set; }

        [JsonProperty("uploader")]
        public Author Uploader { get; private set; }
    }

    internal class Author
    {
        [JsonProperty("login")]
        public string Login { get; private set; }

        [JsonProperty("id")]
        public long Id { get; private set; }

        [JsonProperty("node_id")]
        public string NodeId { get; private set; }

        [JsonProperty("avatar_url")]
        public Uri AvatarUrl { get; private set; }

        [JsonProperty("gravatar_id")]
        public string GravatarId { get; private set; }

        [JsonProperty("url")]
        public Uri Url { get; private set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; private set; }

        [JsonProperty("followers_url")]
        public Uri FollowersUrl { get; private set; }

        [JsonProperty("following_url")]
        public string FollowingUrl { get; private set; }

        [JsonProperty("gists_url")]
        public string GistsUrl { get; private set; }

        [JsonProperty("starred_url")]
        public string StarredUrl { get; private set; }

        [JsonProperty("subscriptions_url")]
        public Uri SubscriptionsUrl { get; private set; }

        [JsonProperty("organizations_url")]
        public Uri OrganizationsUrl { get; private set; }

        [JsonProperty("repos_url")]
        public Uri ReposUrl { get; private set; }

        [JsonProperty("events_url")]
        public string EventsUrl { get; private set; }

        [JsonProperty("received_events_url")]
        public Uri ReceivedEventsUrl { get; private set; }

        [JsonProperty("type")]
        public string Type { get; private set; }

        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; private set; }
    }
}