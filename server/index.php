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
 	
//	Require include of SQL functions
	require_once './includes/SQL.inc.php';
	
//	Require include of Message class
	require_once './includes/Message.inc.php';
	require_once './includes/Login_Message.inc.php';

//	Connect to SQL server
	SQL_connect();
	
	if(isset($_REQUEST['message']) && class_exists($_REQUEST['message'] . '_Message')) {
		$className = $_REQUEST['message'] . '_Message';
		
		$message = new $className($_REQUEST['message']);
		$message->process($_REQUEST);
		$message->respond();
	}
	else {
		$message = new ErrorMessage('Unknown Error');
		$message->respond();
	}
	
	
?>