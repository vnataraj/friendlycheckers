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
using System.Threading;
using System.Windows.Threading;
using Microsoft.Phone.Net.NetworkInformation;

namespace FriendlyCheckers{
    public class UnknownException : System.Exception { }
    public class LoginException : System.Exception { }
    public class QueueMatchException : System.Exception { }
    public class RequestMatchException : System.Exception { }
    public class PollMatchException : System.Exception { }
    public class PollRequestException : System.Exception { }
    public class WriteToServerException : System.Exception { }
    public class CheckUserException : System.Exception { }
    public class CreateUserException : System.Exception { }
    public class GetSaveDataException : System.Exception { }
    public class GetGameDataException : System.Exception { }
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
        private List<SaveData> saveData;
        private GameData gameData;

        private int moveNumber;
        private int gameID;
        private int matchID;
        private static int defaultTimeout = 60 * 1000;

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
        private static string getSaveDataSuccess = "42.9";
        private static string checkUserExistsSuccess = "42.10";
        private static string getGameDataSuccess = "42.12";
        private static string createUserSuccess = "42.13";
        private static string loginFailure = "666.1";
        private static string queueMatchFailure = "666.2";
        private static string requestMatchFailure = "666.3";
        private static string pollMatchFailure = "666.4";
        private static string writeToServerFailure = "666.5";
        private static string pollRequestFailure = "666.6";
        private static string acceptMatchFailure = "666.7";
        private static string getSaveDataFailure = "666.9";
        private static string checkUserFailure = "666.10";
        private static string getGameDataFailure = "666.12";
        private static string createUserFailure = "666.13";
        private static string unknownError = "666.666";

        private bool getLoginState;
        private bool getQueueState;
        private bool getRequestState;
        private bool getPollMatchState;
        private bool writeState;
        private bool getPollRequestState;
        private bool checkUserExistsState;
        private bool getSaveDataState;
        private bool getGameDataState;
        private bool communication;
        private bool getAcceptMatchState;
        private bool createUserState;
        private bool exists;
        private bool internetState;
        
