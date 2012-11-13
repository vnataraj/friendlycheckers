<?php
/*
 *	Message.inc.php
 *	
 *	Generic Message
 *
 *	
 */
	
	abstract class Message {
		
		protected $name;
		protected $code;
		protected $data;
		
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
				$this->setData('Unknown Error');
				$this->respondFailure();
			}
		}
		
		protected function setCode($type) {
			$codes = array(
				'Unknown Error' => 	'666.666',
				'Login Success'	=>	'42.1',
				'Login Failure'	=>	'666.1'
			);
			
			$this->code = $codes[$type];
		}
		
		protected function setData($d) {
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
			$response	.=	$responseHeader;
			$response	.=	"\n";
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
?>
