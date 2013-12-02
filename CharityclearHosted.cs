using System;
using System.Web;
using System.Web.UI;


using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;


namespace charityclear
{
	public partial class Default : System.Web.UI.Page
	{
		
		 private Uri GATEWAY_URL = new Uri("https://gateway.charityclear.com/hosted/");
		// holds the request string
        private string reqString = "";
		// holds the response string
        private string resString = "";
		// holds the pre shared key
		private string sharedKey = "Circle4Take40Idea";
		// debug flag, flip to true for verbose console logging
		private bool debug = true;
		
       
        private NameValueCollection resCol = new NameValueCollection();
        private String pageUrl = HttpContext.Current.Request.Url.AbsoluteUri;

        public string merchantId { get; set; }

        public string action { get; set; }

        public int transType { get; set; }

        public string uniqueIdentifier { get; set; }

        public int currencyCode { get; set; }

        public int amount { get; set; }

        public String orderRef { get; set; }

        public string cardNumber { get; set; }

        public string cardExpiryMM { get; set; }

        public string cardExpiryYY { get; set; }

        public string cardCVV { get; set; }

        public string customerName { get; set; }

        public string customerEmail { get; set; }

        public string customerPhone { get; set; }

        public string customerAddress { get; set; }

        public int countryCode { get; set; }

        public string customerPostcode { get; set; }

        public string threeDSMD { get; set; }

        public string threeDSPaRes { get; set; }

        public string threeDSPaReq { get; set; }

        public string threeDSACSURL { get; set; }
		
		
		 protected void Page_Load(object sender, EventArgs e)
        {
					
            
            threeDSMD = "";
            threeDSPaRes = "";
            threeDSPaReq = "";

            if (Request.Form["MD"] != null)
                threeDSMD = Request.Form["MD"];
            if (Request.Form["PaRes"] != null)
                threeDSPaRes = Request.Form["PaRes"];
            if (Request.Form["PaReq"] != null)
                threeDSPaReq = Request.Form["PaReq"];

            fillInfo();
            BuildForm();
			return;
          
            int responseCode;
			
           // responseCode = ParseResponse();
			
			if(debug){
				Console.WriteLine("Response Code (as parsed by the code)" + responseCode);
			}
			
            if (responseCode == 65802)
            {
                send3DSForm();
            }
            else if (responseCode == 0)
            {
                Response.Write("<P>Thank you for your payment</P>");
            }
            else
            {
                
                Response.Write(String.Format("<P>Failed to take payment- Response Code {0}</P>", responseCode));
            }

        }

        private void fillInfo()
        {
            merchantId = "100001";
            action = "SALE";
            transType = 1;
            uniqueIdentifier = "12345s6789c1";
            currencyCode = 826;
            amount = 1202; // VISA
            orderRef = "TestPurchase";
            cardNumber = "4012010000000000009"; // VISA
            cardExpiryMM = "12";
            cardExpiryYY = "14";
            cardCVV = "332";
            customerName = "CharityClear"; // VISA
            customerEmail = "solutions@charityclear.com";
            customerPhone = "+44(0)8450099575";
            customerAddress = "31 Test Card Street";
            countryCode = 826;
            customerPostcode = "1TEST8";
        }

        private void BuildForm()
        {
			
			SortedDictionary<String, String> sortedDictionary = new SortedDictionary<string,string>();
			
	
            sortedDictionary.Add("merchantID",merchantId.ToString());
            sortedDictionary.Add("action" ,action.ToString());
            sortedDictionary.Add("type" ,transType.ToString());
            sortedDictionary.Add("transactionUnique" ,uniqueIdentifier.ToString ());
            sortedDictionary.Add("currencyCode" ,currencyCode.ToString());
            sortedDictionary.Add("amount"  ,amount.ToString()) ;
            sortedDictionary.Add("orderRef" ,orderRef.ToString()) ;
            sortedDictionary.Add("cardNumber" ,cardNumber.ToString());
            sortedDictionary.Add("cardExpiryMonth" ,cardExpiryMM.ToString());
            sortedDictionary.Add("cardExpiryYear" ,cardExpiryYY.ToString());
            sortedDictionary.Add("cardCVV" ,cardCVV.ToString()) ;
            sortedDictionary.Add("customerName" ,customerName.ToString());
            sortedDictionary.Add("customerEmail" ,customerEmail.ToString());
            sortedDictionary.Add("customerPhone" ,customerPhone.ToString()) ;
            sortedDictionary.Add("customerAddress" ,customerAddress.ToString()) ;
            sortedDictionary.Add("countryCode" ,countryCode.ToString()) ;
            sortedDictionary.Add("customerPostcode" ,customerPostcode.ToString()) ;
            sortedDictionary.Add("threeDSMD" ,threeDSMD.ToString()) ;
            sortedDictionary.Add("threeDSPaRes",threeDSPaRes.ToString()) ;
            sortedDictionary.Add("threeDSPaReq" ,threeDSPaReq.ToString()) ;
			
			  //Build request String
			
			int count = 0;
			
            foreach (KeyValuePair<String, String> kvp in sortedDictionary){
				
				if(count > 0){
					reqString += "&";
				}
				
				reqString += kvp.Key + "=" + urlEncode(kvp.Value);
				count++;

            }
			
			
			if(debug){
				Console.WriteLine("String Sent To Hasher: "+ reqString + sharedKey);
			}
			
	
			
			System.String Hashed = System.BitConverter.ToString(((System.Security.Cryptography.SHA512)new System.Security.Cryptography.SHA512Managed()).ComputeHash(System.Text.Encoding.UTF8.GetBytes(reqString + sharedKey))).Replace("-","").ToLower();
			
			if(debug){
				Console.WriteLine("Hash Returned From Hasher: "+ Hashed);
			}
			
			
			sortedDictionary.Add("signature", Hashed);
			
			String formString = "<form method='post' action='"+GATEWAY_URL+"'>\n";
			
			foreach (KeyValuePair<String, String> kvp in sortedDictionary){
				
				
				
				formString +=  "<input type='hidden' name='"+kvp.Key+"' value='"+kvp.Value+"'/>\n";
				

            }
			formString += "" +
				"<input type='submit' value='Submit'/>\n" +
				"</form>";
			
			Response.Write(formString);
			
			if(debug){
				Console.WriteLine("Request String in Full: "+ reqString);
			}
            return;
        }

