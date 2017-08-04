using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fiddler;

namespace SecureNet.Classes
{
    class FiddlerService
    {

        /*

        private void runService()
        {
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
        }

        private void FiddlerApplication_BeforeRequest(Session oSession)
        {
            string getLongUrl = oSession.url; //Mostly url+port
            string getUrl = null;

            int delimiterColon = getLongUrl.IndexOf(':');
            int delimiterSlash = getLongUrl.IndexOf('/');

            //Gets URL only
            if (delimiterColon != -1)
            {
                getUrl = getLongUrl.Substring(0, delimiterColon);
            }
            else if (delimiterSlash != -1)
            {
                getUrl = getLongUrl.Substring(0, delimiterSlash);
            }
            else
            {
                getUrl = getLongUrl;
            }

            Console.WriteLine("** Long Url: " + getLongUrl);
            Console.WriteLine("** Shortened url: " + getUrl);

            //EnterStanleyCode();

            bool stanley = true;

            if (oSession.HostnameIs("www.yahoo.com"))
            {
                stanley = false;
            }
            else
                stanley = true;

            if (stanley == false) //site is unsafe
            {
                oSession.Abort();
                Console.WriteLine("** Session Aborted");

                PrintResults(oSession);
            }

        }

        public DataObject PrintResults(Session oSession)
        {
            DataObject results = new DataObject()
            { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = oSession.state.ToString() };
            return results;
        }

        public void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            Invoke(new UpdateUI(() =>
            {
                dataGrid1.Items.Add(new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = oSession.state.ToString() });

            }));

        }
        
         */
    }
}
