using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace IEEECheckin.ASPDocs.MemberPages
{

    class RegexElement
    {
        public int Id { get; set; }

        public string Regex { get; set; }

        public string Name { get; set; }

        public string Indices { get; set; }

        public string Value
        {
            get
            {
                return String.Format("{{\"regex\": \"{0}\", \"indices\": {1}, \"name\": \"{2}\"}}", Regex.Replace(@"\", @"\\"), Indices, Name);
            }
        }

        public RegexElement() { }

        public RegexElement(int id, string regex, string name, string indices)
        {
            Id = id;
            Regex = regex;
            Name = name;
            Indices = indices;
        }
    }

    class ThemeElement
    {
        public int Id { get; set; }

        public string Theme { get; set; }

        public string Name { get; set; }

        public ThemeElement() { }

        public ThemeElement(int id, string theme, string name)
        {
            Id = id;
            Theme = theme;
            Name = name;
        }
    }

    public partial class Format : System.Web.UI.Page
    {
        private string _myConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DataConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                List<RegexElement> regexes = new List<RegexElement>();
                List<ThemeElement> themes = new List<ThemeElement>();

                MySqlConnection sqlConn = new MySqlConnection(_myConnectionString);
                sqlConn.Open();

                MySqlCommand regexCmd = new MySqlCommand();
                regexCmd.Connection = sqlConn;
                regexCmd.CommandType = System.Data.CommandType.Text;
                regexCmd.CommandText = String.Format("SELECT RegexId, Regex, DisplayName, Indices FROM type_regex ORDER BY DisplayName ASC;");
                using (MySqlDataReader rdr = regexCmd.ExecuteReader())
                {
                    int id = -1;
                    string regex = "";
                    string name = "";
                    string indices = "";

                    while (rdr.Read())
                    {
                        id = -1;
                        regex = "";
                        name = "";
                        indices = "";

                        if (!rdr.IsDBNull(0))
                            id = rdr.GetInt32("RegexId");
                        if (!rdr.IsDBNull(1))
                            regex = rdr.GetString("Regex");
                        if (!rdr.IsDBNull(2))
                            name = rdr.GetString("DisplayName");
                        if (!rdr.IsDBNull(3))
                            indices = rdr.GetString("Indices");

                        if (id >= 0 && !String.IsNullOrWhiteSpace(regex) && !String.IsNullOrWhiteSpace(name) && !String.IsNullOrWhiteSpace(indices))
                            regexes.Add(new RegexElement(id, regex, name, indices));
                    }
                }

                regexDropdown.DataTextField = "Name";
                regexDropdown.DataValueField = "Value";
                regexDropdown.DataSource = regexes;
                regexDropdown.DataBind();

                MySqlCommand themeCmd = new MySqlCommand();
                themeCmd.Connection = sqlConn;
                themeCmd.CommandType = System.Data.CommandType.Text;
                themeCmd.CommandText = String.Format("SELECT ThemeId, Theme, DisplayName FROM type_themes ORDER BY DisplayName ASC;");
                using (MySqlDataReader rdr = themeCmd.ExecuteReader())
                {
                    int id = -1;
                    string theme = "";
                    string name = "";

                    while (rdr.Read())
                    {
                        id = -1;
                        theme = "";
                        name = "";

                        if (!rdr.IsDBNull(0))
                            id = rdr.GetInt32("ThemeId");
                        if (!rdr.IsDBNull(1))
                            theme = rdr.GetString("Theme");
                        if (!rdr.IsDBNull(2))
                            name = rdr.GetString("DisplayName");

                        if (id >= 0 && !String.IsNullOrWhiteSpace(theme) && !String.IsNullOrWhiteSpace(name))
                            themes.Add(new ThemeElement(id, theme, name));
                    }
                }

                themeDropdown.DataTextField = "Name";
                themeDropdown.DataValueField = "Theme";
                themeDropdown.DataSource = themes;
                themeDropdown.DataBind();

                sqlConn.Close();
            }
            catch (Exception ex)
            {

            }
        }
    }
}