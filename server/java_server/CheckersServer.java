import java.util.*;
import java.io.*;
import java.net.*;

public class CheckersServer {

	private Server server;
	
	public CheckersServer(int port) {
		server = new Server(port);
	}
	
	public void run() {
		NetworkConnection n;
		boolean finished = false;
		while(!finished) {
			
			n = server.accept();
			processConnection(n);
		
		}
	
	}
	
	public void processConnection(NetworkConnection n) {
		System.out.println("-- Starting processing --");
		String response = "666.666";
		try {
			String in = n.readLine();
			System.out.println(in);
		
			String message = extractMessage(in);
			String[] varsArray = extractVars(in);
			System.out.println("Message: " + message);
			System.out.println("Vars:");
			for(String v : varsArray) {
				System.out.println("\t" + v);
			}
			
			response = getHTML("http://checkers.nne.net" + in + "&Client=Passthrough");
			/*
			if("CheckUser".equals(message)) {
				response = processCheckUser(varsArray);
			}
			else if("Login".equals(message)) {
				response = processLogin(varsArray);
			}
			else if("RecordMove".equals(message)) {
				response = processRecordMove(varsArray);
			}
			else if("GetSaveData".equals(message)) {
				response = processGetSaveData(varsArray);
			}
			else if("GetGameData".equals(message)) {
				response = processGetGameData(varsArray);
			}
			else if("CreateUser".equals(message)) {
				response = processCreateUser(varsArray);
			}
			*/
		}
		catch(Exception e) {
			e.printStackTrace();
		}
		finally {
			System.out.println("Responding with: " + response);
			n.println(response);
			n.close();
		}
		
		System.out.println("-- Ended processing --");
	}
	
	
	private String getHTML(String urlToRead) {
      URL url;
      HttpURLConnection conn;
      BufferedReader rd;
      String line;
      String result = "";
      try {
         url = new URL(urlToRead);
         conn = (HttpURLConnection) url.openConnection();
         conn.setRequestMethod("GET");
         rd = new BufferedReader(new InputStreamReader(conn.getInputStream()));
         while ((line = rd.readLine()) != null) {
            result += line + "\n";
         }
         rd.close();
      } catch (Exception e) {
         e.printStackTrace();
      }
      return result;
   }
	
	private String processCheckUser(String[] vars) {
		if(! varExists(vars, "Username") || varEmpty(vars, "Username")) {
			return "666.10\nUsername not supplied";
		}
		return "666.10\nError";
	}
	
	private String processLogin(String[] vars) {
		return "42.10";
	}
	
	private String processRecordMove(String[] vars) {
		return "42.10";
	}
	
	private String processGetSaveData(String[] vars) {
		return "42.10";
	}
	
	private String processGetGameData(String[] vars) {
		return "42.10";
	}
	
	private String processCreateUser(String[] vars) {
		return "42.10";
	}
	
	private String extractMessage(String in) {
		if(in == null) 
			return "";
		if(in.indexOf("&") > 0)
			return in.substring(9, in.indexOf("&"));
		else
			return in.substring(9);
	}
	
	private String[] extractVars(String in) {
		String vars = in.substring(in.indexOf("&") + 1);
		String[] varsArray = vars.split("&");
		return varsArray;
	}
	
	private boolean varExists(String[] vA, String k) {
		for(String var : vA) {
			if(var.substring(0, var.indexOf("=")).equals(k))
				return true;
		}
		return false;
	}
	
	private String var(String[] vA, String k) {
		for(String var : vA) {
			if(var.substring(0, var.indexOf("=")).equals(k))
				return var.substring(var.indexOf("=") + 1);
		}
		return "";
	}
	
	private boolean varEmpty(String[] vA, String k) {
		if(! varExists(vA, k) ) return true;
		return "".equals(var(vA, k));
	}
	
	
	public static void main(String[] args) {
		for(String s : args) {
			System.out.println(s);
		}
		int port = Integer.parseInt(args[0]);
		
		CheckersServer s = new CheckersServer(port);
		s.run();
	}
	
}