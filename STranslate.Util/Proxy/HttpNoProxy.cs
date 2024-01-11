using System;
using System.Net;

namespace STranslate.Util.Proxy;

internal sealed class HttpNoProxy : IWebProxy
{
    public ICredentials? Credentials { get; set; }

    public Uri? GetProxy(Uri destination) => null;

    public bool IsBypassed(Uri host) => true;
}
