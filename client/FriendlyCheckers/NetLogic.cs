using System;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace FriendlyCheckers{
    public class YoureFuckedException : System.Exception {  }
    public class WebClientEx : WebClient
    {
        public WebRequest webreq;
        public WebClientEx(Uri address)
        {
            webreq = this.GetWebRequest(address);
        }

        private readonly CookieContainer container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
            }
        }
    }

    public class NetworkLogic
    {

        /*
         * @vnataraj, CS252
         * ideas: to create a Network Logic object that can be called by any of the other objects(aka Caleb's GameLogic object and Joe's GUI)
         * basic concept idea is to create a service on the windows phone that calls a while loop every x seconds(defined in phone prefs)
         * provide getter/setter methods in case manual getting/setting is required
         * relay between all C# and server functions, no functions should be parsed from server directly in other objects
         * codes are 42.xx on success and 666.xx on failure
         * 
         * 
         * 
         * UPDATE: 11/28/12 need to fix request handler, only currently handles queuematch requests(need to handle all types)-> how to do it??
         */
        private List<Piece> removals; //check with caleb on object types!!
        private List<Piece> additions;
        private HttpWebRequest webreq;
        private HttpWebRequest request;
        private HttpWebResponse response;
        private Stream dataStream;
        private StreamReader reader;

        private int moveNumber;
        private string returncode = "\0";
        private int gameID;
        private int matchID;
        private string username;
        private string opponentname;
        private string server="http://localhost:8000";
        private string serverpath;
        private string responseFromServer;
        private string path = "/root";
        private static string loginSuccess = "42.1";
        private static string queueMatchSuccess = "42.2";

        public bool getLoginState;
        
        public NetworkLogic(Move move, UserGame game){   // constructor for NetworkLogic object, querys server for data
            this.additions = move.getAdditions();
            this.removals = move.getRemovals();
            this.moveNumber=move.getMoveNumber();
            this.gameID = game.getGameID();
            this.matchID = game.getMatchID();
            this.username = game.getUsername();
           // this.opponentname= game.getOpponentName();
            // do startup stuff
        }

        public string queryServer(int move, int position)
        {
            //call PHP server for last known moves
            //return String (returncode) with either 42.xx for success or 666.xx for failure 
            return returncode;
        }
        public void writeToServer(int move, int position)
        {
            // writes information to server after querying when server was last modified
            //error codes still apply(42.xx for success, 666.xx for failure)
        }
        public void login(string username, string password)
        {

            serverpath=server + path + "?message=Login[&" + "Username=" + username + "&Password=" + password;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(serverpath);
                request.BeginGetResponse(new AsyncCallback(requestHandler), request);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception : "+e.ToString());  //...
            }
        }
        public void queueMatch(string username, int gameID)
        {
            string serverpath = server + path + "?message=QueueMatch&" + "UserID=" + username + "&GameID="+gameID.ToString();
            try
            {
                request = (HttpWebRequest)WebRequest.Create(serverpath);
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(server + path + "?message=Login[&" + "Username=" + username + "&Password=" + password);
                request.BeginGetResponse(new AsyncCallback(requestHandler), request);
                //dataStream = response.GetResponseStream();
               // reader = new StreamReader(dataStream);
               // responseFromServer = reader.ReadToEnd();
              //  reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught error :" + e.ToString());  //...
            }
        }
        private void requestHandler(IAsyncResult result)
        {
            webreq = (HttpWebRequest)result.AsyncState;
            response = (HttpWebResponse)webreq.EndGetResponse(result);
            Stream dataStream = response.GetResponseStream();
            reader = new StreamReader(dataStream);
            responseFromServer = reader.ReadToEnd();
            dataStream.Close();
            reader.Close();
            response.Close();
            if (responseFromServer.Equals(loginSuccess))
            {
                this.getLoginState = true;
            }
        }
        public void requestMatch(string username, int gameID, string opponentname)
        {
           /* try
            {
                //HttpWebRequest request = (HttpWebRequest)Webrequest.Create(server + path + "?message=RequestMatch&" + "Username=" + username + "&Password=" + password);
                //HttpWebResponse response = (HttpWebResponse)request.GetResponseStream();
            }
            catch (Exception e)
            {
                //...
            }
            */
        }
        public void pollRequest(string username) // called by Caleb's GameLogic to poll server, threads not necessary!
        {
            /*
            try
            {
                //HttpWebRequest request = (HttpWebRequest)Webrequest.Create(server + path + "?message=PollRequest&" + "Username=" + username);
                //HttpWebResponse response = (HttpWebResponse)request.GetResponseStream();
            }
            catch (Exception e)
            {
                //...
            }
             */
        }
        public void pollMatch(string username, int gameID)
        {
            /*
            try
            {
                HttpWebRequest request = (HttpWebRequest)Webrequest.Create(server + path + "?message=PollMatch&" + "Username=" + username + "&GameID=" + gameID.ToString);
                HttpWebResponse response = (HttpWebResponse)request.GetResponseStream();
            }
            catch (Exception e)
            {
                //...
            }
             */
        }
    }
}
