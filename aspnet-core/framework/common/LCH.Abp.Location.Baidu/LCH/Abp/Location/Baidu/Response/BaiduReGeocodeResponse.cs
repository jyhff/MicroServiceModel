using LCH.Abp.Location.Baidu.Model;

namespace LCH.Abp.Location.Baidu.Response;

public class BaiduReGeocodeResponse : BaiduLocationResponse
{
    public BaiduReGeocode Result { get; set; } = new BaiduReGeocode();
}
