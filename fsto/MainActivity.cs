using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Content;
using Android.Views;
using Android.Views.Animations;

using Square.Picasso;
using Java.Lang;
using System;

namespace fsto
{
    [Activity(Label = "FS.TO", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, GestureDetector.IOnGestureListener
    {
        private GestureDetector gestureDetector;
        LinearLayout SearchLayout;
        EditText ETSearch;
        public Button BSearch;
        LinearLayout RootLayout;
        ProgressDialog pl;

        Animation translateAnimForvard;
        Animation translateAnimBackvard;

        public bool OnDown(MotionEvent e)
        {
            return false;
        }


        //Жест для свайпа вниз и отработка анимации
        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            translateAnimForvard = new TranslateAnimation(0, 0, -90, 0);
            translateAnimBackvard = new TranslateAnimation(0, 0, 0, -90);
            translateAnimBackvard.Duration = 400;
            translateAnimForvard.Duration = 700;
            translateAnimForvard.FillAfter = true;
            translateAnimBackvard.FillAfter = true;
            SearchLayout.Visibility = ViewStates.Visible;
            SearchLayout.StartAnimation(translateAnimForvard);
            if (velocityY < 0)
            {
                SearchLayout.StartAnimation(translateAnimBackvard);
                SearchLayout.Visibility = ViewStates.Gone;
            }
            return true;
        }



        public void OnLongPress(MotionEvent e) { }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return false;
        }
        public void OnShowPress(MotionEvent e) { }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            gestureDetector.OnTouchEvent(e);
            return false;
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Toast.MakeText(this, "Swipe Down to SEARCH", ToastLength.Short).Show();

            gestureDetector = new GestureDetector(this);

            SetContentView(Resource.Layout.Main);

            pl = new ProgressDialog(this, 4);
            pl.SetTitle("Parsing Data...");
            pl.SetMessage("Please wait...");
            pl.Show();

            SearchLayout = FindViewById<LinearLayout>(Resource.Id.SearchLayout);
            SearchLayout.Visibility = ViewStates.Gone;
            ETSearch = FindViewById<EditText>(Resource.Id.ETSearch);
            BSearch = FindViewById<Button>(Resource.Id.button);
            RootLayout = FindViewById<LinearLayout>(Resource.Id.RootLayout);

            FilmSearchParse fsp = new FilmSearchParse();
            SwitchIndicator(true);
            System.Threading.Thread tr = new System.Threading.Thread(new ThreadStart(() => {
                fsp.mostViewedFilms = FilmUtils.DoParse("", mostPop: true).Result;
                RunOnUiThread(() => { SwitchIndicator(false); });
                RunOnUiThread(() => { ShowFilms(fsp.mostViewedFilms); });
            }));
            tr.Start();
            

            
            

            BSearch.Click += delegate
            {
                {
                    if (fsp.filmList != null)
                        fsp.filmList.Clear();
                    RootLayout.RemoveAllViews();

                    string searchRequest = ETSearch.Text.Replace(' ', '+');

                    SwitchIndicator(true);

                    System.Threading.Thread parsing = new System.Threading.Thread(new ThreadStart(() =>
                    {
                    fsp.filmList = FilmUtils.DoParse(@"http://fs.to/search.aspx?search=" + searchRequest).Result;
                    
                    RunOnUiThread(() => { SwitchIndicator(false);});
                    RunOnUiThread(() => { ShowFilms(fsp.filmList); });
                    }));
                    parsing.Start();
                    
                }
            };
            }     

        public void SwitchIndicator(bool on)
        {
            if (on)
                pl.Show();
            else
                pl.Hide();
        }

        

        //Построение layout для листа фильмов
        void ShowFilms(List<Film> filmList)
        {
            if (filmList != null)
            {
                foreach (Film o in filmList)
                {
                    LinearLayout filmCardLayout = new LinearLayout(this);
                    filmCardLayout.SetBackgroundColor(Color.ParseColor("#009688"));
                    filmCardLayout.Clickable = true;
                    filmCardLayout.Click += delegate
                    {
                        var filmPreview = new Intent(this, typeof(FilmPreview));

                        filmPreview.PutExtra("FilmUri", o.FilmUri);
                        filmPreview.PutExtra("FilmName", o.FilmName);
                        filmPreview.PutExtra("Genre", o.Genre);
                        filmPreview.PutExtra("Year", (int)o.Year);
                        filmPreview.PutExtra("Countries", o.Countries);
                        filmPreview.PutExtra("Directors", o.Directors);
                        filmPreview.PutExtra("Actors", o.Actors);
                        filmPreview.PutExtra("PositiveRate", o.PositiveRate);
                        filmPreview.PutExtra("NegativeRate", o.NegativeRate);
                        filmPreview.PutExtra("Description", o.Description);

                        StartActivity(filmPreview);
                    };
                    filmCardLayout.Elevation = 10;
                    filmCardLayout.SetPadding(10, 10, 10, 10);
                    LinearLayout.LayoutParams lpForFilmCardLayout = new LinearLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                    lpForFilmCardLayout.SetMargins(5, 7, 9, 7);
                    filmCardLayout.Orientation = Orientation.Horizontal;

                    ImageView img = new ImageView(this);
                    Picasso.With(this).Load(o.ImageUri).Into(img);
                    img.SetAdjustViewBounds(true);
                    img.SetScaleType(ImageView.ScaleType.CenterCrop);
                    filmCardLayout.AddView(img);

                    LinearLayout nameDescLayout = new LinearLayout(this);
                    var lpForNameDescLayout = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
                    lpForNameDescLayout.SetMargins(30, 0, 0, 0);
                    nameDescLayout.Orientation = Orientation.Vertical;

                    TextView filmName = new TextView(this);
                    var lpForFilmName = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
                    lpForFilmName.SetMargins(0, 0, 0, 0);
                    filmName.LayoutParameters = lpForFilmName;
                    filmName.Text = o.FilmName;
                    filmName.TextSize = 18;

                    TextView filmDesc = new TextView(this);
                    var lpForFilmDesc = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
                    lpForFilmDesc.SetMargins(0, 10, 0, 0);
                    if (o.Description.Length > 0)
                        filmDesc.Text = o.Description.Remove(50, o.Description.Length - 50) + "...";
                    filmDesc.SetTextColor(Color.LightGray);

                    nameDescLayout.AddView(filmName, lpForFilmName);
                    nameDescLayout.AddView(filmDesc, lpForFilmDesc);

                    filmCardLayout.AddView(nameDescLayout, lpForNameDescLayout);

                    RootLayout.AddView(filmCardLayout, lpForFilmCardLayout);
                }
            }
        }
    }
}
