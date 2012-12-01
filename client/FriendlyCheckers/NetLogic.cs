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
    public class UnknownException : System.Exception { }
    public class LoginException : System.Exception { }
    public class QueueMatchException : System.Exception { }
    public class RequestMatchException : System.Exception { }
    public class PollMatchException : System.Exception { }
    public class PollRequestException : System.Exception { }
    public class WriteToServerException : System.Exception { }
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
        private int gameID;
        private int matchID;

        private string username;
        private string opponentname;
        private string server="http://checkers.nne.net/";
        private string serverpath;
        private string responseFromServer;
        private string returncode = "\0";
        private static string loginSuccess = "42.1";
        private static string queueMatchSuccess = "42.2";
        private static string requestMatchSuccess = "42.3";
        private static string pollMatchSuccess = "42.4";
        private static string writeToServerSuccess = "42.5";
        private static string pollRequestSuccess = "42.6";
        private static string acceptMatchSuccess="42.7";
        private static string pollMatchNoMoveSuccess="42.8";
        private static string loginFailure = "666.1";
        private static string queueMatchFailure = "666.2";
        private static string requestMatchFailure = "666.3";
        private static string pollMatchFailure = "666.4";
        private static string writeToServerFailure = "666.5";
        private static string pollRequestFailure = "666.6"; 

        private bool getLoginState;
        private bool getQueueState;
        private bool getRequestState;
        private bool getPollMatchState;
        private bool writeState;
        private bool getPollRequestState;
        
        public NetworkLogic(){   // constructor for NetworkLogic object, querys server for data
            this.getQueueState = false;
            this.getLoginState = false;
            this.getRequestState = false;
            this.getPollRequestState = false;
            this.getPollMatchState = false;
            this.writeState = false;
           // this.opponentname= game.getOpponentName();
            // do startup stuff
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
            this.checker(responseFromServer);
            return;
        }
        private void checker(string responseFromServer)
        {
            if (responseFromServer.Contains(loginSuccess))
            {
                this.getLoginState = true;
                return;
            }
            else if (responseFromServer.Contains(queueMatchSuccess))
            {
                this.getQueueState = true;
                return;
            } 
            else if (responseFromServer.Contains(requestMatchSuccess))
            {
                this.getRequestState = true;
                return;
            } 
            else if (responseFromServer.Contains(pollMatchSuccess))
            {
                this.getPollMatchState = true;
                return;
            } 
            else if (responseFromServer.Contains(writeToServerSuccess))
            {
                this.writeState = true;
                return;
            } 
            else if (responseFromServer.Contains(pollRequestSuccess))
            {
                this.getRequestState = true;
                return;
            }
            else if (responseFromServer.Contains(loginFailure))
            {
                return;
            }
            else if (responseFromServer.Contains(queueMatchFailure))
            {
                return;
            }
            else if (responseFromServer.Contains(requestMatchFailure))
            {
                return;
            }
            else if (responseFromServer.Contains(pollMatchFailure))
            {
                return;
            }
            else if (responseFromServer.Contains(writeToServerFailure))
            {
                return;
            }
            else if (responseFromServer.Contains(pollRequestFailure))
            {
                return;
            }
            return;
        }
        private void createUrl(List<Piece> removals, List<Piece> additions)
        {

        }
        public bool writeToServer(string username, int gameID, int matchID, Move move)
        {
            serverpath = server + "?message=RequestMatch&" + "Username=" + username + "&GameID=" + gameID.ToString() + "&MatchID="+matchID.ToString()+"&Move="+move.ToString(); // parse move!!!
            try
            {
                request = (HttpWebRequest)WebRequest.Create(serverpath);
                request.BeginGetResponse(new AsyncCallback(requestHandler), request);
            }
            catch (WebException we)
            {
                we.ToString();
                  //...
                WriteToServerException wtse = new WriteToServerException();
                throw wtse;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception : " + e.ToString()); // debug
            }
            if (this.getRequestState)
            {
                this.getRequestState = false;
                return true;
            }
            return false;
        }

        public void parseUserGame(UserGame game)
        {

        }
        public bool login(string username, string password)
        {

            serverpath=server + "?message=Login&" + "Username=" + username + "&Password=" + password;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(serverpath);
                request.BeginGetResponse(new AsyncCallback(requestHandler), request);
               }
            catch (WebException e)
            {
                Console.WriteLine("Caught exception : "+e.ToString());  //...
                LoginException le = new LoginException();
                throw le;
            }
            if (this.getLoginState)
            {
                this.getLoginState = false;
                return true;
            }
            return false;
        }
        public bool queueMatch(string username, int gameID)
        {
            string serverpath = server + "?message=QueueMatch&" + "UserID=" + username + "&GameID="+gameID.ToString();
            try
            {
                request = (HttpWebRequest)WebRequest.Create(serverpath);
                request.BeginGetResponse(new AsyncCallback(requestHandler), request);
                dataStream.Close();
                response.Close();
            } 
            catch (WebException e)
            {
                Console.WriteLine("Caught error :" + e.ToString());  //...
                QueueMatchException qe = new QueueMatchException();
                throw qe;
            }
            if (this.getQueueState)
            {
                this.getQueueState = false;
                return true;
            }
            return false;
            
        }

        public bool requestMatch(string username, int gameID, string opponentName)
        {
            serverpath = server + "?message=RequestMatch&" + "Username=" + username + "&GameID=" + gameID.ToString() + "&Opponentname="+opponentName;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(serverpath);
                request.BeginGetResponse(new AsyncCallback(requestHandler), request);
            }
            catch (WebException we)
            {
                we.ToString();
                  //...
                RequestMatchException rme = new RequestMatchException();
                throw rme;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception : " + e.ToString()); // debug
            }
            if (this.getRequestState)
            {
                this.getRequestState = false;
                return true;
            }
            return false;
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
        public void pollMatch(string username, int gameID, int matchID)
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
        public bool checkUser(string username);
    }
}
