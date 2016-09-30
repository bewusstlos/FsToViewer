using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Square.Picasso;
using fsto.Parse;

namespace fsto
{
    [Activity(Label = "FS.TO", MainLauncher = false, Icon = "@drawable/icon")]
    class FilmPreview : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FilmPreview);

            var IVFilmImage = FindViewById<ImageView>(Resource.Id.FilmImage);
            var TVFilmName = FindViewById<TextView>(Resource.Id.FilmName);
            var TVGenre = FindViewById<TextView>(Resource.Id.Genre);
            var TVYear = FindViewById<TextView>(Resource.Id.Year);
            var TVCountry = FindViewById<TextView>(Resource.Id.Country);
            var TVDirectors = FindViewById<TextView>(Resource.Id.Directors);
            var TVActors = FindViewById<TextView>(Resource.Id.Actors);
            var TVRate = FindViewById<RelativeLayout>(Resource.Id.TVRate);
            var PBRate = FindViewById<ProgressBar>(Resource.Id.Rate);
            var TVDescription = FindViewById<TextView>(Resource.Id.Description);
            var BPlayVideo = new Button(this);
            BPlayVideo.Text = "PLAY TRAILER";

            string filmUri = Intent.GetStringExtra("FilmUri");
            string filmName = Intent.GetStringExtra("FilmName");
            string imageUri = ParseImage(filmUri);
            string genre = Intent.GetStringExtra("Genre");
            int positiveRate = Intent.GetIntExtra("PositiveRate", 0);
            int negativeRate = Intent.GetIntExtra("NegativeRate", 0);
            string description = Intent.GetStringExtra("Description");

            Picasso.With(this).Load(imageUri).Into(IVFilmImage);
            TVFilmName.Text = filmName;
            TVGenre.Text = "Жанры: " + genre;
            TVYear.Text = "Год: " + Intent.GetIntExtra("Year", 0);
            TVCountry.Text = "Страна: " + Intent.GetStringExtra("Countries");
            TVDirectors.Text = "Режисер: " + Intent.GetStringExtra("Directors");
            TVActors.Text = "Актеры: " + Intent.GetStringExtra("Actors");
            float progress = ((float)positiveRate / (positiveRate + negativeRate)) * 100F;
            PBRate.Progress = (int)progress;
            TVDescription.Text = description;


            this.ActionBar.SetCustomView(BPlayVideo, new ActionBar.LayoutParams(GravityFlags.Right));
            this.ActionBar.DisplayOptions = ActionBarDisplayOptions.ShowCustom;

            TextView TVPositiveRate = new TextView(this);
            TVPositiveRate.SetTextColor(Color.Green);
            RelativeLayout.LayoutParams lpForPositiveRate = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            TVPositiveRate.Text = positiveRate.ToString();

            TextView TVNegativeRate = new TextView(this);
            TVNegativeRate.SetTextColor(Color.Red);
            RelativeLayout.LayoutParams lpForNegativeRate = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            lpForNegativeRate.AddRule(LayoutRules.AlignParentRight);
            TVNegativeRate.Text = negativeRate.ToString();

            TVRate.AddView(TVPositiveRate, lpForPositiveRate);
            TVRate.AddView(TVNegativeRate, lpForNegativeRate);


            string videoFile = ParseVideos(filmUri);

            BPlayVideo.Click += delegate
            {
                var filmWatch = new Intent();
                filmWatch.SetType("video/*");
                //filmWatch.SetAction(Intent.ActionGetContent);
                filmWatch.SetDataAndType(Android.Net.Uri.Parse(videoFile), "video/*");
                //filmWatch.PutExtra("VideoUri", videoFile);
                StartActivity(filmWatch);
            };
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok && requestCode == 1)
            {
                var selectedVideo = Intent.GetStringExtra("VideoUri");
                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(Android.Net.Uri.Parse(selectedVideo), "video/*");
                StartActivity(Intent.CreateChooser(intent, "Complete Action using"));
            }
        }

        string ParseImage(string filmUri)
        {
            string searchPage = FilmUtils.getResponse(filmUri);

            Regex r = new Regex(@"<link rel=""image_src"" href=""(?<ImageUri>.*)"" />");
            Match m = r.Match(searchPage);
            return m.Groups["ImageUri"].Value;
        }

        string ParseVideos(string filmUri)
        {
            string searchPage = FilmUtils.getResponse(filmUri);

            Regex r = new Regex(@"<a href=""(?<FilmOnline>.*?)"" class=""b-button"" rel=""nofollow"">\s*<span class=""sliding"">");
            Match m = r.Match(searchPage);

            searchPage = FilmUtils.getResponse(@"http://fs.to" + m.Groups["FilmOnline"].Value + @"?play&file");

            Regex regFile = new Regex(@"<meta property=""og:video"" content=""(?<VideoFile>.*?)"" />");
            Match match = regFile.Match(searchPage);

            return @"http:" + match.Groups["VideoFile"].Value;
        }
    }
}