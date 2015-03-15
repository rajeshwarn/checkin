using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.Text;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;

using Google.GData.Client;
using Google.GData.Spreadsheets;
using Google.Apis.Drive.v2;
using GDrive = Google.Apis.Drive.v2.Data;

using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Util.Store;

using IEEECheckin.ASPDocs.Models;
using Google.Apis.Drive.v2.Data;


namespace IEEECheckin.ASPDocs.Models
{
    // You can add User data for the user by adding more properties to your User class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ClaimsIdentity GenerateUserIdentity(ApplicationUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = manager.CreateIdentity(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            return Task.FromResult(GenerateUserIdentity(manager));
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class GoogleSheet
    {
        [XmlAttribute]
        public string Title { get; set; }
        [XmlAttribute]
        public string Uri { get; set; }
        [XmlAttribute]
        public string FeedUri { get; set; }
        [XmlAttribute]
        public string Id { get; set; }
        [XmlIgnoreAttribute]
        public GoogleFolder Parent { get; set; }

        public GoogleSheet() { }

        public GoogleSheet(GoogleFolder parent, string id, string title, string uri, string feedUri)
        {
            Parent = parent;
            Id = id;
            Title = title;
            Uri = uri;
            FeedUri = feedUri;
        }
    }

    public class GoogleFolder
    {
        [XmlAttribute]
        public string Title { get; set; }
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public string Uri { get; set; }
        [XmlAttribute]
        public int Level { get; set; }
        private int _childCount = 0;
        [XmlAttribute]
        public int ChildrenCount
        { 
            get
            {
                return (_childCount = (Children != null) ? Children.Count : 0);
            }
            set
            {
                _childCount = (Children != null) ? Children.Count : 0;
            }
        }
        public List<GoogleFolder> Children { get; set; }
        private int _sheetCount = 0;
        [XmlAttribute]
        public int SheetCount
        {
            get
            {
                return (_sheetCount = (Sheets != null) ? Sheets.Count : 0);
            }
            set
            {
                _sheetCount = (Sheets != null) ? Sheets.Count : 0;
            }
        }
        public List<GoogleSheet> Sheets { get; set; }
        [XmlIgnoreAttribute]
        public GoogleFolder Parent { get; set; }

        public GoogleFolder()
        {
            Children = new List<GoogleFolder>();
            Sheets = new List<GoogleSheet>();
        }

        public GoogleFolder(GoogleFolder parent, string id, string title, string uri, int level)
        {
            Children = new List<GoogleFolder>();
            Sheets = new List<GoogleSheet>();
            Parent = parent;
            Id = id;
            Title = title;
            Uri = uri;
            Level = level;
        }

    }

    public class CellAddress
    {
        public uint Row { get; set; }
        public uint Col { get; set; }
        public string IdString { get; set; }

        public CellAddress() { }

        public CellAddress(uint row, uint col)
        {
            Row = row;
            Col = col;
            IdString = string.Format("R{0}C{1}", row, col);
        }
    }

    public class CheckinEntry
    {
        private const string _dateFormat = "yyyy-MM-dd";

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StudentId { get; set; }
        public string Email { get; set; }
        public string Meeting { get; set; }
        public string DateStr { get; set; }

        public CheckinEntry() { }

        public CheckinEntry(string firstName, string lastName, string meeting, string date, string email = "", string studentId = "")
        {
            FirstName = firstName;
            LastName = lastName;
            Meeting = meeting;
            DateStr = date;
            Email = email;
            StudentId = studentId;
        }

        public CheckinEntry(string firstName, string lastName, string meeting, int day, int month, int year, string email = "", string studentId = "")
        {
            FirstName = firstName;
            LastName = lastName;
            Meeting = meeting;
            DateStr = String.Format("{0}-{1}-{2}", year, month.ToString("D2"), day.ToString("D2"));
            Email = email;
            StudentId = studentId;
        }

        public CheckinEntry(string firstName, string lastName, string meeting, string day, string month, string year, string email = "", string studentId = "")
        {
            FirstName = firstName;
            LastName = lastName;
            Meeting = meeting;
            DateStr = String.Format("{0}-{1}-{2}", year, month, day);
            Email = email;
            StudentId = studentId;
        }

        public CheckinEntry(string firstName, string lastName, string meeting, DateTime date, string email = "", string studentId = "")
        {
            FirstName = firstName;
            LastName = lastName;
            Meeting = meeting;
            DateStr = date.ToString(_dateFormat);
            Email = email;
            StudentId = studentId;
        }

    }

    public class GoogleClientSecretsData
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string[] redirect_uris { get; set; }
        public string auth_uri { get; set; }
        public string token_uri { get; set; }