        private bool SendForm()
        {
            string line;

            try
            {
				if(debug){
					Console.WriteLine("Starting Request");
				}
				
                // Create the request
                HttpWebRequest request = (HttpWebRequest)WebRequest.CreateDefault(GATEWAY_URL);
                request.Method = "POST";
                request.KeepAlive = false;
                request.ContentType = "application/x-www-form-urlencoded";
				
				if(debug){
					Console.WriteLine("Request created");
				}
				
				
                // Send the data
                StreamWriter reqStream = new StreamWriter(request.GetRequestStream());
                reqStream.Write(reqString);
                reqStream.Close();
				
				if(debug){
					Console.WriteLine("Request Sent :"+reqString);
				}
            
				
				
                // Get the response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader resStream = new StreamReader(response.GetResponseStream());

                while ((line = resStream.ReadLine()) != null)
                {
                    resString += line;
                }

                resStream.Close();

                
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }

            return true;
        }

        private int ParseResponse()
        {
			if(debug){
				Console.WriteLine("Query String Length: "+resString.Length);
			}
			if (resString.Length > 0)
            {
				if(debug){
					Console.WriteLine("processing query string"+resString);
				}
                
				resCol = HttpUtility.ParseQueryString(resString);
            }else{
				// return a number bigger than 0 so on networking issues the class does not tell the application a good payment went through
				return 1;
			}
			
			// check response signature is correct
			if (!IsHashCorrect(resCol)) {
				// error code for invalid signature
				return 66343;
			}
			
            threeDSMD = resCol["threeDSMD"];
            threeDSPaReq = resCol["threeDSPaReq"];
            threeDSACSURL = resCol["threeDSACSURL"];

            return Convert.ToInt32(resCol["responseCode"]);
        }

        private void send3DSForm()
        {

            String formString = "";

            formString += "<p>Your transaction requires 3D Secure Authentication</p>";
            formString += "<form action=\"" + threeDSACSURL + "\" method=\"post\">";
            formString += "<input type=\"hidden\" name=\"MD\" value=\"" + threeDSMD + "\">";
            formString += "<input type=\"hidden\" name=\"PaReq\" value=\"" + threeDSPaReq + "\">";
            formString += "<input type=\"hidden\" name=\"TermUrl\" value=\"" + pageUrl + "\">";
            formString += "<input type=\"submit\" value=\"Continue\">";
            formString += "</form>";
            Response.Write(formString);
        }
		
		public string urlEncode(string value) {

			String newString = UpperCaseUrlEncode(value);

			newString = newString.Replace("!", "%21");

			newString = newString.Replace("*", "%2A");

			newString = newString.Replace("(", "%28");

			newString = newString.Replace(")", "%29");

			return newString;

		}
		
		public string UpperCaseUrlEncode(string value) {

							

			char[] charArray = HttpUtility.UrlEncode(value).ToCharArray();

			for (int i = 0; i < charArray.Length - 2; i++) {

				if (charArray[i] == '%') {

					charArray[i + 1] = char.ToUpper(charArray[i + 1]);

					charArray[i + 2] = char.ToUpper(charArray[i + 2]);

				}

			}



			return new string(charArray);

		}
		
		private Boolean IsHashCorrect(NameValueCollection resCol){

            StringBuilder res = new StringBuilder("");

			SortedDictionary<String, String> sortedDictionary = new SortedDictionary<string, string>();



            String signature = "";

			//Add to sorted dictionary

            foreach (String key in resCol) {

				sortedDictionary.Add(key, resCol[key]);			

            }

			// build string and retrieve the returned signature

			foreach (KeyValuePair<String, String> kvp in sortedDictionary) {

				if (kvp.Key == "signature"){

                    signature = kvp.Value;

                } else {

					res.Append(kvp.Key + "=" + urlEncode(kvp.Value) + "&");

                }

			}

			// Remove trailing &

            res.Length = res.Length - 1;

			

			// Test if signatures match
			System.String Hash = System.BitConverter.ToString(((System.Security.Cryptography.SHA512)new System.Security.Cryptography.SHA512Managed()).ComputeHash(System.Text.Encoding.UTF8.GetBytes(res.ToString() + sharedKey))).Replace("-","").ToLower();
			
			if(debug){
				Console.WriteLine("Checking Response Hash");
				Console.WriteLine("Response Sig: "+ signature);
				Console.WriteLine("Checking Agenst: "+ Hash);
				Console.WriteLine("Responce String "+ res.ToString());
			}
			
			return signature == Hash;
	

		}
		
		
	}
}


