using System;

namespace LCH.Abp.Identity.Session;
public interface ISessionInfoProvider
{
    string SessionId { get; }

    IDisposable Change(string sessionId);
}
