using LCH.Abp.Location.Baidu.Model;

namespace LCH.Abp.Location.Baidu.Response;

public class BaiduIpGeocodeResponse : BaiduLocationResponse
{
    public string Address { get; set; }

    public Content Content { get; set; } = new Content();
}
