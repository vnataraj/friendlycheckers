<?php
/*
 *	SQL.inc.php
 *	
 *	Dealing with the SQL server
 *
 *	
 */
	
	$SQL_config = array(
		'server'	=>	'localhost',
		'port'		=>	'8889',
		'user'		=>	'root',
		'password'	=>	'root',
		'database'	=>	'Checkers'
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
?>
