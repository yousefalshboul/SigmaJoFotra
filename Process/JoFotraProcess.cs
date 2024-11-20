using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SigmaJofotraAPICore.Process
{
    public class JoFotraProcess
    {
        public async Task<string> processJoFotra(IConfiguration _configuration)
        {
            string xmlTotalInvoice = "";
            string xmlInvoiceContent = "";
            StringBuilder allInvoicesContent = new StringBuilder();
            string rootPath = _configuration["FatoraSettings:FatoraXmlPath"];
            string extension = _configuration["FatoraSettings:FileExtension"];
            string processedFilePath = _configuration["FatoraSettings:ProcessedFile"];
            string failedFilePath = _configuration["FatoraSettings:FailedFile"];
            string[] files;
            string filename = "";
            string FatoraBatchFile = "";
            string responseBody = "";
            try
            {
                files = System.IO.Directory.GetFiles(rootPath, "*." + extension);
                FatoraBatchFile = Directory.GetFiles(rootPath, "*." + extension)[0];
                filename = Path.GetFileName(FatoraBatchFile);
                if (files.Length == 0)
                {
                    Console.WriteLine("The file doesn't exist");
                }
                else
                {
                    XmlDocument SigmaFatora = new XmlDocument();
                    SigmaFatora.Load(FatoraBatchFile);
                    XmlNodeList Totalinvoices = SigmaFatora.SelectNodes("//table_invoice");
                    foreach (XmlNode Totalinvoice in Totalinvoices)
                    {
                        string N12 = Totalinvoice.SelectSingleNode("N12")?.InnerText;
                        if (N12 == "2")
                        {
                            int vouno = 177668;
                            DateTime date = DateTime.Now;
                            string description = "Total";
                            double Discount = Convert.ToDouble(Totalinvoice.SelectSingleNode("DISCOUNT")?.InnerText);
                            double Total = Convert.ToDouble(Totalinvoice.SelectSingleNode("TOTAL")?.InnerText);
                            double TaxAmount = Convert.ToDouble(Totalinvoice.SelectSingleNode("TAX")?.InnerText);
                            double GTotal = Convert.ToDouble(Totalinvoice.SelectSingleNode("GTOTAL")?.InnerText);
                            string TaxCode = Totalinvoice.SelectSingleNode("TAXCODE1")?.InnerText;
                            string Profile = "2932792";
                            string UUID = Guid.NewGuid().ToString();

                            string IssueDt = date.ToString("yyyy-MM-dd");

                            Discount = Math.Round(Discount, 9);
                            Total = Math.Round(Total, 9);
                            TaxAmount = Math.Round(TaxAmount, 9);
                            GTotal = Math.Round(GTotal, 9);

                            double TaxExclusiveAmount = Total;
                            TaxExclusiveAmount = Math.Round(TaxExclusiveAmount, 9);

                            double TaxInclusiveAmount = TaxExclusiveAmount - Discount + TaxAmount;
                            TaxInclusiveAmount = Math.Round(TaxInclusiveAmount, 9);

                            xmlTotalInvoice = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"" xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
    <cbc:ProfileID>reporting:1.0</cbc:ProfileID>
    <cbc:ID>#id#</cbc:ID>
    <cbc:UUID>#UUID#</cbc:UUID>
    <cbc:IssueDate>#IssueDt#</cbc:IssueDate>
    <cbc:InvoiceTypeCode name=""012"">#TaxCode#</cbc:InvoiceTypeCode>
    <cbc:Note>#description#</cbc:Note>
    <cbc:DocumentCurrencyCode>JOD</cbc:DocumentCurrencyCode>
    <cbc:TaxCurrencyCode>JOD</cbc:TaxCurrencyCode>
    <cac:AdditionalDocumentReference>
        <cbc:ID>ICV</cbc:ID>
        <cbc:UUID>#UUID#</cbc:UUID>
    </cac:AdditionalDocumentReference>
    <cac:AccountingSupplierParty>
        <cac:Party>
            <cac:PostalAddress>
                <cac:Country>
                    <cbc:IdentificationCode>JO</cbc:IdentificationCode>
                </cac:Country>
            </cac:PostalAddress>
            <cac:PartyTaxScheme>
                <cbc:CompanyID>#Profile#</cbc:CompanyID>
                <cac:TaxScheme>
                    <cbc:ID>VAT</cbc:ID>
                </cac:TaxScheme>
            </cac:PartyTaxScheme>
            <cac:PartyLegalEntity>
                <cbc:RegistrationName>شركة زيد محمد مصطفى العدوان وولده</cbc:RegistrationName>
            </cac:PartyLegalEntity>
        </cac:Party>
    </cac:AccountingSupplierParty>
    <cac:AccountingCustomerParty />
    <cac:SellerSupplierParty>
        <cac:Party>
        <cac:PartyIdentification>
        <cbc:ID>#id#</cbc:ID>
        </cac:PartyIdentification>
        </cac:Party>
    </cac:SellerSupplierParty>
    <cac:AllowanceCharge>
        <cbc:ChargeIndicator>false</cbc:ChargeIndicator>
        <cbc:AllowanceChargeReason>discount</cbc:AllowanceChargeReason>
        <cbc:Amount currencyID=""JO"">#Discount#</cbc:Amount>
    </cac:AllowanceCharge>
    <cac:TaxTotal>
        <cbc:TaxAmount currencyID=""JO"">#TaxAmount#</cbc:TaxAmount>
    </cac:TaxTotal>
    <cac:LegalMonetaryTotal>
        <cbc:TaxExclusiveAmount currencyID=""JO"">#TaxExclusiveAmount#</cbc:TaxExclusiveAmount>
        <cbc:TaxInclusiveAmount currencyID=""JO"">#TaxInclusiveAmount#</cbc:TaxInclusiveAmount>
        <cbc:AllowanceTotalAmount currencyID=""JO"">#Discount#</cbc:AllowanceTotalAmount>
        <cbc:PayableAmount currencyID=""JO"">#TaxInclusiveAmount#</cbc:PayableAmount>
    </cac:LegalMonetaryTotal>";
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#Profile#", Profile);
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#id#", vouno.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#UUID#", UUID);
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#IssueDt#", IssueDt.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#TaxAmount#", TaxAmount.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#description#", description.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#Discount#", Discount.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#TaxExclusiveAmount#", TaxExclusiveAmount.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#TaxInclusiveAmount#", TaxInclusiveAmount.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#TaxCode#", TaxCode.ToString());
                        }
                        else
                        {
                            continue;
                        }
                    }

                    XmlNodeList invoices = SigmaFatora.SelectNodes("//table_invoice");
                    foreach (XmlNode invoice in invoices)
                    {
                        string N12 = invoice.SelectSingleNode("N12")?.InnerText;
                        if (N12 == "1")
                        {
                            int vouno = Convert.ToInt32(invoice.SelectSingleNode("VOUNO")?.InnerText);
                            DateTime date = Convert.ToDateTime(invoice.SelectSingleNode("Date")?.InnerText);
                            string pers = invoice.SelectSingleNode("PERS")?.InnerText;
                            string salret = invoice.SelectSingleNode("SALRET")?.InnerText;
                            string prx = invoice.SelectSingleNode("PRX")?.InnerText;
                            string vou = invoice.SelectSingleNode("VOU")?.InnerText;
                            string itemno = invoice.SelectSingleNode("ITEMNO")?.InnerText;
                            double cost = Convert.ToDouble(invoice.SelectSingleNode("COST")?.InnerText);
                            int qty = Convert.ToInt32(invoice.SelectSingleNode("QTY")?.InnerText);
                            string description = invoice.SelectSingleNode("DESCRIPTION")?.InnerText;
                            double Discount = Convert.ToDouble(invoice.SelectSingleNode("DISCOUNT")?.InnerText);
                            string sret1 = invoice.SelectSingleNode("SRET1")?.InnerText;
                            int TX = Convert.ToInt32(invoice.SelectSingleNode("TX")?.InnerText);
                            double Total = Convert.ToDouble(invoice.SelectSingleNode("TOTAL")?.InnerText);
                            double TaxAmount = Convert.ToDouble(invoice.SelectSingleNode("TAX")?.InnerText);
                            double GTotal = Convert.ToDouble(invoice.SelectSingleNode("GTOTAL")?.InnerText);
                            string TaxCode = invoice.SelectSingleNode("TAXCODE1")?.InnerText;
                            string Profile = "2932792";
                            string UUID = Guid.NewGuid().ToString();
                            string IssueDt = date.ToString("yyyy-MM-dd");

                            cost = Math.Round(cost, 9);
                            Discount = Math.Round(Discount, 9);

                            double LineExtensionAmount = (cost * qty) - Discount;
                            LineExtensionAmount = Math.Round(LineExtensionAmount, 9);

                            Total = Math.Round(Total, 9);
                            TaxAmount = Math.Round(TaxAmount, 9);
                            GTotal = Math.Round(GTotal, 9);

                            double TaxExclusiveAmount = cost * qty;
                            TaxExclusiveAmount = Math.Round(TaxExclusiveAmount, 9);

                            double TaxInclusiveAmount = TaxExclusiveAmount - Discount + TaxAmount;
                            TaxInclusiveAmount = Math.Round(TaxInclusiveAmount, 9);

                            xmlInvoiceContent = @"<cac:InvoiceLine>
        <cbc:ID>#itemno#</cbc:ID>
        <cbc:InvoicedQuantity unitCode=""PCE"">#qty#</cbc:InvoicedQuantity>
        <cbc:LineExtensionAmount currencyID=""JO"">#LineExtensionAmount#</cbc:LineExtensionAmount>
        <cac:TaxTotal>
            <cbc:TaxAmount currencyID=""JO"">#TaxAmount#</cbc:TaxAmount>
            <cbc:RoundingAmount currencyID=""JO"">#TaxInclusiveAmount#</cbc:RoundingAmount>
            <cac:TaxSubtotal>
                <cbc:TaxAmount currencyID=""JO"">#TaxAmount#</cbc:TaxAmount>
                <cac:TaxCategory>
                    <cbc:ID schemeAgencyID=""6"" schemeID=""UN/ECE 5305"">S</cbc:ID>
                    <cbc:Percent>#TX#</cbc:Percent>
                    <cac:TaxScheme>
                        <cbc:ID schemeAgencyID=""6"" schemeID=""UN/ECE 5153"">VAT</cbc:ID>
                    </cac:TaxScheme>
                </cac:TaxCategory>
            </cac:TaxSubtotal>
        </cac:TaxTotal>
        <cac:Item>
            <cbc:Name>#description#</cbc:Name>
        </cac:Item>
        <cac:Price>
            <cbc:PriceAmount currencyID=""JO"">#cost#</cbc:PriceAmount>
            <cac:AllowanceCharge>
            <cbc:ChargeIndicator>false</cbc:ChargeIndicator>
            <cbc:AllowanceChargeReason>discount</cbc:AllowanceChargeReason>
            <cbc:Amount currencyID=""JO"">#Discount#</cbc:Amount>
            </cac:AllowanceCharge>
        </cac:Price>
    </cac:InvoiceLine>";
                            //xmlInvoiceContent = xmlInvoiceContent.Replace("#Profile#", Profile);
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#itemno#", itemno);
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#id#", vouno.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#UUID#", UUID);
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#IssueDt#", IssueDt.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#Amount#", Total.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TaxAmount#", TaxAmount.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#cost#", cost.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#description#", description.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#qty#", qty.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#Discount#", Discount.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TaxExclusiveAmount#", TaxExclusiveAmount.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TaxInclusiveAmount#", TaxInclusiveAmount.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TX#", TX.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TaxCode#", TaxCode.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#LineExtensionAmount#", LineExtensionAmount.ToString());
                            allInvoicesContent.Append(xmlInvoiceContent);
                        }
                        else
                        {
                            break;
                        }
                    }
                    string fullXml = xmlTotalInvoice + allInvoicesContent + "</Invoice>";
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(fullXml);
                    byte[] bytesToEncode = System.Text.Encoding.Default.GetBytes(fullXml);
                    using (HttpClient client = new HttpClient())
                    {
                        string encodedInvoice = Convert.ToBase64String(bytesToEncode);
                        string jsonPayload = $"{{ \"invoice\": \"{encodedInvoice}\" }}";
                        string serverUrl = "https://backend.jofotara.gov.jo/core/invoices/";
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serverUrl);
                        request.Headers.Add("Client-Id", "27e3f63a-e87c-47f8-a1c4-3bbdb0fe3598");
                        request.Headers.Add("Secret-Key", "Gj5nS9wyYHRadaVffz5VKB4v4wlVWyPhcJvrTD4NHtO2kYWBiQezsd+n4D1gjx6rLOMFMfCLU5af1VrlYBxZzB6U8nf6v8zLcKeGVJ9Hc/KA+xcE7PkmY46xmJARPAsm81LR5x84Mxf5lyBsczZdDdUDHWWyQ/TXVIt5T72bMVB01j/4RihyIZk9eKUkgSSFGrC3xeOtH0kNIkbnGdpgYB+4BTdRhzbbZHDQIcyDTCBuTS/lt9BitGUe6doeKuzMfbnOO1zApU3465vkctfniQ==");
                        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            responseBody = await response.Content.ReadAsStringAsync();

                            // Move the file to failed path
                            string todayFolder = Path.Combine(processedFilePath, DateTime.Now.ToString("yyyyMMdd"));
                            if (!Directory.Exists(todayFolder))
                            {
                                Directory.CreateDirectory(todayFolder);
                            }
                            string processedFileName = Path.Combine(todayFolder, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + filename);
                            Directory.Move(FatoraBatchFile, processedFileName);

                            // Write request and response to the new folder
                            string requestFilePath = Path.Combine(todayFolder, "Request_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml");
                            File.WriteAllText(requestFilePath, fullXml);

                            string responseFilePath = Path.Combine(todayFolder, "Response_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
                            File.WriteAllText(responseFilePath, responseBody);
                        }
                        else
                        {
                            responseBody = await response.Content.ReadAsStringAsync();

                            // Move the file to failed path
                            string todayFolder = Path.Combine(failedFilePath, DateTime.Now.ToString("yyyyMMdd"));
                            if (!Directory.Exists(todayFolder))
                            {
                                Directory.CreateDirectory(todayFolder);
                            }
                            string processedFileName = Path.Combine(todayFolder, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + filename);
                            Directory.Move(FatoraBatchFile, processedFileName);

                            // Write request and response to the new folder
                            string requestFilePath = Path.Combine(todayFolder, "Request_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml");
                            File.WriteAllText(requestFilePath, fullXml);

                            string responseFilePath = Path.Combine(todayFolder, "Response_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
                            File.WriteAllText(responseFilePath, responseBody);
                        }
                    }
                }
                return responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public async Task<string> processRefundJoFotra(IConfiguration _configuration)
        {
            string xmlTotalInvoice = "";
            string xmlInvoiceContent = "";
            StringBuilder allInvoicesContent = new StringBuilder();
            string RefundrootPath = _configuration["FatoraSettings:RefundFatoraXmlPath"];
            string extension = _configuration["FatoraSettings:FileExtension"];
            string RefundprocessedFilePath = _configuration["FatoraSettings:RefundProcessedFile"];
            string RefundfailedFilePath = _configuration["FatoraSettings:RefundFailedFile"];
            string[] files;
            string filename = "";
            string FatoraBatchFile = "";
            string responseBody = "";
            try
            {
                files = System.IO.Directory.GetFiles(RefundrootPath, "*." + extension);
                FatoraBatchFile = Directory.GetFiles(RefundrootPath, "*." + extension)[0];
                filename = Path.GetFileName(FatoraBatchFile);
                if (files.Length == 0)
                {
                    Console.WriteLine("The file doesn't exist");
                }
                else
                {
                    XmlDocument SigmaFatora = new XmlDocument();
                    SigmaFatora.Load(FatoraBatchFile);
                    XmlNodeList Totalinvoices = SigmaFatora.SelectNodes("//Rtable_invoice");
                    foreach (XmlNode Totalinvoice in Totalinvoices)
                    {
                        string N12 = Totalinvoice.SelectSingleNode("N12")?.InnerText;
                        if (N12 == "2")
                        {
                            int vouno = 177668;
                            DateTime date = DateTime.Now;
                            string description = "Total";
                            double Discount = 0;
                            double Total = Convert.ToDouble(Totalinvoice.SelectSingleNode("TOTAL")?.InnerText);
                            double TaxAmount = Convert.ToDouble(Totalinvoice.SelectSingleNode("TAX")?.InnerText);
                            double GTotal = Convert.ToDouble(Totalinvoice.SelectSingleNode("GTOTAL")?.InnerText);
                            string BuyerName = Totalinvoice.SelectSingleNode("ACCNAME")?.InnerText;
                            string BuyerID = Totalinvoice.SelectSingleNode("ACCNO")?.InnerText;
                            string TaxCode = Totalinvoice.SelectSingleNode("TAXCODE1")?.InnerText;
                            string Profile = "2932792";
                            string UUID = Guid.NewGuid().ToString();

                            string IssueDt = date.ToString("yyyy-MM-dd");

                            Discount = Math.Round(Discount, 9);
                            Total = Math.Round(Total, 9);
                            TaxAmount = Math.Round(TaxAmount, 9);
                            GTotal = Math.Round(GTotal, 9);

                            double TaxExclusiveAmount = Total;
                            TaxExclusiveAmount = Math.Round(TaxExclusiveAmount, 9);

                            double TaxInclusiveAmount = TaxExclusiveAmount - Discount + TaxAmount;
                            TaxInclusiveAmount = Math.Round(TaxInclusiveAmount, 9);

                            xmlTotalInvoice = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"" xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
    <cbc:ProfileID>reporting:1.0</cbc:ProfileID>
    <cbc:ID>#id#</cbc:ID>
    <cbc:UUID>#UUID#</cbc:UUID>
    <cbc:IssueDate>#IssueDt#</cbc:IssueDate>
    <cbc:InvoiceTypeCode name=""022"">#TaxCode#</cbc:InvoiceTypeCode>
    <cbc:Note>#description#</cbc:Note>
    <cbc:DocumentCurrencyCode>JOD</cbc:DocumentCurrencyCode>
    <cbc:TaxCurrencyCode>JOD</cbc:TaxCurrencyCode>
    <cac:AdditionalDocumentReference>
        <cbc:ID>ICV</cbc:ID>
        <cbc:UUID>#UUID#</cbc:UUID>
    </cac:AdditionalDocumentReference>
    <cac:AccountingSupplierParty>
        <cac:Party>
            <cac:PostalAddress>
                <cac:Country>
                    <cbc:IdentificationCode>JO</cbc:IdentificationCode>
                </cac:Country>
            </cac:PostalAddress>
            <cac:PartyTaxScheme>
                <cbc:CompanyID>#Profile#</cbc:CompanyID>
                <cac:TaxScheme>
                    <cbc:ID>VAT</cbc:ID>
                </cac:TaxScheme>
            </cac:PartyTaxScheme>
            <cac:PartyLegalEntity>
                <cbc:RegistrationName>شركة زيد محمد مصطفى العدوان وولده</cbc:RegistrationName>
            </cac:PartyLegalEntity>
        </cac:Party>
    </cac:AccountingSupplierParty>
    <cac:AccountingCustomerParty>
<cac:Party>
<cac:PartyIdentification>
<cbc:ID schemeID=""TN"">#BuyerID#</cbc:ID>
</cac:PartyIdentification>
<cac:PostalAddress>
<cbc:PostalZone>33554</cbc:PostalZone>
<cbc:CountrySubentityCode>JO-AZ</cbc:CountrySubentityCode>
<cac:Country>
<cbc:IdentificationCode>JO</cbc:IdentificationCode>
</cac:Country>
</cac:PostalAddress>
<cac:PartyTaxScheme>
<cbc:CompanyID>#BuyerID#</cbc:CompanyID>
<cac:TaxScheme>
<cbc:ID>VAT</cbc:ID>
</cac:TaxScheme>
</cac:PartyTaxScheme>
<cac:PartyLegalEntity>
<cbc:RegistrationName>#BuyerName#</cbc:RegistrationName>
</cac:PartyLegalEntity>
</cac:Party>
<cac:AccountingContact>
<cbc:Telephone>324323434</cbc:Telephone>
</cac:AccountingContact>
</cac:AccountingCustomerParty>
    <cac:SellerSupplierParty>
        <cac:Party>
        <cac:PartyIdentification>
        <cbc:ID>#id#</cbc:ID>
        </cac:PartyIdentification>
        </cac:Party>
    </cac:SellerSupplierParty>
    <cac:AllowanceCharge>
        <cbc:ChargeIndicator>false</cbc:ChargeIndicator>
        <cbc:AllowanceChargeReason>discount</cbc:AllowanceChargeReason>
        <cbc:Amount currencyID=""JO"">#Discount#</cbc:Amount>
    </cac:AllowanceCharge>
    <cac:TaxTotal>
        <cbc:TaxAmount currencyID=""JO"">#TaxAmount#</cbc:TaxAmount>
    </cac:TaxTotal>
    <cac:LegalMonetaryTotal>
        <cbc:TaxExclusiveAmount currencyID=""JO"">#TaxExclusiveAmount#</cbc:TaxExclusiveAmount>
        <cbc:TaxInclusiveAmount currencyID=""JO"">#TaxInclusiveAmount#</cbc:TaxInclusiveAmount>
        <cbc:AllowanceTotalAmount currencyID=""JO"">#Discount#</cbc:AllowanceTotalAmount>
        <cbc:PayableAmount currencyID=""JO"">#TaxInclusiveAmount#</cbc:PayableAmount>
    </cac:LegalMonetaryTotal>";
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#Profile#", Profile);
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#id#", vouno.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#UUID#", UUID);
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#IssueDt#", IssueDt.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#TaxAmount#", TaxAmount.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#description#", description.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#Discount#", Discount.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#TaxExclusiveAmount#", TaxExclusiveAmount.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#TaxInclusiveAmount#", TaxInclusiveAmount.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#TaxCode#", TaxCode.ToString());
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#BuyerID#", BuyerID);
                            xmlTotalInvoice = xmlTotalInvoice.Replace("#BuyerName#", BuyerName);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    XmlNodeList invoices = SigmaFatora.SelectNodes("//Rtable_invoice");
                    foreach (XmlNode invoice in invoices)
                    {
                        string N12 = invoice.SelectSingleNode("N12")?.InnerText;
                        if (N12 == "1")
                        {
                            int vouno = Convert.ToInt32(invoice.SelectSingleNode("VOUNO")?.InnerText);
                            DateTime date = Convert.ToDateTime(invoice.SelectSingleNode("Date")?.InnerText);
                            string pers = invoice.SelectSingleNode("PERS")?.InnerText;
                            string salret = invoice.SelectSingleNode("SALRET")?.InnerText;
                            string prx = invoice.SelectSingleNode("PRX")?.InnerText;
                            string vou = invoice.SelectSingleNode("VOU")?.InnerText;
                            string itemno = invoice.SelectSingleNode("ITEMNO")?.InnerText;
                            double cost = Convert.ToDouble(invoice.SelectSingleNode("COST")?.InnerText);
                            int qty = Convert.ToInt32(invoice.SelectSingleNode("QTY")?.InnerText);
                            string description = invoice.SelectSingleNode("DESCRIPTION")?.InnerText;
                            double Discount = Convert.ToDouble(invoice.SelectSingleNode("DISCOUNT")?.InnerText);
                            string sret1 = invoice.SelectSingleNode("SRET1")?.InnerText;
                            int TX = Convert.ToInt32(invoice.SelectSingleNode("TX")?.InnerText);
                            double Total = Convert.ToDouble(invoice.SelectSingleNode("TOTAL")?.InnerText);
                            double TaxAmount = Convert.ToDouble(invoice.SelectSingleNode("TAX")?.InnerText);
                            double GTotal = Convert.ToDouble(invoice.SelectSingleNode("GTOTAL")?.InnerText);
                            string TaxCode = invoice.SelectSingleNode("TAXCODE1")?.InnerText;
                            string Profile = "2932792";
                            string UUID = Guid.NewGuid().ToString();
                            string IssueDt = date.ToString("yyyy-MM-dd");

                            cost = Math.Round(cost, 9);
                            Discount = Math.Round(Discount, 9);

                            double LineExtensionAmount = (cost * qty) - Discount;
                            LineExtensionAmount = Math.Round(LineExtensionAmount, 9);

                            Total = Math.Round(Total, 9);
                            TaxAmount = Math.Round(TaxAmount, 9);
                            GTotal = Math.Round(GTotal, 9);

                            double TaxExclusiveAmount = cost * qty;
                            TaxExclusiveAmount = Math.Round(TaxExclusiveAmount, 9);

                            double TaxInclusiveAmount = TaxExclusiveAmount - Discount + TaxAmount;
                            TaxInclusiveAmount = Math.Round(TaxInclusiveAmount, 9);

                            xmlInvoiceContent = @"<cac:InvoiceLine>
        <cbc:ID>#itemno#</cbc:ID>
        <cbc:InvoicedQuantity unitCode=""PCE"">#qty#</cbc:InvoicedQuantity>
        <cbc:LineExtensionAmount currencyID=""JO"">#LineExtensionAmount#</cbc:LineExtensionAmount>
        <cac:TaxTotal>
            <cbc:TaxAmount currencyID=""JO"">#TaxAmount#</cbc:TaxAmount>
            <cbc:RoundingAmount currencyID=""JO"">#TaxInclusiveAmount#</cbc:RoundingAmount>
            <cac:TaxSubtotal>
                <cbc:TaxAmount currencyID=""JO"">#TaxAmount#</cbc:TaxAmount>
                <cac:TaxCategory>
                    <cbc:ID schemeAgencyID=""6"" schemeID=""UN/ECE 5305"">S</cbc:ID>
                    <cbc:Percent>#TX#</cbc:Percent>
                    <cac:TaxScheme>
                        <cbc:ID schemeAgencyID=""6"" schemeID=""UN/ECE 5153"">VAT</cbc:ID>
                    </cac:TaxScheme>
                </cac:TaxCategory>
            </cac:TaxSubtotal>
        </cac:TaxTotal>
        <cac:Item>
            <cbc:Name>#description#</cbc:Name>
        </cac:Item>
        <cac:Price>
            <cbc:PriceAmount currencyID=""JO"">#cost#</cbc:PriceAmount>
            <cac:AllowanceCharge>
            <cbc:ChargeIndicator>false</cbc:ChargeIndicator>
            <cbc:AllowanceChargeReason>discount</cbc:AllowanceChargeReason>
            <cbc:Amount currencyID=""JO"">#Discount#</cbc:Amount>
            </cac:AllowanceCharge>
        </cac:Price>
    </cac:InvoiceLine>";
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#Profile#", Profile);
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#itemno#", itemno);
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#id#", vouno.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#UUID#", UUID);
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#IssueDt#", IssueDt.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#Amount#", Total.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TaxAmount#", TaxAmount.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#cost#", cost.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#description#", description.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#qty#", qty.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#Discount#", Discount.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TaxExclusiveAmount#", TaxExclusiveAmount.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TaxInclusiveAmount#", TaxInclusiveAmount.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TX#", TX.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#TaxCode#", TaxCode.ToString());
                            xmlInvoiceContent = xmlInvoiceContent.Replace("#LineExtensionAmount#", LineExtensionAmount.ToString());
                            allInvoicesContent.Append(xmlInvoiceContent);
                        }
                        else
                        {
                            break;
                        }
                    }
                    string fullXml = xmlTotalInvoice + allInvoicesContent + "</Invoice>";
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(fullXml);
                    byte[] bytesToEncode = System.Text.Encoding.Default.GetBytes(fullXml);
                    using (HttpClient client = new HttpClient())
                    {
                        string encodedInvoice = Convert.ToBase64String(bytesToEncode);
                        string jsonPayload = $"{{ \"invoice\": \"{encodedInvoice}\" }}";
                        string serverUrl = "https://backend.jofotara.gov.jo/core/invoices/";
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serverUrl);
                        request.Headers.Add("Client-Id", "27e3f63a-e87c-47f8-a1c4-3bbdb0fe3598");
                        request.Headers.Add("Secret-Key", "Gj5nS9wyYHRadaVffz5VKB4v4wlVWyPhcJvrTD4NHtO2kYWBiQezsd+n4D1gjx6rLOMFMfCLU5af1VrlYBxZzB6U8nf6v8zLcKeGVJ9Hc/KA+xcE7PkmY46xmJARPAsm81LR5x84Mxf5lyBsczZdDdUDHWWyQ/TXVIt5T72bMVB01j/4RihyIZk9eKUkgSSFGrC3xeOtH0kNIkbnGdpgYB+4BTdRhzbbZHDQIcyDTCBuTS/lt9BitGUe6doeKuzMfbnOO1zApU3465vkctfniQ==");
                        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            responseBody = await response.Content.ReadAsStringAsync();

                            // Move the file to failed path
                            string todayFolder = Path.Combine(RefundprocessedFilePath, DateTime.Now.ToString("yyyyMMdd"));
                            if (!Directory.Exists(todayFolder))
                            {
                                Directory.CreateDirectory(todayFolder);
                            }
                            string processedFileName = Path.Combine(todayFolder, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + filename);
                            Directory.Move(FatoraBatchFile, processedFileName);

                            // Write request and response to the new folder
                            string requestFilePath = Path.Combine(todayFolder, "Request_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml");
                            File.WriteAllText(requestFilePath, fullXml);

                            string responseFilePath = Path.Combine(todayFolder, "Response_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
                            File.WriteAllText(responseFilePath, responseBody);
                        }
                        else
                        {
                            responseBody = await response.Content.ReadAsStringAsync();

                            // Move the file to failed path
                            string todayFolder = Path.Combine(RefundfailedFilePath, DateTime.Now.ToString("yyyyMMdd"));
                            if (!Directory.Exists(todayFolder))
                            {
                                Directory.CreateDirectory(todayFolder);
                            }
                            string processedFileName = Path.Combine(todayFolder, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + filename);
                            Directory.Move(FatoraBatchFile, processedFileName);

                            // Write request and response to the new folder
                            string requestFilePath = Path.Combine(todayFolder, "Request_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml");
                            File.WriteAllText(requestFilePath, fullXml);

                            string responseFilePath = Path.Combine(todayFolder, "Response_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
                            File.WriteAllText(responseFilePath, responseBody);
                        }
                    }
                }
                return responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}