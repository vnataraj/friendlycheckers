<?php
/*
 *	Login_Message.inc.php
 *	
 *	Supports checking to see if a username and password are valid
 *
 *	
 */
	
	class Login_Message extends Message {
		
		public function process($input) {
			if(	isset($input['Username']) && !empty($input['Username']) && 
				isset($input['Password']) && !empty($input['Password']) ) {
				$query = 	'SELECT COUNT(*) AS count FROM `User` WHERE User_handle = \'' . 
							$input['Username'] . '\' AND User_password = \'' . $input['Password'] . '\'';
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
?>
