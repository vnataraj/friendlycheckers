<?php
/*
 *	index.php
 *	
 *	Main landing page for Checkers requests
 *
 *	
 */
 	error_reporting(E_ALL);
 	ini_set('display_errors', 'On');
 	
	abstract class Message {
		
		protected $name;
		public $code;
		public $data;
		
		public function __construct($name) {
			$this->name = $name;
		}
		
		abstract function process($input);
		
		public function respond() {
			// If $this->code is Success (starts with 42)
			if(strpos($this->code, '42') === 0) {
				$this->respondSuccess();
			}
			// If $this->code is Failure (starts with 666)
			else if(strpos($this->code, '666') === 0) {
				$this->respondFailure();
			}
			// Otherwise set error code to 666.666 (Unknown error) and respond
			else {
				$this->setCode('Unknown Error'); // Unknown error;
				//if(empty($this->data)) $this->setData('Unknown Error');
				$this->respondFailure();
			}
		}
		
		protected function setCode($type) {
			$codes = array(
				'Unknown Error' => 	'666.666',
				'Login Success'	=>	'42.1',
				'Login Failure'	=>	'666.1',
				'RecordMove Success' => '42.5',
				'RecordMove Failure' => '666.5',
				'PollMatch Success' => '42.4',
				'PollMatch Success No Move Data', '42.8',
				'CheckUser UsernameDoesNotExist' => '666.4',
				'QueueMatch Success' => '42.2',
				'QueueMatch Failure' => '666.2',
				'CheckUser Success' => '42.10',
				'CheckUser Failure' => '666.10',
				'GetSaveData Failure' => '666.9',
				'GetSaveData Success' => '42.9',
				'GetGameData Success' => '42.12',
				'GetGameData Failure' => '666.12',
				'CreateUser Failure' => '666.13',
				'CreateUser Success' => '42.13',
				'RecordWinner Success'=>'42.14',
				'RecordWinner Failure'=>'666.14'
			);
			
			$this->code = $codes[$type];
		}
		
		public function setData($d) {
			$this->data = $d;
		}
		
		
		protected function respondSuccess() {
			$this->respondGeneric('Success');
		}
		
		protected function respondFailure() {
			$this->respondGeneric('Failure');
		}
		
		protected function respondGeneric($responseHeader) {
			$response 	=	'';
			//$response	.=	$responseHeader;
			//$response	.=	"\n";
			$response	.=	$this->code;
			$response	.=	"\n";
			$response	.=	$this->data;
			$response	.=	"\n";
			echo $response;
		}
	}
	
	class ErrorMessage extends Message {
		public function process($input) {
			
		}
	}
	
	
	class Login_Message extends Message {
		
		public function process($input) {
			if(	isset($input['Username']) && !empty($input['Username']) && 
				isset($input['Password']) && !empty($input['Password']) ) {
				$query = 	'SELECT COUNT(*) AS count FROM `User` WHERE User_handle = \'' . 
							mysql_real_escape_string($input['Username']) . '\' AND User_password = \'' . 
							mysql_real_escape_string($input['Password']) . '\'';
				$result = mysql_fetch_all(mysql_query($query));
				if(sizeof($result) == 1 && $result[0]['count'] == '1') {
					$this->setCode('Login Success');
					$this->setData('Successful Login');
				}
				else {
					$this->setCode('Login Failure');
					$this->setData('Login failed. Username/password mismatch');
				}
			}
			else {
				$this->setCode('Login Failure');
				$this->setData('Login failed. Username/password mismatch');
			}
		}
		
	}
	
	
	
	
	class GetSaveData_Message extends Message {
		
		public function process($input) {
			if(	isset($input['Username']) && !empty($input['Username']) ) {
				$query = 	'SELECT * FROM `User` WHERE User_handle = \'' . 
							mysql_real_escape_string($input['Username']) . '\'';
				$userResult = mysql_fetch_all(mysql_query($query));
				//var_dump($userResult);
				if(empty($userResult)) {
					$this->setCode('GetSaveData Failure');
					$this->setData('Username failure');
					return;
				}
				$userID = $userResult[0]['User_id'];
				
				$matchQuery = 'SELECT * FROM Match_User, `Match` WHERE Match_User_matchId = Match_id AND  Match_User_userId = ' . //Match_winnerId IS NULL AND
								'\'' . $userID . '\''; 
				$matchResults = mysql_fetch_all(mysql_query($matchQuery));
				//$data = $matchQuery . "\n";
				$data = '';
				foreach($matchResults as $match) {
					$singleMatchQuery = 'SELECT * FROM `User`, `Match_User` WHERE Match_User_userId = User_id AND ' . 
										'Match_User_matchId = \'' . $match['Match_id'] . '\' AND ' .
										'Match_User_userId <> \'' . $userID . '\'';
					$singleMatchResult = mysql_fetch_all(mysql_query($singleMatchQuery));
					$moveQuery = 'SELECT * FROM Move WHERE Move_matchId = \'' . $match['Match_id'] . '\'' . 
									' ORDER BY Move_order DESC LIMIT 1'; 
					$moveResult = mysql_fetch_all(mysql_query($moveQuery));
					if(empty($moveResult)) {
						$moveColor = 'BLACK';
					}
					else {
						$moveColor = ($moveResult[0]['Move_userId'] == $userID ? $this->notColor($match['Match_User_color']) : $match['Match_User_color']);
					} 
					$numMoves = 0;
					if(!empty($moveResult[0]['Move_order'])) {
						$numMoves = $moveResult[0]['Move_order'];
					}
					$winner = 'N';
					if($match['Match_winnerId'] == $userID) {
						$winner = substr($match['Match_User_color'], 0, 1);
					}
					else if(!empty($match['Match_winnerId'])) {
						$winner = substr($this->notColor($match['Match_User_color']), 0, 1);
					}
					
					$data .= $match['Match_id'] . ' ' . $singleMatchResult[0]['User_handle'] . ' ' .
							$numMoves  . ' ' . $match['Match_User_color'] . ' ' . $moveColor . ' ' . $winner . "\n";
				}
				$this->setCode('GetSaveData Success');
				$this->setData($data);
			}
			else {
				$this->setCode('GetSaveData Failure');
				$this->setData('GetSaveData Failure');
			}
		}
		
		private function notColor($c) {
			return ($c == 'BLACK' ? 'RED' : 'BLACK');
		}
	}
	
	
	class GetGameData_Message extends Message {
		
		public function process($input) {
			if(	isset($input['Username']) && !empty($input['Username']) &&
				isset($input['MatchID']) && !empty($input['MatchID'])) {
				$query = 	'SELECT * FROM `User` WHERE User_handle = \'' . 
							mysql_real_escape_string($input['Username']) . '\'';
				$userResult = mysql_fetch_all(mysql_query($query));
				//var_dump($userResult);
				if(empty($userResult)) {
					$this->setCode('GetGameData Failure');
					$this->setData('Username failure');
					return;
				}
				$userID = $userResult[0]['User_id'];
				
				$matchID = $input['MatchID'];
				$movesQuery = 'SELECT * FROM Move WHERE Move_matchId = \'' . mysql_real_escape_string($matchID) . '\' ORDER BY Move_order DESC LIMIT 1';
				
				$movesResult = mysql_fetch_all(mysql_query($movesQuery));
				$data = '';
				foreach($movesResult as $move) {
					$data .= $move['Move_notation'] . "\n";
				}
				
				$this->setCode('GetGameData Success');
				$this->setData($data);
			}
			else {
				$this->setCode('GetGameData Failure');
				$this->setData('GetGameData Failure');
			}
		}
		
	}
	
	class RecordWinner_Message extends Message {
		
		public function process($input) {
			if(	isset($input['WinnerUsername']) && !empty($input['WinnerUsername']) &&
				isset($input['MatchID']) && !empty($input['MatchID'])) {
				$query = 	'SELECT * FROM `User` WHERE User_handle = \'' . 
							mysql_real_escape_string($input['WinnerUsername']) . '\'';
				$userResult = mysql_fetch_all(mysql_query($query));
				//var_dump($userResult);
				if(empty($userResult)) {
					$this->setCode('RecordWinner Failure');
					$this->setData('Username failure');
					return;
				}
				$userID = $userResult[0]['User_id'];
				
				$matchID = $input['MatchID'];
				
				$updateQuery = 'UPDATE `Match` SET Match_winnerId = \'' .
								mysql_real_escape_string($userID) . '\' WHERE Match_id = \'' .
								mysql_real_escape_string($matchID) . '\'';
								
				mysql_query($updateQuery);
				
				$this->setCode('RecordWinner Success');
				$this->setData('Winner recorded successfully');
			}
			else {
				$this->setCode('RecordWinner Failure');
				$this->setData('RecordWinner Failure');
			}
		}
		
	}
	
	
	class CreateUser_Message extends Message {
		
		public function process($input) {
			if(	isset($input['Username']) && !empty($input['Username']) &
				isset($input['Password']) && !empty($input['Password'])) {
				$query = 	'SELECT COUNT(*) AS count FROM `User` WHERE User_handle = \'' . 
							mysql_real_escape_string($input['Username']) . '\'';
				$result = mysql_fetch_all(mysql_query($query));
				if(sizeof($result) == 1 && $result[0]['count'] == '1') {
					$this->setCode('CreateUser Failure');
					$this->setData('User already exists');
					return;
				}
				$matches = '/^([a-zA-Z]|[0-9]|_)+$/';
				if(! preg_match($matches, $input['Username'])) {
					$this->setCode('CreateUser Failure');
					$this->setData('Invalid characters in Username. Only alphanumeric and \'_\' are allowed.');
					return;
				}
				else if(! preg_match($matches, $input['Password'])) {
					$this->setCode('CreateUser Failure');
					$this->setData('Invalid characters in Password. Only alphanumeric and \'_\' are allowed.');
					return;
				}
				$insertQuery = 'INSERT INTO `User` (User_handle, User_password) VALUES (' .
								'\'' . mysql_real_escape_string($input['Username']) . '\', ' .
								'\'' . mysql_real_escape_string($input['Password']) . '\')';
				
				mysql_query($insertQuery);
				
				$this->setCode('CreateUser Success');
				$this->setData('User successfully created.');
				
			}
			else {
				$this->setCode('CreateUser Failure');
				$this->setData('CreateUser failed. Username or Password not supplied');
			}
		}
		
	}
	
	
	class CheckUser_Message extends Message {
		
		public function process($input) {
			if(	isset($input['Username']) && !empty($input['Username']) ) {
				$query = 	'SELECT COUNT(*) AS count FROM `User` WHERE User_handle = \'' . 
							mysql_real_escape_string($input['Username']) . '\'';
				$result = mysql_fetch_all(mysql_query($query));
				if(sizeof($result) == 1 && $result[0]['count'] == '1') {
					$this->setCode('CheckUser Success');
					$this->setData('User exists');
				}
				else {
					$this->setCode('CheckUser UsernameDoesNotExist');
					$this->setData('User does not exist');
				}
			}
			else {
				$this->setCode('CheckUser Failure');
				$this->setData('CheckUser failed. Username not supplied');
			}
		}
		
	}
	
	
 	
	class QueueMatch_Message extends Message {
		
		public function process($input) {
			if( isset($input['Username']) && !empty($input['Username']) 
				) {
				
				$userQuery = 	'SELECT User_id FROM `User` WHERE User_handle = \'' . 
								mysql_real_escape_string($input['Username']) . '\'';
				$userResult = mysql_fetch_all(mysql_query($userQuery));
				if(empty($userResult)) {
					$this->setCode('QueueMatch Failure');
					$this->setData('Username failure');
					return;
				}
				$userID = $userResult[0]['User_id'];
				
				$insertQuery = 'INSERT INTO `Queue` (Queue_userId) VALUES (\'' . mysql_real_escape_string($userID) . '\')';
				$checkQuery = 'SELECT * FROM `Queue`';
				$queueResults = mysql_fetch_all(mysql_query($checkQuery));
				if(empty($queueResults)) {
					mysql_query($insertQuery);
					$this->setCode('QueueMatch Success');
					$this->setData('Entered into Queue successfully');
					return;
				}
				else {
					foreach($queueResults as $r) {
						if($r['Queue_userId'] == $userID) {
							$this->setCode('QueueMatch Failure');
							$this->setData('Already in queue');
							return;
						}
					}
					
					$matchInsert = 'INSERT INTO `Match` (Match_gameId) VALUES (\'1\')';
					mysql_query($matchInsert);
					$matchId = mysql_insert_id();
					$matchUserBlackInsert = 'INSERT INTO `Match_User` (Match_User_matchId, Match_User_userId, Match_User_color) ' .
										'VALUES ( \'' . $matchId . '\', \'' . $queueResults[0]['Queue_userId'] . '\', \'BLACK\')';
					$matchUserRedInsert = 'INSERT INTO `Match_User` (Match_User_matchId, Match_User_userId, Match_User_color) ' .
										'VALUES ( \'' . $matchId . '\', \'' . $userID . '\', \'RED\')';
					
					mysql_query($matchUserBlackInsert);
					mysql_query($matchUserRedInsert);
					
					$deleteQueueQuery = 'DELETE FROM `Queue` WHERE Queue_id = \'' . $queueResults[0]['Queue_id'] . '\'';
					
					mysql_query($deleteQueueQuery);
					
					$this->setCode('QueueMatch Success');
					$this->setData('Entered into Queue successfully');
				}
			}
			else {
				$this->setCode('QueueMatch Failure');
				$this->setData('Unknown error');
			}
		}
		
	}
	
