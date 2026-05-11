using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LCH.Abp.WeChat.Work.ExternalContact.Customers.Models;
/// <summary>
/// 规则组
/// </summary>
public class CustomerStrategy
{
    /// <summary>
    /// 规则组id
    /// </summary>
    [NotNull]
    [JsonProperty("strategy_id")]
    [JsonPropertyName("strategy_id")]
    public int StrategyId { get; set; }
}
