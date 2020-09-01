using System;
using Newtonsoft.Json;

namespace MatsueNet.Structures
{
    public struct DonaldTagGetJson : IEquatable<DonaldTagGetJson>
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("_embedded")]
        public DonaldEmbedTag Embed { get; set; }

        public bool Equals(DonaldTagGetJson other)
        {
            throw new NotImplementedException();
        }
    }

    public struct DonaldEmbedTag : IEquatable<DonaldEmbedTag>
    {
        [JsonProperty("tag")]
        public DonaldTagJson[] Tags { get; set; }

        public bool Equals(DonaldEmbedTag other)
        {
            throw new NotImplementedException();
        }
    }

    public struct DonaldEmbedRandomTag : IEquatable<DonaldEmbedRandomTag>
    {
        [JsonProperty("author")]
        public AuthorJson[] Author { get; set; }

        [JsonProperty("source")]
        public SourceJson[] Source { get; set; }

        public bool Equals(DonaldEmbedRandomTag other)
        {
            throw new NotImplementedException();
        }
    }

    public struct DonaldTagJson : IEquatable<DonaldTagJson>
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("_links")]
        public LinkJson Link { get; set; }

        public bool Equals(DonaldTagJson other)
        {
            throw new NotImplementedException();
        }
    }

    public struct LinkJson : IEquatable<LinkJson>
    {
        [JsonProperty("self")]
        public UriJson Uri { get; set; }

        public bool Equals(LinkJson other)
        {
            throw new NotImplementedException();
        }
    }

    public struct UriJson : IEquatable<UriJson>
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        public bool Equals(UriJson other)
        {
            throw new NotImplementedException();
        }
    }

    public struct RandomQuoteJson : IEquatable<RandomQuoteJson>
    {
        [JsonProperty("appeared_at")]
        public string AppearedAt { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("quote_id")]
        public string QuoteId { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("_embedded")]
        public DonaldEmbedRandomTag Embed { get; set; }

        [JsonProperty("_links")]
        public LinkJson Links { get; set; }

        public bool Equals(RandomQuoteJson other)
        {
            throw new NotImplementedException();
        }
    }

    public struct AuthorJson : IEquatable<AuthorJson>
    {
        [JsonProperty("author_id")]
        public string AuthorId { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("update_at")]
        public string UpdateAt { get; set; }

        [JsonProperty("_links")]
        public LinkJson Link { get; set; }

        public bool Equals(AuthorJson other)
        {
            throw new NotImplementedException();
        }
    }

    public struct SourceJson : IEquatable<SourceJson>
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("quote_source_id")]
        public string QuoteSourceId { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; }

        [JsonProperty("update_at")]
        public string UpdateAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("_links")]
        public LinkJson Link { get; set; }

        public bool Equals(SourceJson other)
        {
            throw new NotImplementedException();
        }
    }
}