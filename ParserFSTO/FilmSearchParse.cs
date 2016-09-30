using System.Collections.Generic;

namespace fsto.Parse
{
    public struct Film
    {
        public string FilmUri;
        public string FilmName;
        public string ImageUri;
        public string Genre;
        public int Year;
        public string Countries;
        public string Directors;
        public string Actors;
        public int PositiveRate;
        public int NegativeRate;
        public string Description;
    };

    public class FilmSearchParse
    {
        public List<Film> filmList;
        public List<Film> mostViewedFilms;
    }

}