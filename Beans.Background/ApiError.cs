using System;

namespace Beans.Background;
public class ApiError
{
    public int Code { get; set; }
    public string Message { get; set; }
    public string[] Messages { get; set; }

    public ApiError()
    {
        Code = 0;
        Message = string.Empty;
        Messages = Array.Empty<string>();
    }
}
