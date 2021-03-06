using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Print.Pdf;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
/***
*EventProfile.cs Activity: 
*
* 
* OnCreate: Initializes and sets activity view to display lists, buttons and textviews. 
*
* BtnETDelete_Click: Event handler which makes a delete request to delete event.
*
* BtnETSave_Click: Not implemented 
*
* MakeGetRequest: Receives data from web api through http get request and returns the content received. 
* 
* SetUpMap: Sets up the map view to display in activity.
* 
* OnMapReady: Handles all of the pin posting and markers sent to the map along with handlers for marker options
* 
* OnMapLongClick: not implemented.
* 
* MakeDeleteRequest: Sets the http delete request sent to the web api.
*
***/

namespace Login
{
    [Activity(Label = "EventProfile")]
    public class EventProfile : Activity, IOnMapReadyCallback, GoogleMap.IOnMapLongClickListener
    {
        // initialize variables 
        private TextView tvEPError;
        private static string eventId;
        private static string AccessToken;
        private static string userName;
        Event requestedEvent;
        private GoogleMap epMap;
        private EditText etEPAddress1;
        private EditText etEPAddress2;
        private EditText etEPCity;
        private EditText etEPState;
        private EditText etEPPostal;
        private EditText etEPEventName;
        private EditText etEPDetails;
        private EditText etEPDateTime;
        private EditText etEPLongitude;
        private EditText etEPLatitude;
        private EditText etEPEventHost;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.EventProfile);
            

            // set views
            tvEPError = (TextView) FindViewById(Resource.Id.tvEPError);
            etEPAddress1 = (EditText)FindViewById(Resource.Id.etEPAddress1);
            etEPAddress2 = (EditText)FindViewById(Resource.Id.etEPAddress2);
            etEPCity = (EditText)FindViewById(Resource.Id.etEPCity);
            etEPState = (EditText)FindViewById(Resource.Id.etEPState);
            etEPPostal = (EditText)FindViewById(Resource.Id.etEPPostal);
            etEPEventName = (EditText) FindViewById(Resource.Id.etEPEventName);
            etEPDetails = (EditText) FindViewById(Resource.Id.etEPDetails);
            etEPDateTime = (EditText) FindViewById(Resource.Id.etEPDateTime);
            etEPLongitude = (EditText) FindViewById(Resource.Id.etEPLongitude);
            etEPLatitude = (EditText) FindViewById(Resource.Id.etEPLatitude);
            etEPEventHost = (EditText) FindViewById(Resource.Id.etEPEventHost);

            // wire up buttons 
            Button btnETSave = (Button)FindViewById(Resource.Id.btnETSave);
            btnETSave.Click += BtnETSave_Click;
            Button btnETDelete = (Button)FindViewById(Resource.Id.btnETDelete);
            btnETDelete.Click += BtnETDelete_Click;

            // get the intents data
            eventId = Intent.GetStringExtra("eventId");
            AccessToken = Intent.GetStringExtra("token");
            userName = Intent.GetStringExtra("userName");

            // form url
            string url = GetString(Resource.String.IP) + "api/events/" + eventId;

            // get the details about the event
            try
            {
                string response = await MakeGetRequest(url);
                requestedEvent = JsonConvert.DeserializeObject<Event>(response);
            }
            catch
            {
                tvEPError.Text = "ERROR GETTING EVENT DETAILS";
            }

            //present the data
            etEPAddress1.Text = requestedEvent.Address1;
            etEPAddress2.Text = requestedEvent.Address1;
            etEPCity.Text = requestedEvent.City;
            etEPState.Text = requestedEvent.State;
            etEPPostal.Text = requestedEvent.PostalCode;
            etEPEventName.Text = requestedEvent.Name;
            etEPDetails.Text = requestedEvent.Details;
            etEPDateTime.Text = requestedEvent.EventDateTime;
            etEPLongitude.Text = requestedEvent.Longitude.ToString();
            etEPLatitude.Text = requestedEvent.Latitude.ToString();
            etEPEventHost.Text = requestedEvent.Username;

            //if this is the owner of the event, changes are allowed
            if (requestedEvent.Username != userName)
            {
                etEPAddress1.Focusable = false;
                etEPAddress2.Focusable = false;
                etEPCity.Focusable = false;
                etEPState.Focusable = false;
                etEPPostal.Focusable = false;
                etEPDetails.Focusable = false;
                etEPDateTime.Focusable = false;
                etEPLongitude.Focusable = false;
                etEPLatitude.Focusable = false;
                etEPEventHost.Focusable = false;
                etEPEventName.Focusable = false;

                // wire up buttons 
                btnETSave.Visibility = ViewStates.Gone;
                btnETDelete.Visibility = ViewStates.Gone;
                
            }

            SetUpMap();
        }

        private async void BtnETDelete_Click(object sender, EventArgs e)
        {
            // set url to delete event 
            string deleteUrl = GetString(Resource.String.IP) + "api/Events/" + eventId;
      
            try
            {
                // make delete request to web api 
                string response = await MakeDeleteRequest(deleteUrl, true);
                Toast.MakeText(this, "Event Deleted", ToastLength.Short).Show();
                Finish();
            }
            catch
            {
                tvEPError.Text = "ERROR IN DELETING";
            }
          
        }

        private void BtnETSave_Click(object sender, EventArgs e)
        {
            // no implemented functionality 
        }

        public static async Task<string> MakeGetRequest(string url)
        {
            //initialize http request and set method to a get request 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            //initialize variables to stream data from web api
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            respStream.Flush();

            // read content that was returned from the get request
            using (StreamReader sr = new StreamReader(respStream))
            {
                //Need to return this response 
                string strContent = sr.ReadToEnd();
                respStream = null;
                return strContent;
            }
        }

        private void SetUpMap()
        {
            // if there is no map set fragmet to display map specified in OnMapReady
            if (epMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.epMap).GetMapAsync(this);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            // initialize map and set camera to lattitude location 
            epMap = googleMap;     
            LatLng latlng = new LatLng(requestedEvent.Latitude, requestedEvent.Longitude);
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 15);
            epMap.MoveCamera(camera);

            //place a pin where the event is located
            MarkerOptions options = new MarkerOptions().SetPosition(latlng).
                SetTitle(requestedEvent.Name).SetSnippet(requestedEvent.Details);

            if (requestedEvent.Username == userName)
            {
                options.Draggable(true); 
            }

            epMap.AddMarker(options);
            //epMap.SetOnMapLongClickListener(this);
        }

        public void OnMapLongClick(LatLng point)
        {
            // not implemented 
        }

        public async Task<string> MakeDeleteRequest(string url, bool isJson)
        {
            // initialize http request and set url to passed string 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            // set http method to delete 
            request.Method = "DELETE";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
            var stream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(stream))
            {
                
                writer.Flush();
                writer.Dispose();
            }

            // read content that was returned from the get request
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();

            using (StreamReader sr = new StreamReader(respStream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