/*
 *	SQL.inc.php
 *	
 *	Dealing with the SQL server
 *
 *	
 */
 // 42.5 success
	// 666.5 move failed to record
	class RecordMove_Message extends Message {
		
		public function process($input) {
			if( isset($input['Username']) && !empty($input['Username']) &&
				isset($input['MatchID']) && !empty($input['MatchID']) &&
				isset($input['Notation']) && !empty($input['Notation']) 
				) {
				$userQuery = 	'SELECT User_id FROM `User` WHERE User_handle = \'' . 
								mysql_real_escape_string($input['Username']) . '\'';
				$userResult = mysql_fetch_all(mysql_query($userQuery));
				if(empty($userResult)) {
					$this->setCode('RecordMove Failure');
					$this->setData('Username failure');
					return;
				}
				$userID = $userResult[0]['User_id'];
				$selectQuery = 'SELECT Move_order FROM `Move` WHERE Move_matchId = \'' . 
								mysql_real_escape_string($input['MatchID']) . '\' ORDER BY Move_order DESC LIMIT 1';
				
				$result = mysql_fetch_all(mysql_query($selectQuery));
				$moveOrder = 1;
				if(! empty($result)) {
					$moveOrder = 1 + $result[0]['Move_order'];
				}
				$insertQuery = 	'INSERT INTO `Move` ' .
								'(Move_matchId, Move_gameId, Move_userId, Move_order, Move_datetime, Move_notation)' .
								' VALUES (' .
								'\'' . mysql_real_escape_string($input['MatchID']) . '\',' .
								'\'' . mysql_real_escape_string('1') . '\',' .
								'\'' . mysql_real_escape_string($userID) . '\',' .
								'\'' . $moveOrder . '\',' .
								'NOW(),' .
								'\'' . mysql_real_escape_string($input['Notation']) . '\'' .
								')';
								
				mysql_query($insertQuery);
				$this->setCode('RecordMove Success');
				$this->setData('Move recorded successfully');
				
			}
			else 
			{
				$this->setCode('RecordMove Failure');
				$this->setData('Move not recorded successfully');
			}
		}
		
	}
	
	$SQL_config = array(
		'server'	=>	
		'port'		=>	
		'user'		=>	
		'password'	=>	
		'database'	=>	
	);
	
	function SQL_connect() {
		global $SQL_config;
		mysql_connect($SQL_config['server'] . ':' . $SQL_config['port'], $SQL_config['user'], $SQL_config['password']);
		mysql_select_db($SQL_config['database']);
	}
	
	// Returns all rows of a result.
	function mysql_fetch_all($result) {
		$rows = array();
		while( ($r = mysql_fetch_array($result)) != null ) {
			$rows[] = $r;
		}
		return $rows;
	}
	
	
