using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections;
using System.Net;

namespace ShiftboardAPI
{
	public class ShiftboardClient
	{
		const string ApiURL = "https://api.shiftdata.com/api/api.cgi";
		string apiKey, apiSecret;

		public ShiftboardClient(string apiKey, string apiSecret) {
			if (String.IsNullOrWhiteSpace(apiKey))
				throw new ArgumentNullException("apiKey should contain a valid key.");
			
			if (String.IsNullOrWhiteSpace(apiSecret))
				throw new ArgumentNullException("apiSecret should be a valid key.");

			this.apiKey = apiKey;
			this.apiSecret = apiSecret;
		}

		public static void Main (string[] args)
		{
			ShiftboardClient client = new ShiftboardClient ("YOUR API KEY", "YOUR API SECRET");
			string method = "system.echo";
			string parameter = "{ \"dinner\": \"nachos\" }";
			Console.WriteLine( client.call (method, parameter));
		}

		public string generateSignature (string method, string parameters) {
			// Join the method and parameter to be signed
			string joinedMessage = $"method{method}params{parameters}";
			string digest = encodeToHMAC (joinedMessage);

			return digest;
		}

		public string encodeParamsInBase64 (string parameters) {
			// Get byte representation of string
			byte[] dataRepresentation = Encoding.UTF8.GetBytes (parameters);
			string base64String = Convert.ToBase64String (dataRepresentation);

			return base64String;
		}

		public string encodeToHMAC (string input) {
			// Turn api secret into bytes
			byte[] apiSecretBytes = Encoding.UTF8.GetBytes(apiSecret);
			HMACSHA1 hmacSHA = new HMACSHA1 (apiSecretBytes);
			// Turn input into bytes and compute it's hmac value
			byte[] inputByteArray = Encoding.UTF8.GetBytes (input);
			byte[] calculatedHash = hmacSHA.ComputeHash (inputByteArray);
			// Convert bytes into a base-64 encoded string
			string calculatedHashedString = Convert.ToBase64String (calculatedHash);

			return calculatedHashedString;
		}

		public string generatePayload(string method, string parameters) {
			// Generate pieces needed for url
			string encodedParams = encodeParamsInBase64 (parameters);
			string signature = generateSignature (method, parameters);
			string jsonRPC = "jsonrpc=2.0";
			string fullURL = $"{ApiURL}?{jsonRPC}&method={method}&params={encodedParams}&access_key_id={this.apiKey}&signature={signature}";

			return fullURL;
		}

		public string call(string method, string parameters) {
			string fullURL = generatePayload (method, parameters);

			// Create a request
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (fullURL);
			request.ContentType = "application/json";

			using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
			{
				// Get the response stream
				StreamReader reader = new StreamReader(response.GetResponseStream());
				// return response
				return reader.ReadToEnd();
			}

		}
	}
}