        public NetworkLogic(){   // constructor for NetworkLogic object, querys server for data
            this.internetState = false;
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                this.internetState = true;
            }
            this.getQueueState = false;
            this.getLoginState = false;
            this.getRequestState = false;
            this.getPollRequestState = false;
            this.getPollMatchState = false;
            this.writeState = false;
            this.checkUserExistsState = false;
            this.communication=false;
            this.getGameDataState = false;
            this.getSaveDataState = false;
            this.getAcceptMatchState = false;
            this.createUserState = false;
            this.exists = false;
            saveData = new List<SaveData>();
           // this.opponentname= game.getOpponentName();
            // do startup stuff
        }
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        private void requestHandler(IAsyncResult result)
        {
            webreq = (HttpWebRequest)result.AsyncState;
            response = (HttpWebResponse)webreq.EndGetResponse(result);
            Stream dataStream = response.GetResponseStream();

            using (reader = new StreamReader(dataStream))
            {
                responseFromServer = reader.ReadToEnd();
                //TextBlockResults.Text = results; //-- on another thread!
                this.checker(responseFromServer);
                Deployment.Current.Dispatcher.BeginInvoke(() => {});
            }
            dataStream.Close();
            reader.Close();
            response.Close();
            allDone.Set();
            return;
        }
        private void sendHttpRequest(string serverpath)
        {
            request = (HttpWebRequest)WebRequest.Create(serverpath);
            request.BeginGetResponse(new AsyncCallback(requestHandler), request);
        }
        private void checker(string responseFromServer)
        {
            if (responseFromServer.Contains(queueMatchSuccess))
            {
                this.getQueueState = true;
                return;
            } 
            else if (responseFromServer.Contains(createUserSuccess))
            {
                this.createUserState=true;
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
            else if (responseFromServer.Contains(acceptMatchSuccess))
            {
                this.getAcceptMatchState=true;
                return;
            }
            else if (responseFromServer.Contains(pollRequestSuccess))
            {
                this.getRequestState = true;
                return;
            }
            else if (responseFromServer.Contains(checkUserExistsSuccess))
            {
                this.checkUserExistsState = true;
                System.Diagnostics.Debug.WriteLine("set user to exists");
                return;
            }
            else if(responseFromServer.Contains(getGameDataSuccess))
            {
                this.getGameDataState = true;
                return;
            }
            else if(responseFromServer.Contains(getSaveDataSuccess))
            {
                parseSaveData(responseFromServer);
                this.getSaveDataState = true;
                return;
            }
            else if (responseFromServer.Contains(loginSuccess))
            {
                System.Diagnostics.Debug.WriteLine("bbbbb");
                this.getLoginState = true;
                return;
            }
            else if (responseFromServer.Contains(checkUserFailure))
            {
                this.checkUserExistsState = false;
                return;
            }
            else if(responseFromServer.Contains(getGameDataFailure))
            {
                this.getGameDataState=false;
                return;
            }
            else if (responseFromServer.Contains(createUserFailure))
            {
                if(responseFromServer.Contains("exists"))
                {
                    this.exists=true;
                }
                this.createUserState=false;
                return;
            }
            else if (responseFromServer.Contains(loginFailure))
            {
                this.getLoginState = false;
                return;
            }
            else if (responseFromServer.Contains(queueMatchFailure))
            {
                this.getQueueState = false;
                return;
            }
            else if (responseFromServer.Contains(requestMatchFailure))
            {
                this.getRequestState = false;
                return;
            }
            else if (responseFromServer.Contains(pollMatchFailure))
            {
                this.getPollMatchState = false;
                return;
            }
            else if (responseFromServer.Contains(writeToServerFailure))
            {
                this.writeState = false;
                return;
            }
            else if (responseFromServer.Contains(pollRequestFailure))
            {
                this.getPollRequestState = false;
                return;
            }
            else if(responseFromServer.Contains(getSaveDataFailure))
            {
                this.getSaveDataState=false;
                return;
            }
            return;
        }
        private void createUrl(List<Piece> removals, List<Piece> additions)
        {
            
        }
        private string encodeHttp(string b)
        {
            string a;
            a = HttpUtility.UrlEncode(b);
            return a;
        }
        private void parseSaveData(string a)
        {
            string[] lines = a.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            PieceColor p, whoseMove;
            for (int i = 1; i < lines.Length; i++)
            {
                string[] finished = lines[i].Split(new string[] { " " }, StringSplitOptions.None);
                if (finished[0].Equals("")) return;

                p = finished[3].Equals("BLACK") ? PieceColor.BLACK : PieceColor.RED;
                whoseMove = finished[4].Equals("BLACK") ? PieceColor.BLACK : PieceColor.RED;

                saveData.Add(new SaveData(Convert.ToInt32(finished[0]), finished[1], Convert.ToInt32(finished[2]), p, whoseMove));
            }
            return;
        }
        public void checkUser(string username) //should be void
        {
            
            serverpath = server + "?message=CheckUser&" + "Username=" + username;
            try
            {
                sendHttpRequest(serverpath);
            }
            catch (WebException e)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception : " + e.ToString());  //...
                CheckUserException cue = new CheckUserException();
                throw cue;
            }
            return;
/*            if (this.checkUserExistsState)
            {
                System.Diagnostics.Debug.WriteLine("USER EXISTS is true");
                this.checkUserExistsState = false;
                return true;
            }
            return false;
 */
        }
        public void getSaveData(string username)
        {
            serverpath = server + "?message=GetSaveData&" + "Username=" + username;
            try
            {
                sendHttpRequest(serverpath);
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception : " + we.ToString());
                GetSaveDataException gsde = new GetSaveDataException();
                throw gsde;
            }
            return;
        }
        public void getGameData(string username, int matchID)
        {
            serverpath = server + "?message=GetGameData&" + "Username=" + username + "&MatchID=" + matchID.ToString();
            try
            {
                sendHttpRequest(serverpath);
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception : " + we.ToString());
                GetGameDataException ggde = new GetGameDataException();
                throw ggde;
            }
        }
        public void writeToServer(string username, SaveData saveData, GameData gameData)
        {
            string str = gameData.ToString();
            serverpath = server + "?message=RecordMove&" + "Username=" + username + "&MatchID" + saveData.getMatchID().ToString() + "&MoveNumber=" + saveData.getNumMoves().ToString()+"&Notation="+encodeHttp(str); // parse move!!!
            try
            {
                sendHttpRequest(serverpath);
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
            return;
        }

        public void login(string username, string password)// should be void
        {
            serverpath=server + "?message=Login&" + "Username=" + username + "&Password=" + password;
            try
            {
                sendHttpRequest(serverpath);
            }
            catch (WebException e)
            {
                Console.WriteLine("Caught exception : "+e.ToString());  //...
                LoginException le = new LoginException();
                throw le;
            }
            return;
        }
        private string user;
        public void createUser(string username, string password)
        {
            if (createUserState && user!=null && user.Equals(username))
            {
                createUserState = false;
                return;
            }
            user = username;
            serverpath = server + "?message=CreateUser&" + "Username=" + username + "&Password=" + password;
            try
            {
                sendHttpRequest(serverpath);
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception : " + we.ToString());
            }
            return;
        }
        public void queueMatch(string username, int gameID)
        {
            string serverpath = server + "?message=QueueMatch&" + "UserID=" + username + "&GameID="+gameID.ToString();
            try
            {
                sendHttpRequest(serverpath);
            } 
            catch (WebException e)
            {
                Console.WriteLine("Caught error :" + e.ToString());  //...
                QueueMatchException qe = new QueueMatchException();
                throw qe;
            }
            return;
        }

        public void requestMatch(string username, int gameID, string opponentName)
        {
            this.getRequestState = false;
            serverpath = server + "?message=RequestMatch&" + "Username=" + username + "&GameID=" + gameID.ToString() + "&Opponentname="+opponentName;
            try
            {
                sendHttpRequest(serverpath);
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
            return;
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
        public bool getCheckUserState()
        {
            return checkUserExistsState;
        }
        private void setCommunicationState(bool b)
        {
            this.communication = b;
        }
        public bool getGetLoginState()
        {
            return getLoginState;
        }
        public bool getGetRequestState()
        {
            return getRequestState;
        }
        public bool getQueueMatchState()
        {
            return getQueueState;
        }
        public bool getGetSaveDataState()
        {
            return getSaveDataState;
        }
        public bool getGetGameDataState()
        {
            return getGameDataState;
        }
        public bool getGetAcceptMatchState()
        {
            return getAcceptMatchState;
        }
        public bool getCreateUserState()
        {
            return createUserState;
        }
        public bool getInternetState()
        {
            return internetState;
        }
        public List<SaveData> getGetSaveData()
        {
            if (getSaveDataState)
            {
                this.getSaveDataState = false;
                return saveData;
            }
            return saveData;
        }
    }
}
