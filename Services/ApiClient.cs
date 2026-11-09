using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace RaceDay.Mvc.Services;
public class ApiClient {
 private readonly IHttpClientFactory _factory; private readonly IHttpContextAccessor _accessor;
 private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
 public ApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor){_factory=factory;_accessor=accessor;}
 private HttpClient Client(){var c=_factory.CreateClient("RaceDayApi"); var cookie=_accessor.HttpContext?.Session.GetString("ApiCookie"); if(!string.IsNullOrWhiteSpace(cookie)) c.DefaultRequestHeaders.TryAddWithoutValidation("Cookie",cookie); return c;}
 public async Task<(bool Ok,string Error)> Login(string email,string password){
  var c=Client(); var r=await c.PostAsJsonAsync("api/auth/login",new{email,password});
  if(!r.IsSuccessStatusCode)return(false,await Error(r));
  var set=r.Headers.TryGetValues("Set-Cookie",out var values)?values.FirstOrDefault():"";
  var cookie=set?.Split(';')[0]; if(string.IsNullOrWhiteSpace(cookie))return(false,"The API did not issue an authentication cookie.");
  _accessor.HttpContext!.Session.SetString("ApiCookie",cookie);
  var u=await r.Content.ReadFromJsonAsync<JsonElement>(); SaveUser(u);
  return(true,"");
 }
 public async Task<(bool Ok,string Error)> Register(object model){var r=await Client().PostAsJsonAsync("api/auth/register",model);return(r.IsSuccessStatusCode,r.IsSuccessStatusCode?"":await Error(r));}
 public void SaveUser(JsonElement u){var s=_accessor.HttpContext!.Session;s.SetInt32("UserId",u.GetProperty("userId").GetInt32());s.SetString("FirstName",u.GetProperty("firstName").GetString()??"");s.SetString("LastName",u.GetProperty("lastName").GetString()??"");s.SetString("Email",u.GetProperty("email").GetString()??"");s.SetString("Role",u.GetProperty("role").GetString()??"");}
 public async Task<T?> Get<T>(string path)=>await Client().GetFromJsonAsync<T>(path,Json);
 public async Task<(bool Ok,string Error)> Send(string method,string path,object? body=null){
  var req=new HttpRequestMessage(new HttpMethod(method),path);
  if(body!=null)req.Content=JsonContent.Create(body,options:Json);
  var r=await Client().SendAsync(req); return(r.IsSuccessStatusCode,r.IsSuccessStatusCode?"":await Error(r));
 }
 public async Task<(bool Ok,string Error)> Upload(string path,IFormFile file){
  using var form=new MultipartFormDataContent(); await using var stream=file.OpenReadStream(); using var content=new StreamContent(stream);
  content.Headers.ContentType=new MediaTypeHeaderValue(file.ContentType); form.Add(content,"file",Path.GetFileName(file.FileName));
  var r=await Client().PostAsync(path,form);return(r.IsSuccessStatusCode,r.IsSuccessStatusCode?"":await Error(r));
 }
 public async Task<string> Error(HttpResponseMessage r){try{var e=await r.Content.ReadFromJsonAsync<JsonElement>();if(e.TryGetProperty("message",out var m))return m.GetString()??"Request failed.";if(e.TryGetProperty("title",out var t))return t.GetString()??"Request failed.";}catch{}return $"Request failed ({(int)r.StatusCode} {r.ReasonPhrase}).";}
 public async Task Logout(){try{await Client().PostAsync("api/auth/logout",null);}catch{} _accessor.HttpContext!.Session.Clear();}
}