        public GoogleClientSecretsData() { }
    }

    public class GoogleOAuth2
    {

        /// Google authentication flow
        /// 1. Call 'GoogleAuthenticate' from any page. This will return a URI which you should redirect to.
        /// 2. User will be redirected to Google permissions page where they can accept or decline.
        /// 3. User accepts and will be redirected to the 'GoogleRedirectUri' page found in Web.config. This is also set in the Google developer console.
        /// 4. The redirected page will be given an access code as an URL query parameter.
        /// 5. Call 'GoogleAuthToken' from the redirected page to get the access and refresh token.
        /// 6. These tokens are encrypted and saved to the user's cookies for later access and refresh use.
        /// 7. Once authenticated, 'GoogleAuthDrive' and 'GoogleAuthSheets' can be called to get the Drive and Sheets services respectively.
        /// 8. Access tokens last about an hour, of which time a refresh token is needed to be used. If a call in step 7 returns null, call 'GoogleAuthRefresh' to get a new access token.


        /// <summary>
        /// Gets the JSON format for the OAuth2 authentication parameters.
        /// </summary>
        /// <returns>The authentication parameters as a JSON string.</returns>
        public static string GetParametersJson()
        {
            GoogleClientSecretsData secrets = new GoogleClientSecretsData();
            
            secrets.client_id = ConfigurationManager.AppSettings["GoogleClientId"];
            secrets.client_secret = ConfigurationManager.AppSettings["GoogleClientSecret"];
            secrets.redirect_uris = ConfigurationManager.AppSettings["GoogleRedirectUri"].Split(new char[1] { ' ' });
            secrets.auth_uri = "https://accounts.google.com/o/oauth2/auth";
            secrets.token_uri = "https://accounts.google.com/o/oauth2/token";

            string output = JsonConvert.SerializeObject(secrets);
            return "{\"web\": " + output + " }";

        }

        /// <summary>
        /// Gets the OAuth2 parameters, before any authentication has occurred.
        /// </summary>
        /// <param name="forceRefresh">If a force refresh should occur.</param>
        /// <returns>The OAuth2 parameters.</returns>
        public static OAuth2Parameters GetParameters(bool forceRefresh = false)
        {
            // OAuth2Parameters holds all the parameters related to OAuth 2.0.
            OAuth2Parameters parameters = new OAuth2Parameters();

            parameters.ClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            parameters.ClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
            parameters.Scope = ConfigurationManager.AppSettings["GoogleDriveScope"];
            parameters.RedirectUri = ConfigurationManager.AppSettings["GoogleRedirectUri"];
            parameters.AccessType = "offline";

            if (forceRefresh)
            {
                parameters.ApprovalPrompt = "force";
            }

            return parameters;
        }

        /// <summary>
        /// Gets the OAuth2 parameters from the provided access and refresh Tokens.
        /// </summary>
        /// <param name="accessToken">The previously obtained and encrypted Google access token.</param>
        /// <param name="refreshToken">The previously obtained and encrypted Google refresh token.</param>
        /// <param name="forceRefresh">If a force refresh should occur.</param>
        /// <returns>The OAuth2 parameters.</returns>
        public static OAuth2Parameters GetParameters(string accessToken, string refreshToken, bool forceRefresh = false)
        {
            // OAuth2Parameters holds all the parameters related to OAuth 2.0.
            OAuth2Parameters parameters = new OAuth2Parameters();

            parameters.ClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            parameters.ClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
            parameters.Scope = ConfigurationManager.AppSettings["GoogleDriveScope"];
            parameters.RedirectUri = ConfigurationManager.AppSettings["GoogleRedirectUri"];
            parameters.AccessType = "offline";

            if (forceRefresh)
            {
                parameters.ApprovalPrompt = "force";
            }

            FormsAuthenticationTicket tokenTicket = FormsAuthentication.Decrypt(accessToken);
            parameters.AccessToken = tokenTicket.UserData;

            FormsAuthenticationTicket refreshTicket = FormsAuthentication.Decrypt(refreshToken);
            parameters.RefreshToken = refreshTicket.UserData;

            return parameters;
        }

        /// <summary>
        /// Gets the OAuth2 parameters, including the authentication tokens after authentication has occurred.
        /// </summary>
        /// <param name="Request">The page request.</param>
        /// <param name="forceRefresh">If a force refresh should occur.</param>
        /// <returns>The OAuth2 parameters.</returns>
        public static OAuth2Parameters GetParameters(HttpRequest Request, bool forceRefresh = false)
        {
            // OAuth2Parameters holds all the parameters related to OAuth 2.0.
            OAuth2Parameters parameters = new OAuth2Parameters();

            string codeCookieName = ConfigurationManager.AppSettings["GoogleCodeCookie"];
            string tokenCookieName = ConfigurationManager.AppSettings["GoogleTokenCookie"];
            string refreshCookieName = ConfigurationManager.AppSettings["GoogleRefreshCookie"];

            parameters.ClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            parameters.ClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
            parameters.Scope = ConfigurationManager.AppSettings["GoogleDriveScope"];
            parameters.RedirectUri = ConfigurationManager.AppSettings["GoogleRedirectUri"];
            parameters.AccessType = "offline";

            if (forceRefresh)
            {
                parameters.ApprovalPrompt = "force";
            }


            if (Request.Cookies[codeCookieName] != null && !String.IsNullOrWhiteSpace(Request.Cookies[codeCookieName].Value))
            {
                FormsAuthenticationTicket codeTicket = FormsAuthentication.Decrypt(Request.Cookies[codeCookieName].Value);
                parameters.AccessCode = codeTicket.UserData;
            }

            if (Request.Cookies[tokenCookieName] != null && !String.IsNullOrWhiteSpace(Request.Cookies[tokenCookieName].Value))
            {
                FormsAuthenticationTicket tokenTicket = FormsAuthentication.Decrypt(Request.Cookies[tokenCookieName].Value);
                parameters.AccessToken = tokenTicket.UserData;
            }

            if (Request.Cookies[refreshCookieName] != null && !String.IsNullOrWhiteSpace(Request.Cookies[refreshCookieName].Value))
            {
                FormsAuthenticationTicket refreshTicket = FormsAuthentication.Decrypt(Request.Cookies[refreshCookieName].Value);
                parameters.RefreshToken = refreshTicket.UserData;
            }
            
            return parameters;
        }

        /// <summary>
        /// Gets the Spreadsheet Service using the previously obtained authentication tokens.
        /// </summary>
        /// <param name="Request">The page request.</param>
        /// <returns>The Spreadsheet Service.</returns>
        public static SpreadsheetsService GoogleAuthSheets(HttpRequest Request)
        {
            // Get OAuth parameters (from config and cookies)
            OAuth2Parameters parameters = GoogleOAuth2.GetParameters(Request);
            if (parameters == null || String.IsNullOrWhiteSpace(parameters.AccessToken) || String.IsNullOrWhiteSpace(parameters.RefreshToken))
            {
                return null;
            }

            // Make an OAuth authorized request to Google
            // Initialize the variables needed to make the request
            GOAuth2RequestFactory requestFactory =
                new GOAuth2RequestFactory(null, ConfigurationManager.AppSettings["ApplicationName"], parameters);
            SpreadsheetsService service = new SpreadsheetsService(ConfigurationManager.AppSettings["ApplicationName"]);
            service.RequestFactory = requestFactory;

            return service;
        }

        /// <summary>
        /// Gets the Spreadsheet Service using the previously obtained authentication tokens.
        /// </summary>
        /// <param name="accessToken">The previously obtained and encrypted Google access token.</param>
        /// <param name="refreshToken">The previously obtained and encrypted Google refresh token.</param>
        /// <returns>The Spreadsheet Service.</returns>
        public static SpreadsheetsService GoogleAuthSheets(string accessToken, string refreshToken)
        {
            // Get OAuth parameters (from config and cookies)
            OAuth2Parameters parameters = GoogleOAuth2.GetParameters(accessToken, refreshToken);
            if (parameters == null || String.IsNullOrWhiteSpace(parameters.AccessToken) || String.IsNullOrWhiteSpace(parameters.RefreshToken))
            {
                return null;
            }

            // Make an OAuth authorized request to Google
            // Initialize the variables needed to make the request
            GOAuth2RequestFactory requestFactory =
                new GOAuth2RequestFactory(null, ConfigurationManager.AppSettings["ApplicationName"], parameters);
            SpreadsheetsService service = new SpreadsheetsService(ConfigurationManager.AppSettings["ApplicationName"]);
            service.RequestFactory = requestFactory;

            return service;
        }

        /// <summary>
        /// Gets the Drive Service using previously obtained authentication tokens.
        /// </summary>
        /// <param name="Request">The page request.</param>
        /// <returns>The Drive Service.</returns>
        public static DriveService GoogleAuthDrive(HttpRequest Request)
        {
            // Get OAuth parameters (from config and cookies)
            OAuth2Parameters parameters = GoogleOAuth2.GetParameters(Request);
            if (parameters == null || String.IsNullOrWhiteSpace(parameters.AccessToken) || String.IsNullOrWhiteSpace(parameters.RefreshToken))
            {
                return null;
            }

            GoogleAuthorizationCodeFlow flow;
            string secretStr = GetParametersJson();
            using (Stream stream = GenerateStreamFromString(secretStr))
            {
                flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    DataStore = new FileDataStore("Drive.Sample.Store"),
                    ClientSecretsStream = stream,
                    Scopes = ConfigurationManager.AppSettings["GoogleDriveScope"].Split(new char[1] { ' ' })
                });
            }

            TokenResponse tok = new TokenResponse()
            {
                AccessToken = parameters.AccessToken,
                RefreshToken = parameters.RefreshToken,
                Scope = parameters.Scope,
                TokenType = "Bearer",
                ExpiresInSeconds = 3600
            };

            UserCredential cred = new UserCredential(flow, "user-id", tok);

            return new DriveService(new BaseClientService.Initializer
            {
                ApplicationName = ConfigurationManager.AppSettings["ApplicationName"],
                HttpClientInitializer = cred
            });
        }

        /// <summary>
        /// Gets the Drive Service using previously obtained authentication tokens.
        /// </summary>
        /// <param name="accessToken">The previously obtained and encrypted Google access token.</param>
        /// <param name="refreshToken">The previously obtained and encrypted Google refresh token.</param>
        /// <returns>The Drive Service.</returns>
        public static DriveService GoogleAuthDrive(string accessToken, string refreshToken)
        {
            // Get OAuth parameters (from config and cookies)
            OAuth2Parameters parameters = GoogleOAuth2.GetParameters(accessToken, refreshToken);
            if (parameters == null || String.IsNullOrWhiteSpace(parameters.AccessToken) || String.IsNullOrWhiteSpace(parameters.RefreshToken))
            {
                return null;
            }

            GoogleAuthorizationCodeFlow flow;
            string secretStr = GetParametersJson();
            using (Stream stream = GenerateStreamFromString(secretStr))
            {
                flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    DataStore = new FileDataStore("Drive.Sample.Store"),
                    ClientSecretsStream = stream,
                    Scopes = ConfigurationManager.AppSettings["GoogleDriveScope"].Split(new char[1] { ' ' })
                });
            }

            TokenResponse tok = new TokenResponse()
            {
                AccessToken = parameters.AccessToken,
                RefreshToken = parameters.RefreshToken,
                Scope = parameters.Scope,
                TokenType = "Bearer",
                ExpiresInSeconds = 3600
            };

            UserCredential cred = new UserCredential(flow, "user-id", tok);

            return new DriveService(new BaseClientService.Initializer
            {
                ApplicationName = ConfigurationManager.AppSettings["ApplicationName"],
                HttpClientInitializer = cred
            });
        }

        /// <summary>
        /// Generates a stream from a string.
        /// </summary>
        /// <param name="s">The string to generate a stream from.</param>
        /// <returns>A stream consisting of the contents of the provided string.</returns>
        private static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Performs the initial step in Google authentication to get the access code.
        /// </summary>
        /// <param name="Request">The page request.</param>
        /// <returns>The Google response URI.</returns>
        public static string GoogleAuthenticate(HttpRequest Request)
        {
            string refreshCookieName = ConfigurationManager.AppSettings["GoogleRefreshCookie"];

            bool forceRefresh = false;
            if (Request.Cookies[refreshCookieName] == null || String.IsNullOrWhiteSpace(Request.Cookies[refreshCookieName].Value))
                forceRefresh = true; // refresh token is gone, so need to force new creation

            return OAuthUtil.CreateOAuth2AuthorizationUrl(GoogleOAuth2.GetParameters(forceRefresh));
        }

        /// <summary>
        /// Performs the second authentication step after receiving the access code.
        /// </summary>
        /// <param name="Request">The page request.</param>
        /// <param name="Response">The page response.</param>
        /// <param name="forceRefresh">If a forced refresh of access privileges should occur.</param>
        /// <param name="expireHours">The hours to save the cookie for (default is a year).</param>
        /// <returns>If the authentication was successful.</returns>
        public static bool GoogleAuthToken(HttpRequest Request, HttpResponse Response, bool forceRefresh = false, int expireHours = 8765)
        {
            bool persistent = true;
            if (expireHours > 0)
                persistent = false;

            string codeCookieName = ConfigurationManager.AppSettings["GoogleCodeCookie"];
            string tokenCookieName = ConfigurationManager.AppSettings["GoogleTokenCookie"];
            string refreshCookieName = ConfigurationManager.AppSettings["GoogleRefreshCookie"];

            if (Request.Cookies[refreshCookieName] == null || String.IsNullOrWhiteSpace(Request.Cookies[refreshCookieName].Value))
                forceRefresh = true; // refresh token is gone, so need to force new creation


            OAuth2Parameters parameters = GoogleOAuth2.GetParameters(forceRefresh);
            if(parameters == null)
            {
                return false;
            }

            // parse access code from url
            if (String.IsNullOrWhiteSpace(Request.QueryString["code"]))
            {
                return false;
            }
            parameters.AccessCode = Request.QueryString["code"];

            // save access code to secure cookie for later use
            FormsAuthenticationTicket codeTicket = new FormsAuthenticationTicket(1, codeCookieName, DateTime.Now, DateTime.Now.AddHours(expireHours), persistent, parameters.AccessCode, FormsAuthentication.FormsCookiePath);
            string encCodeTicket = FormsAuthentication.Encrypt(codeTicket);
            Response.Cookies.Add(new HttpCookie(codeCookieName, encCodeTicket));
            //Response.Cookies[codeCookieName].HttpOnly = true;
            if (!persistent)
                Response.Cookies[codeCookieName].Expires = DateTime.Now.AddHours(expireHours);

            // Once the user authorizes with Google, the request token (from url) can be exchanged
            // for a long-lived access token.
            OAuthUtil.GetAccessToken(parameters);

            // Save access token to secure cookie for later use
            FormsAuthenticationTicket tokenTicket = new FormsAuthenticationTicket(1, tokenCookieName, DateTime.Now, DateTime.Now.AddHours(expireHours), persistent, parameters.AccessToken, FormsAuthentication.FormsCookiePath);
            string encTokenTicket = FormsAuthentication.Encrypt(tokenTicket);
            Response.Cookies.Add(new HttpCookie(tokenCookieName, encTokenTicket));
            //Response.Cookies[tokenCookieName].HttpOnly = true;
            if (!persistent)
                Response.Cookies[tokenCookieName].Expires = DateTime.Now.AddHours(expireHours);

            // Save refresh token to secure cookie for later use
            if (!String.IsNullOrWhiteSpace(parameters.RefreshToken))
            {
                FormsAuthenticationTicket refreshTicket = new FormsAuthenticationTicket(1, refreshCookieName, DateTime.Now, DateTime.Now.AddHours(expireHours), true, parameters.RefreshToken, FormsAuthentication.FormsCookiePath);
                string encRefreshTicket = FormsAuthentication.Encrypt(refreshTicket);
                Response.Cookies.Add(new HttpCookie(refreshCookieName, encRefreshTicket));
                //Response.Cookies[refreshCookieName].HttpOnly = true;
                Response.Cookies[refreshCookieName].Expires = DateTime.Now.AddHours(expireHours);
            }

            return true;
        }

        /// <summary>
        /// Gets a new access token using the previously acquired refresh token.
        /// </summary>
        /// <param name="Request">The page request.</param>
        /// <param name="Response">The page response.</param>
        /// <param name="expireHours">The hours to save the cookie for (default is a year).</param>
        /// <returns>If the refresh was successful.</returns>
        public static bool GoogleAuthRefresh(HttpRequest Request, HttpResponse Response, int expireHours = 8765)
        {
            bool persistent = true;
            if (expireHours > 0)
                persistent = false;

            string tokenCookieName = ConfigurationManager.AppSettings["GoogleTokenCookie"];
            string refreshCookieName = ConfigurationManager.AppSettings["GoogleRefreshCookie"];

            OAuth2Parameters parameters = GoogleOAuth2.GetParameters(Request);
            if (parameters == null || String.IsNullOrWhiteSpace(parameters.RefreshToken))
            {
                return false;
            }

            // Once the user authorizes with Google, the request token (from url) can be exchanged
            // for a long-lived access token.
            OAuthUtil.GetAccessToken(parameters);

            // Save access token to secure cookie for later use
            FormsAuthenticationTicket tokenTicket = new FormsAuthenticationTicket(1, tokenCookieName, DateTime.Now, DateTime.Now.AddHours(expireHours), persistent, parameters.AccessToken, FormsAuthentication.FormsCookiePath);
            string encTokenTicket = FormsAuthentication.Encrypt(tokenTicket);
            Response.Cookies.Add(new HttpCookie(tokenCookieName, encTokenTicket));
            //Response.Cookies[tokenCookieName].HttpOnly = true;
            if (!persistent)
                Response.Cookies[tokenCookieName].Expires = DateTime.Now.AddHours(expireHours);

            // Save refresh token to secure cookie for later use
            if (!String.IsNullOrWhiteSpace(parameters.RefreshToken))
            {
                FormsAuthenticationTicket refreshTicket = new FormsAuthenticationTicket(1, refreshCookieName, DateTime.Now, DateTime.Now.AddHours(expireHours), true, parameters.RefreshToken, FormsAuthentication.FormsCookiePath);
                string encRefreshTicket = FormsAuthentication.Encrypt(refreshTicket);
                Response.Cookies.Add(new HttpCookie(refreshCookieName, encRefreshTicket));
                //Response.Cookies[refreshCookieName].HttpOnly = true;
                Response.Cookies[refreshCookieName].Expires = DateTime.Now.AddHours(expireHours);
            }

            return true;
        }

        /// <summary>
        /// Gets if cookies exist for Google authentication.
        /// </summary>
        /// <param name="Request">The page request.</param>
        /// <returns>If cookies exist for Google authentication.</returns>
        public static bool IsGoogleAuthenticated(HttpRequest Request)
        {
            string tokenCookieName = ConfigurationManager.AppSettings["GoogleTokenCookie"];
            string refreshCookieName = ConfigurationManager.AppSettings["GoogleRefreshCookie"];

            if (Request.Cookies[tokenCookieName] != null && !String.IsNullOrWhiteSpace(Request.Cookies[tokenCookieName].Value)
                && Request.Cookies[refreshCookieName] != null && !String.IsNullOrWhiteSpace(Request.Cookies[refreshCookieName].Value))
                return true;

            return false;
        }

        /// <summary>
        /// Deletes the Google authentication cookies, requiring the user to re-enter their credentials.
        /// </summary>
        /// <param name="Request">The page request.</param>
        public static void GoogleLogOff(HttpRequest Request, HttpResponse Response)
        {
            string codeCookieName = ConfigurationManager.AppSettings["GoogleCodeCookie"];
            string tokenCookieName = ConfigurationManager.AppSettings["GoogleTokenCookie"];
            string refreshCookieName = ConfigurationManager.AppSettings["GoogleRefreshCookie"];

            if (Request.Cookies[codeCookieName] != null)
            {
                HttpCookie aCookie = new HttpCookie(codeCookieName);
                aCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(aCookie);
            }

            if (Request.Cookies[tokenCookieName] != null)
            {
                HttpCookie aCookie = new HttpCookie(tokenCookieName);
                aCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(aCookie);
            }

            if (Request.Cookies[refreshCookieName] != null)
            {
                HttpCookie aCookie = new HttpCookie(refreshCookieName);
                aCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(aCookie);
            }
        }

    }

    public class GoogleDriveHelpers
    {
        public const string _FolderMime = "application/vnd.google-apps.folder";
        public const string _SheetMime = "application/vnd.google-apps.spreadsheet";
        private const string _UserId = "user-id";

        #region Folder Tree

        /// <summary>
        /// Serializes an object into an XML string.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="value">The object instance to serialize.</param>
        /// <returns>The serialized object XML string.</returns>
        public static string SerializeXml<T>(T value)
        {

            if (value == null)
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }

        /// <summary>
        /// Retrieves all sheets using the SpreadSheet API.
        /// </summary>
        /// <param name="service">The Google SpreadSheet service to access.</param>
        /// <returns>A list of Google sheets.</returns>
        public static List<GoogleSheet> GoogleRetrieveAllSheets(SpreadsheetsService service)
        {
            // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
            SpreadsheetQuery query = new SpreadsheetQuery();

            // Make a request to the API and get all spreadsheets.
            SpreadsheetFeed feed = service.Query(query);

            List<GoogleSheet> sheetList = new List<GoogleSheet>(feed.Entries.Count);
            sheetList.Add(new GoogleSheet(null, "", "<Select a Spreadsheet>", "", ""));

            // Iterate through all of the spreadsheets returned
            foreach (SpreadsheetEntry entry in feed.Entries)
            {
                // Print the title of this spreadsheet to the screen
                sheetList.Add(new GoogleSheet(null, entry.Id.Uri.Content.Split('/').Last(), entry.Title.Text, entry.Id.Uri.Content, entry.Id.Uri.Content));
            }

            return sheetList;
        }

        /// <summary>
        /// Retrieve a list of File resources.
        /// </summary>
        /// <param name="service">Drive API service instance.</param>
        /// <returns>List of File resources.</returns>
        public static List<GDrive.File> GoogleRetrieveAllSheets(DriveService service)
        {
            List<GDrive.File> result = new List<GDrive.File>();
            FilesResource.ListRequest request = service.Files.List();
            request.Q = String.Format("mimeType='{0}'", _SheetMime);

            do
            {
                try
                {
                    GDrive.FileList files = request.Execute();

                    result.AddRange(files.Items);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return result;
        }

        /// <summary>
        /// Retrieve all sheets and their folder structure using the Drive API.
        /// </summary>
        /// <param name="service">The Google Drive service to access.</param>
        /// <param name="root">The root folder instance to start the tree search. Use null for root.</param>
        /// <param name="level">The current level of the folder in the tree.</param>
        /// <returns>The populated Google Drive folder.</returns>
        public static GoogleFolder GoogleRetrieveSheetTree(DriveService service, GoogleFolder root, ref List<GoogleSheet> sheetList, int level = 1)
        {
            // Start with relative root folder
            if (root == null)
            {
                root = root = new GoogleFolder(null, "root", "<i class=\"fa fa-folder\"></i>   root", "", 0);
                level = 1;
            }
            ChildrenResource.ListRequest request = service.Children.List(root.Id);
            // Request immediate folder children of the relative root folder
            request.Q = String.Format("mimeType='{0}'", _FolderMime);

            do
            {
                try
                {
                    ChildList children = request.Execute();

                    foreach (ChildReference child in children.Items)
                    {
                        // Get child folder metadata
                        GDrive.File file = service.Files.Get(child.Id).Execute();
                        // Add folder to relative root children folder list
                        root.Children.Add(new GoogleFolder(root, child.Id, "<i class=\"fa fa-folder\"></i>   " + file.Title, child.ChildLink, level));
                    }

                    request.PageToken = children.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            // Get list of sheets in the relative root folder
            FilesResource.ListRequest requestFile = service.Files.List();
            requestFile.Q = String.Format("mimeType='{0}' and '{1}' in parents", _SheetMime, root.Id);

            do
            {
                try
                {
                    GDrive.FileList files = requestFile.Execute();

                    foreach (GDrive.File file in files.Items)
                    {
                        // Get feed uri for current sheet
                        string feedUri = "";
                        if (sheetList != null && sheetList.Count > 0)
                        {
                            feedUri = (from feed in sheetList where feed.Id.Equals(file.Id) select feed.FeedUri).First();
                        }
                        // Add sheet to relative root sheet list
                        root.Sheets.Add(new GoogleSheet(root, file.Id, "<i class=\"fa fa-file\"></i>   " + file.Title, file.SelfLink, feedUri));
                    }
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            // Recurse through the children folders
            for (int i = 0; i < root.Children.Count; i++)
            {
                root.Children[i] = GoogleRetrieveSheetTree(service, root.Children[i], ref sheetList, level + 1);
            }

            // Trim away folders that don't contain children and don't contain sheets
            List<GoogleFolder> notNullChildren = new List<GoogleFolder>();
            foreach (GoogleFolder folder in root.Children)
            {
                if (folder != null)
                    notNullChildren.Add(folder);
            }
            root.Children.Clear();
            root.Children.AddRange(notNullChildren);

            // Return null if not sheets and no children with sheets
            if (root.ChildrenCount > 0 || root.SheetCount > 0)
                return root;
            else
                return null;
        }

        #endregion

        #region Worksheet Addition

        /// <summary>
        /// Creates the check-in file content to be placed in the file.
        /// </summary>
        /// <param name="xmlData">The check-in data as an XML string.</param>
        /// <returns>The list of check-in entries.</returns>
        public static List<CheckinEntry> CreateFileContent(string xmlData)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(xmlData))
                    return null;

                // create file content
                XmlDocument xml = JsonConvert.DeserializeXmlNode(xmlData, "data", false);

                List<CheckinEntry> entries = new List<CheckinEntry>();

                // spreadsheet column layout
                //"Student Id,First Name,Last Name,Email,Date,Meeting"

                foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                {
                    entries.Add(new CheckinEntry()
                    {
                        StudentId = node.ChildNodes[0].FirstChild.Value,
                        FirstName = node.ChildNodes[1].FirstChild.Value,
                        LastName = node.ChildNodes[2].FirstChild.Value,
                        Email = node.ChildNodes[3].FirstChild.Value,
                        DateStr = node.ChildNodes[4].FirstChild.Value,
                        Meeting = node.ChildNodes[5].FirstChild.Value
                    });
                }

                return entries;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a spreadsheet file on Google Drive.
        /// </summary>
        /// <param name="service">The Google Drive service to access.</param>
        /// <param name="docName">The name of the document to add.</param>
        /// <param name="parentFolderId">The id of the folder to add the document to. Default is root.</param>
        /// <returns>The file created.</returns>
        public static GDrive.File CreateFile(DriveService service, string docName, string parentFolderId = "root")
        {
            try
            {
                if (service == null || String.IsNullOrWhiteSpace(docName))
                    return null;

                // File's metadata.
                GDrive.File body = new GDrive.File();
                body.Title = docName;
                body.Description = "";
                body.MimeType = _SheetMime;

                if (String.IsNullOrWhiteSpace(parentFolderId))
                    parentFolderId = "root";

                // Set the parent folder.
                if (!String.IsNullOrWhiteSpace(parentFolderId))
                    body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentFolderId } };

                FilesResource.InsertRequest request = service.Files.Insert(body);
                return request.Execute();
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a worksheet in a Google spreadsheet or gets the existing worksheet with the same meeting name.
        /// </summary>
        /// <param name="service">The Spreadsheet service to access.</param>
        /// <param name="feedUri">The full feed URI to the spreadsheet.</param>
        /// <param name="meetingName">The name of the meeting.</param>
        /// <returns>The spreadsheet created or found.</returns>
        public static WorksheetEntry CreateWorksheet(SpreadsheetsService service, string feedUri, string meetingName = "")
        {
            try
            {
                if (service == null || String.IsNullOrWhiteSpace(feedUri))
                    return null;

                SpreadsheetEntry spreadsheet = QuerySpreadsheet(service, feedUri);
                if (spreadsheet == null)
                    return null; // error

                // Create new worksheet in selected spreadhseet

                if (String.IsNullOrWhiteSpace(meetingName))
                    meetingName = "all_meetings_" + DateTime.Now.ToString("yyyy-MM-dd");
                meetingName = meetingName.Replace(" ", "_");

                // Find if worksheet already present
                WorksheetEntry worksheet = QueryWorksheet(service, feedUri, meetingName);

                // not present so create new worksheet
                if (worksheet == null)
                {
                    worksheet = new WorksheetEntry();
                    worksheet.Title.Text = meetingName;
                    worksheet.Cols = 6;
                    worksheet.Rows = 2;

                    // Send the local representation of the worksheet to the API for
                    // creation.  The URL to use here is the worksheet feed URL of our
                    // spreadsheet.
                    WorksheetFeed wsFeed = spreadsheet.Worksheets;
                    service.Insert(wsFeed, worksheet);


                    // update worksheet query
                    worksheet = QueryWorksheet(service, feedUri, meetingName);
                    if (worksheet == null)
                        return null; // error

                    // Create header row
                    // Fetch the cell feed of the worksheet.
                    CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
                    CellFeed cellFeed = service.Query(cellQuery);

                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("R1C1", "studentid");
                    headers.Add("R1C2", "firstname");
                    headers.Add("R1C3", "lastname");
                    headers.Add("R1C4", "email");
                    headers.Add("R1C5", "date");
                    headers.Add("R1C6", "meeting");

                    List<CellAddress> cellAddrs = new List<CellAddress>();
                    for (uint col = 1; col <= 6; ++col)
                        cellAddrs.Add(new CellAddress(1, col));

                    // Prepare the update
                    // GetCellEntryMap is what makes the update fast.
                    Dictionary<String, CellEntry> cellEntries = GetCellEntryMap(service, cellFeed, cellAddrs);

                    CellFeed batchRequest = new CellFeed(cellQuery.Uri, service);
                    foreach (CellAddress cellAddr in cellAddrs)
                    {
                        CellEntry batchEntry = cellEntries[cellAddr.IdString];
                        batchEntry.InputValue = headers[cellAddr.IdString];
                        batchEntry.BatchData = new GDataBatchEntryData(cellAddr.IdString, GDataBatchOperationType.update);
                        batchRequest.Entries.Add(batchEntry);
                    }

                    // Submit the update
                    CellFeed batchResponse = (CellFeed)service.Batch(batchRequest, new Uri(cellFeed.Batch));

                    // update worksheet query again
                    return QueryWorksheet(service, feedUri, meetingName);
                }
                else
                    return worksheet;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Adds the check-in entries to the worksheet.
        /// </summary>
        /// <param name="service">The Spreadsheet service to access.</param>
        /// <param name="worksheet">The worksheet to add the content to.</param>
        /// <param name="entries">The check-in entries to add to the worksheet.</param>
        /// <returns>If the content was successfully added to the worksheet.</returns>
        public static bool AddContentToWorksheet(SpreadsheetsService service, WorksheetEntry worksheet, List<CheckinEntry> entries)
        {
            try
            {
                if (service == null || worksheet == null || entries == null || entries.Count <= 0)
                    return false;

                // Define the URL to request the list feed of the worksheet.
                AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

                // Fetch the list feed of the worksheet.
                ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                ListFeed listFeed = service.Query(listQuery);

                foreach (CheckinEntry entry in entries)
                {
                    // Create a local representation of the new row.
                    ListEntry row = new ListEntry();
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "studentid", Value = entry.StudentId });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "firstname", Value = entry.FirstName });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "lastname", Value = entry.LastName });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "email", Value = entry.Email });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "date", Value = entry.DateStr });
                    row.Elements.Add(new ListEntry.Custom() { LocalName = "meeting", Value = entry.Meeting });

                    // Send the new row to the API for insertion.
                    service.Insert(listFeed, row);
                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the spreadsheet with the given feed URI.
        /// </summary>
        /// <param name="service">The Spreadsheet service to access.</param>
        /// <param name="feedUri">The URI of the feed of the spreadsheet to get.</param>
        /// <returns>The spreadsheet of the given feed URI.</returns>
        public static SpreadsheetEntry QuerySpreadsheet(SpreadsheetsService service, string feedUri)
        {
            if (service == null || String.IsNullOrWhiteSpace(feedUri))
                return null;

            SpreadsheetQuery query = new SpreadsheetQuery(feedUri);
            
            // Make a request to the API and get all spreadsheets.
            SpreadsheetFeed feed = service.Query(query);

            if (feed.Entries.Count == 0)
            {
                return null;
            }

            return (SpreadsheetEntry)feed.Entries[0];
        }

        /// <summary>
        /// Gets the Worksheet with the given feed URI.
        /// </summary>
        /// <param name="service">The Spreadsheet service to access.</param>
        /// <param name="feedUri">The URI of the feed of the spreadsheet with the worksheet to get.</param>
        /// <param name="worksheetName">The name of the worksheet to get.</param>
        /// <returns>The requested worksheet.</returns>
        public static WorksheetEntry QueryWorksheet(SpreadsheetsService service, string feedUri, string worksheetName)
        {
            if (service == null || String.IsNullOrWhiteSpace(feedUri) || String.IsNullOrWhiteSpace(worksheetName))
                return null;

            // Re-query for the spreadsheet and updated worksheet feed
            SpreadsheetEntry spreadsheet = QuerySpreadsheet(service, feedUri);
            if (spreadsheet == null)
                return null; // error

            // Make a request to the API to fetch information about all
            // worksheets in the spreadsheet.
            WorksheetFeed wsFeed = spreadsheet.Worksheets;

            // Iterate through each worksheet in the spreadsheet.
            WorksheetEntry worksheet = null;
            foreach (WorksheetEntry entry in wsFeed.Entries)
            {
                if (entry.Title.Text.Equals(worksheetName))
                {
                    worksheet = entry;
                    break;
                }
            }

            return worksheet;
        }

        /// <summary>
        /// Gets the mapping between the column headers and their absolute cell address.
        /// </summary>
        /// <param name="service">The Spreadsheet service to access.</param>
        /// <param name="cellFeed">The Cell feed service to the worksheet.</param>
        /// <param name="cellAddrs">The addresses of the cells to map.</param>
        /// <returns>A dictionary of the cell entry and the given cell address.</returns>
        public static Dictionary<String, CellEntry> GetCellEntryMap(SpreadsheetsService service, CellFeed cellFeed, List<CellAddress> cellAddrs)
        {
            if (service == null || cellFeed == null || cellAddrs == null || cellAddrs.Count <= 0)
                return null;

            CellFeed batchRequest = new CellFeed(new Uri(cellFeed.Self), service);
            foreach (CellAddress cellId in cellAddrs)
            {
                CellEntry batchEntry = new CellEntry(cellId.Row, cellId.Col, cellId.IdString);
                batchEntry.Id = new AtomId(string.Format("{0}/{1}", cellFeed.Self, cellId.IdString));
                batchEntry.BatchData = new GDataBatchEntryData(cellId.IdString, GDataBatchOperationType.query);
                batchRequest.Entries.Add(batchEntry);
            }

            CellFeed queryBatchResponse = (CellFeed)service.Batch(batchRequest, new Uri(cellFeed.Batch));

            Dictionary<String, CellEntry> cellEntryMap = new Dictionary<String, CellEntry>();
            foreach (CellEntry entry in queryBatchResponse.Entries)
                cellEntryMap.Add(entry.BatchData.Id, entry);

            return cellEntryMap;
        }

        #endregion

    }
}

