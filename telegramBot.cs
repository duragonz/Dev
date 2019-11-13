public class TelegramBot
{
    private static readonly string _baseUrl = "https://api.telegram.org/bot";
    private static readonly string _token = "xxxxxxxxx:xxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
    private static readonly string _chatId = "xxxxxxxx";
 
    /// <summary>
    /// 텔레그램봇에게 메시지를 보냅니다.
    /// </summary>
    /// <param name="text">보낼 메시지</param>
    /// <param name="errorMessage">오류 메시지</param>
    /// <returns>결과</returns>
    public static bool SendMessage(string text, out string errorMessage)
    {
        return SendMessage(_chatId, text, out errorMessage);
    }
    
    /// <summary>
    /// 텔레그램봇에게 메시지를 보냅니다.
    /// </summary>
    /// <param name="chatId">chat id</param>
    /// <param name="text">보낼 메시지</param>
    /// <param name="errorMessage">오류 메시지</param>
    /// <returns>결과</returns>
    public static bool SendMessage(string chatId, string text, out string errorMessage)
    {
        string url = string.Format("{0}{1}/sendMessage", _baseUrl, _token);
        
        HttpWebRequest req = WebRequest.Create(new Uri(url)) as HttpWebRequest;
        req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
        req.Timeout = 30 * 1000;        
        req.Method = "POST";
        req.ContentType = "application/json";
 
        string json = String.Format("{{\"chat_id\":\"{0}\", \"text\":\"{1}\"}}", chatId, EncodeJsonChars(text));
        byte[] data = UTF8Encoding.UTF8.GetBytes(json);
        req.ContentLength = data.Length;
        using (Stream stream = req.GetRequestStream())
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
 
        HttpWebResponse httpResponse = null;
        try
        {
            httpResponse = req.GetResponse() as HttpWebResponse;
            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                string responseData = null;
                using (Stream responseStream = httpResponse.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(responseStream, UTF8Encoding.UTF8))
                    {
                        responseData = reader.ReadToEnd();
                    }
                }
                
                if (0 < responseData.IndexOf("\"ok\":true"))
                {
                    errorMessage = String.Empty;
                    return true;
                }
                else
                {
                    errorMessage = String.Format("결과 json 파싱 오류 ({0})", responseData);
                    return false;
                }
            }
            else
            {
                errorMessage = String.Format("Http status: {0}", httpResponse.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
        finally
        {
            if (httpResponse != null)
                httpResponse.Close();
        }
    }
    
    private static string EncodeJsonChars(string text)
    {
        return text.Replace("\b", "\\\b")
            .Replace("\f", "\\\f")
            .Replace("\n", "\\\n")
            .Replace("\r", "\\\r")
            .Replace("\t", "\\\t")
            .Replace("\"", "\\\"")
            .Replace("\\", "\\\\");
    }
}
