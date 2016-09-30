using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace fsto
{
    public static class FilmUtils
    {
        static public async Task<List<Film>> DoParse(string uri, bool mostPop = false)
        {
            List<Film> list;
            if (!mostPop)
            {
                list = await Task.FromResult<List<Film>>(FilmUtils.ParseSearchPage(uri));
            }
            else
            {
                list = await Task.FromResult<List<Film>>(FilmUtils.ParseMostViewed());
            }

            return list;
        }

        //??????? ?????????? ???????? ? ???????
        static public List<Film> ParseSearchPage(string searchUri)
        {
            List<Film> filmList = new List<Film>();
            string searchPage = "";
            searchPage = FilmUtils.getResponse(searchUri);
            Regex reg = new Regex(pattern: @"<a href=""(?<filmUri>.*)"" class="".*"" data-subsection="".*"" style="""">\s*<span class="".*"" title=""(?<filmName>.*)/.*"" style="".*"">\s*<span class="".*"">.*</span>\s*<span class="".*\"">.*</span>\s*<img src=""\/\/(?<imageUri>.*)"" border="".*"">\s*</span>",
                options: RegexOptions.Multiline);
            Film film = new Film();
            MatchCollection m = reg.Matches(searchPage);
            foreach (Match matches in m)
            {
                film.FilmUri = @"http://fs.to" + matches.Groups["filmUri"].Value;
                film.FilmName = matches.Groups["filmName"].Value;
                film.ImageUri = @"http://" + matches.Groups["imageUri"].Value;

                film = FilmUtils.SetInfoFromHtmlTable(film);
                filmList.Add(film);
            }

            return filmList;
        }

        //??????? ???????? ??????????????? ???????
        static public List<Film> ParseMostViewed()
        {
            List<Film> mostViewedFilms = new List<Film>();

            string searchPage = FilmUtils.getResponse(@"http://fs.to/video/films/");
            Regex reg = new Regex(pattern: @"<div\sclass=\""b-poster-new\s\"">\s*<a\shref=\""(.*?)\""\s.*>\s*<.*>\s*<.*>\s*<.*\surl\(\'(.*?)\'\)\"">\s*<.*>\s*<.*>\s*<.*>\s*<.*>\s*<.*>\s*<.*\"">(.*?)<\/span>");
            MatchCollection mCollection = reg.Matches(searchPage);

            Film film = new Film();

            foreach (Match matches in mCollection)
            {
                film.FilmUri = @"http://fs.to" + matches.Groups[1].Value;
                film.ImageUri = @"http:" + matches.Groups[2].Value;
                film.FilmName = matches.Groups[3].Value;

                searchPage = @"http://fs.to" + matches.Groups["filmUri"].Value;
                searchPage = FilmUtils.getResponse(searchPage);

                film = FilmUtils.SetInfoFromHtmlTable(film);

                mostViewedFilms.Add(film);
            }

            return mostViewedFilms;
        }

        //??????? ??????? ? ?????????????? ??????????? ? ??????
        static public Film SetInfoFromHtmlTable(Film film)
        {

            string searchPage = getResponse(film.FilmUri);

            film.Description = ParseDescription(searchPage);
            ParseRate(searchPage, ref film.PositiveRate, ref film.NegativeRate);

            Regex regexTable = new Regex(@"<td class=""m-item-info-td_type-short"">\s*\t*(?<genre>.*)\s*\t*[</td>?]?");
            MatchCollection mCollection = regexTable.Matches(searchPage);
            try
            {
                string[] info = {mCollection[0].Groups["genre"].Value, mCollection[1].Groups["genre"].Value,
                mCollection[2].Groups["genre"].Value, mCollection[3].Groups["genre"].Value,
                mCollection[4].Groups["genre"].Value};

                //????? "?????????" ??????? ??????? ????????
                film.Genre = ParseGenreOrYear(info[0]);
                film.Year = int.Parse(ParseGenreOrYear(info[1]));
                film.Countries = (ParseCountries(info[2]));
                film.Directors = ParseActors(info[3]);
                film.Actors = ParseActors(info[4]);
            }

            catch (ArgumentOutOfRangeException e)
            {
                film.Genre = "";
                film.Year = 0;
                film.Countries = "";
                film.Directors = "";
                film.Actors = "";
            }
            return film;
        }

        static string ParseDescription(string s)
        {
            string result = "";

            Regex r = new Regex(@"<p\sclass=\""item-decription\s(full|short)\"">(.*)<\/p>");
            Match m = r.Match(s);

            result += m.Groups[2].Value;

            result = result.Replace(@"<p class=""item - decription full"">", " ");

            return result;
        }

        static void ParseRate(string s, ref int positiveRate, ref int negativeRate)
        {
            Regex r = new Regex(@"<div\sclass=\""b-tab-item__vote-value\sm-tab-item__vote-value_type_yes\"">(.*)<\/div>\s*<div\sclass=\""b-tab-item__vote-value\sm-tab-item__vote-value_type_no\"">(.*)<\/div>");
            Match m = r.Match(s);

            positiveRate = int.Parse(m.Groups[1].Value);
            negativeRate = int.Parse(m.Groups[2].Value);
        }

        static string ParseGenreOrYear(string s)
        {
            Regex r = new Regex(@"tag""><span>(?<genre>.*?)</span></a>");
            MatchCollection m = r.Matches(s);

            string result = "";

            foreach (Match match in m)
            {
                result += match.Groups["genre"].Value + ", ";
            }
            return result.Remove(result.Length - 2, 2);
        }

        static string ParseCountries(string s)
        {
            Regex r = new Regex(@"\);""><[/?]span>&nbsp;(?<country>\w*)</span></a>", RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(s);

            string result = "";

            foreach (Match match in m)
            {
                result += match.Groups["country"].Value + ", ";
            }
            return result.Remove(result.Length - 2, 2);
        }

        static string ParseActors(string s)
        {
            Regex r = new Regex(@"name"">(?<name>.*?)</span></a></span>\s*", RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(s);

            string result = "";

            foreach (Match match in m)
            {
                result += match.Groups["name"].Value + ", ";
            }
            return result.Remove(result.Length - 2, 2);
        }


        public static string getResponse(string uri)
        {
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response =(HttpWebResponse) request.GetResponse();
            Stream resStream = response.GetResponseStream();
            int count = 0;
            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    sb.Append(Encoding.UTF8.GetString(buf, 0, count));
                }
            }
            while (count > 0);
            return sb.ToString();
        }
    }
}