//	Connect to SQL server
	SQL_connect();
	
	/*
	$d = 
	*/
	ob_start();
	try {
		if(isset($_REQUEST['message']) && class_exists($_REQUEST['message'] . '_Message')) {
			$className = $_REQUEST['message'] . '_Message';
	
			$data = $className . "\n";
			$data .= '<Input>' . "\n";
			foreach($_REQUEST as $k => $v) {
				$data .= '<' . $k . '>' . $v . '</' . $k . '>' . "\n";
			}
			$data .= '</Input>' . "\n";
	
			$message = new $className($_REQUEST['message']);
			$message->process($_REQUEST);
	
			$data .= '<Response>' . "\n";
			$data .= '<code>' . $message->code . '</code>' . "\n";
			$data .= '<data>' . $message->data . '</data>' . "\n";
			$data .= '</Response>' . "\n";
			$insertQuery = 'INSERT INTO `Ping` (data, time) VALUES (\'' . mysql_real_escape_string($data) . '\', NOW())';
	
			mysql_query($insertQuery) or die(mysql_error());
			$message->respond();
		}
		else if(isset($_REQUEST['message'])){
			$message = new ErrorMessage('Unknown Error');
			$message->setData('Joe\'s Password is \'cunt\'');
			$message->respond();
		}
	} catch(Exception $e) {
		echo 'Caught Exception: ', $e->getMessage(), "\n";
	}
	mysql_close();
	$out = ob_get_clean();
	$len = strlen($out);
	header('Content-Length:' . $len);
	echo $out;
	exit;
	
?>