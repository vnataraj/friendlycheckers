<?php
/*
 *	index.php
 *	
 *	Main landing page for Checkers requests
 *
 *	
 */

//	Require include of SQL functions
	require_once './includes/SQL.inc.php';

//	Connect to SQL server
	SQL_connect();
	
	$messageCodes = array(
		'InvalidMessage'	=>	'666.1'
	);
	
	
	$valid_message = array(
		'Login',
		'Poll'
	);
	
	if(isset($_REQUEST['message']) && in_array($_REQUEST['message'], $valid_message)) {
		//echo $_REQUEST['message'] . '_Process';
		$funcName = $_REQUEST['message'] . '_Process';
		$funcName($_REQUEST);
	}
	else {
		invalidMessage();
	}
	
	
	function invalidMessage() {
		global $messageCodes;
		failureMessage($messageCodes['InvalidMessage'], 'Invalid/Malformed Message');
	}
	
	function failureMessage($code, $data) {
		genericMessage('Failure', $code, $data);
	}
	
	function successMessage($code, $data) {
		genericMessage('Success', $code, $data);
	}
	
	function genericMessage($status, $code, $data) {
		$response 	= 	'';
		$response 	.=	$status;
		$response	.=	"\n";
		$response	.=	$code;
		$response	.=	"\n";
		$response	.=	$data;
		echo $response;
	
	}
	
	function Poll_Process($data) {
		$dataString = '';
		foreach($data as $key => $value) {
			$dataString .= $key . '=' . $value . '<br/>';
		}
		successMessage('42.1', $dataString);
	}
	
?>