#region Helpers
namespace IEEECheckin.ASPDocs
{
    public static class IdentityHelper
    {
        // Used for XSRF when linking external logins
        public const string XsrfKey = "XsrfId";

        public const string ProviderNameKey = "providerName";
        public static string GetProviderNameFromRequest(HttpRequest request)
        {
            return request.QueryString[ProviderNameKey];
        }

        public const string CodeKey = "code";
        public static string GetCodeFromRequest(HttpRequest request)
        {
            return request.QueryString[CodeKey];
        }

        public const string UserIdKey = "userId";
        public static string GetUserIdFromRequest(HttpRequest request)
        {
            return System.Web.HttpUtility.UrlDecode(request.QueryString[UserIdKey]);
        }

        public static string GetResetPasswordRedirectUrl(string code, HttpRequest request)
        {
            var absoluteUri = "/Account/ResetPassword?" + CodeKey + "=" + System.Web.HttpUtility.UrlEncode(code);
            return new Uri(request.Url, absoluteUri).AbsoluteUri.ToString();
        }

        public static string GetUserConfirmationRedirectUrl(string code, string userId, HttpRequest request)
        {
            var absoluteUri = "/Account/Confirm?" + CodeKey + "=" + System.Web.HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + System.Web.HttpUtility.UrlEncode(userId);
            return new Uri(request.Url, absoluteUri).AbsoluteUri.ToString();
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

        public static void RedirectToReturnUrl(string returnUrl, HttpResponse response)
        {
            if (!String.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
            {
                response.Redirect(returnUrl);
            }
            else
            {
                response.Redirect("~/");
            }
        }
    }
}
#endregion
