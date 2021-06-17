using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NReco;      //   https://www.nrecosite.com/nrecoframework/
using NReco.Linq; //   https://www.nuget.org/packages/NReco/

namespace testCredit
{
    interface ITrade
    {
        double Value { get; } //indicates the transaction amount in dollars
        string ClientSector { get; } //indicates the client´s sector which can be "Public" or "Private"
        DateTime NextPaymentDate { get; } //indicates when the next payment from the client to the bank is expected
    }

    class CCategorizeTrades : ITrade
    {
        private double mValue;
        private string mClientSector;
        private DateTime mNextPaymentDate;
        private DateTime mReferenceDate;

        private List<string> mRules = new List<string>();
        private LambdaParser lambdaParser = new LambdaParser();
        private Dictionary<string, object> varContext = new Dictionary<string, object>();

        public CCategorizeTrades(List<string> rules, string referenceDate, string input)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            mRules = rules;

            string[] separator = { " " };
            string[] item = input.Split(separator, StringSplitOptions.None);

            Value = Double.Parse(item[0]);
            ClientSector = item[1];
            NextPaymentDate = DateTime.ParseExact(item[2], "d", provider);

            mReferenceDate = DateTime.ParseExact(referenceDate, "d", provider);
        }

        public string category()
        {
            string lcategory = "indeterminate";

            // handling the input values
            varContext["Value"] = Value;
            varContext["ClientSector"] = ClientSector;
            varContext["NextPaymentDate"] = NextPaymentDate;
            varContext["DaysAfterTheReferenceDate"] = (mReferenceDate - NextPaymentDate).Days;

            varContext["Private"] = "Private";
            varContext["Public"] = "Public";

            // Categories 
            varContext["EXPIRED"] = "EXPIRED";
            varContext["HIGHRISK"] = "HIGHRISK";
            varContext["MEDIUMRISK"] = "MEDIUMRISK";

            // Control keyword
            varContext["NEXT"] = "__NEXT__";


            foreach (string expression in mRules)
            {
                string lresult;
                if ((lresult = lambdaParser.Eval(expression, varContext).ToString()) != "__NEXT__")
                {
                    lcategory = lresult;
                    break;
                }
            }
            return lcategory;
        }


        //Implements the properties 
        public double Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }
        public string ClientSector
        {
            get
            {
                return mClientSector;
            }
            set
            {
                mClientSector = value;
            }
        }
        public DateTime NextPaymentDate
        {
            get
            {
                return mNextPaymentDate;
            }
            set
            {
                mNextPaymentDate = value;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<string> rules = new List<string>();
            List<CCategorizeTrades> categorizeTrades = new List<CCategorizeTrades>();

            string referenceDate, s;
            int items;

            // Rules loading section
            //
            rules.Add("DaysAfterTheReferenceDate > 30 ? EXPIRED: NEXT");
            rules.Add("Value > 1000000 && ClientSector==Private ? HIGHRISK : NEXT");
            rules.Add("Value > 1000000 && ClientSector==Public ? MEDIUMRISK : NEXT");

            //
            //
            referenceDate = Console.ReadLine();
            items = int.Parse(Console.ReadLine());

            while ((s = Console.ReadLine()) != null)
            {
                categorizeTrades.Add(new CCategorizeTrades(rules, referenceDate, s));
            }

            foreach (CCategorizeTrades ct in categorizeTrades)
            {
                Console.WriteLine(ct.category());
            }
        }
    }
}
