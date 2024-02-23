using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace URLShortner.Model;

public class URLModel
{
    public string FullURL { get; set; }
    [Key]
    [JsonProperty(PropertyName = "HashURL")]
    public string HashURL {get; set;}
    public DateTime CreatedDate{get;set;}
    public long Clicked {get;set;}
}
