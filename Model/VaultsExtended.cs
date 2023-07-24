using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AccountExtended
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("hiddenOnUI")]
    public bool HiddenOnUI { get; set; }

    [JsonProperty("customerRefId")]
    public string CustomerRefId { get; set; }

    [JsonProperty("autoFuel")]
    public bool AutoFuel { get; set; }

    public bool? IsVisibleToSubuser { get; set; }

    [JsonProperty("assets")]
    public Asset[] Assets { get; set; }
}

public class AssetExtended
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("total")]
    public string Total { get; set; }

    [JsonProperty("balance")]
    public string Balance { get; set; }

    [JsonProperty("lockedAmount")]
    public string LockedAmount { get; set; }

    [JsonProperty("available")]
    public string Available { get; set; }

    [JsonProperty("pending")]
    public string Pending { get; set; }

    [JsonProperty("frozen")]
    public string Frozen { get; set; }

    [JsonProperty("staked")]
    public string Staked { get; set; }

    // Additional properties if any
}

public class VaultsExtended
{
    [JsonProperty("accounts")]
    public AccountExtended[] Accounts { get; set; }
}
