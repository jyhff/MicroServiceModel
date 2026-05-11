using LCH.Abp.Location.Baidu.Model;

namespace LCH.Abp.Location.Baidu.Response;

public class BaiduGeocodeResponse : BaiduLocationResponse
{
    public BaiduGeocode Result { get; set; } = new BaiduGeocode();
}
