using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FriendlyCheckers{
    [Serializable()]
    public class YoureFuckedException : System.Exception
    {
        public YoureFuckedException() : base() { }
        public YoureFuckedException(string message) : base(message) { }
        public YoureFuckedException(string message, System.Exception inner) : base(message, inner) { }
        // A constructor is needed for serialization when an 
        // exception propagates from a remoting server to the client.  
        protected YoureFuckedException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
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
         * some functions are duplicates, see members for details
         */
        private List removals; //check with caleb on object types!!
        private List additions;
        private int moveNumber;
        private string returncode = "\0";
        private int gameID;
        private int matchID;
        private string username;
        private string opponentname;
        private string server="http://localhost:8000";
        private string path = "/root";
        private static string loginSuccess = "42.1";
        private static string queueMatchSuccess = "42.2";
        
        public NetworkLogic(Move move, UserGame game){   // constructor for NetworkLogic object, querys server for data
            this.additions = move.getAdditions();
            this.removals = move.getRemovals();
            this.moveNumber=move.getMoveNumber();
            this.gameID = game.getGameID();
            this.matchID = game.getMatchID();
            this.username = game.getUsername();
            this.opponentname= game.getOpponentname();
            if (queryServer(move, position, win).Equals("42.01"))
            {
                writeToServer(move, position, win);
            }
            if (checkGameID(username) != gameID)
            {
                setGameID(username, gameID);
            }
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
        public bool login(string username, string password)
        {
            string serverpath = server + path + "?message=Login[&" + "Username=" + username + "&Password=" + password;
            string responseFromServer;
            Uri address;
            WebClient webclient;
            WebRequest request;
            HttpWebResponse response;
            Stream dataStream;
            StreamReader reader;
            try
            {
                address= new Uri(serverpath);
                webclient = new WebClient();
                request=webclient.GetWebRequest(address);
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(server + path + "?message=Login[&" + "Username=" + username + "&Password=" + password);
                response = (HttpWebResponse)request.GetResponseStream();
                dataStream = response.GetResponseStream();
                reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                if (responseFromServer.Equals(loginSuccess))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                //...
            }
            YoureFuckedException fucked = new YoureFuckedException("Get a new phone!");
            throw fucked;
            return false;
        }
        public bool queueMatch(string username, int gameID)
        {
            string serverpath = server + path + "?message=QueueMatch&" + "UserID=" + username + "&GameID=" + gameID.ToString;
            string responseFromServer;
            Uri address;
            WebClient webclient;
            WebRequest request;
            HttpWebResponse response;
            Stream dataStream;
            StreamReader reader;
            try
            {
                address = new Uri(serverpath);
                webclient = new WebClient();
                request = webclient.GetWebRequest(address);
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(server + path + "?message=Login[&" + "Username=" + username + "&Password=" + password);
                response = (HttpWebResponse)request.GetResponseStream();
                dataStream = response.GetResponseStream();
                reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                if (responseFromServer.Equals(queueMatchSuccess))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                //...
            }
            YoureFuckedException fucked = new YoureFuckedException("Get a new phone!");
            throw fucked;
            return false;
        }
        public void requestMatch(string username, int gameID, string opponentname)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)Webrequest.Create(server + path + "?message=RequestMatch&" + "Username=" + username + "&Password=" + password);
                HttpWebResponse response = (HttpWebResponse)request.GetResponseStream();
            }
            catch (Exception e)
            {
                //...
            }
        }
        public void pollRequest(string username) // called by Caleb's GameLogic to poll server, threads not necessary!
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)Webrequest.Create(server + path + "?message=PollRequest&" + "Username=" + username);
                HttpWebResponse response = (HttpWebResponse)request.GetResponseStream();
            }
            catch (Exception e)
            {
                //...
            }
        }
        public void pollMatch(string username, int gameID)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)Webrequest.Create(server + path + "?message=PollMatch&" + "Username=" + username + "&GameID=" + gameID.ToString);
                HttpWebResponse response = (HttpWebResponse)request.GetResponseStream();
            }
            catch (Exception e)
            {
                //...
            }
        }
    }
}
